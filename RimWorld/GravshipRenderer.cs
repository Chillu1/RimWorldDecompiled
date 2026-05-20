using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class GravshipRenderer
	{
		private Gravship gravship;

		private MaterialPropertyBlock distortionBlock;

		private MaterialPropertyBlock flareBlock;

		private MaterialPropertyBlock thrusterFlameBlock;

		private static EventQueue manualTicker;

		private DrawBatch drawBatch = new DrawBatch();

		private FleckSystem exhaustFleckSystem;

		private Dictionary<Thing, EventQueue> exhaustTimers = new Dictionary<Thing, EventQueue>();

		private Vector3 gravshipOffset;

		private Vector3 takeoffOrLandingPosition;

		private Vector3 takeoffOrLandingEnginePos;

		private Rot4 landingRotation;

		private Map map;

		private const float GravFieldGlowSize = 8f;

		private const float EngineGlowSize = 12.5f;

		private const float GravshipMoveSpeed = 25f;

		private const float ThrusterFlickerSpeed = 100f;

		private const float ThrusterMinBrightness = 0.75f;

		private const int ThrusterFlameRenderQueue = 3201;

		private static readonly int ShaderPropertyColor2 = Shader.PropertyToID("_Color2");

		private static readonly int ShaderPropertyGravshipHeight = Shader.PropertyToID("_GravshipHeight");

		private static readonly int ShaderPropertyIsTakeoff = Shader.PropertyToID("_IsTakeoff");

		private static readonly Material MatGravship = MatLoader.LoadMat("Map/Gravship/Gravship");

		private static readonly Material MatGravshipShadow = MatLoader.LoadMat("Map/Gravship/GravshipShadow");

		private static readonly Material MatGravshipDownwash = MatLoader.LoadMat("Map/Gravship/GravshipDownwash");

		private static readonly Material MatGravshipDistortion = MatLoader.LoadMat("Map/Gravship/GravshipDistortion");

		private static readonly Material MatGravshipLensFlare = MatLoader.LoadMat("Map/Gravship/GravshipLensFlare");

		private static readonly Material MatGravFieldExtenderGlow = MatLoader.LoadMat("Map/Gravship/GravFieldExtenderGlow");

		private static readonly Material MatGravEngineGlow = MatLoader.LoadMat("Map/Gravship/GravEngineGlow");

		public static readonly Material MatTerrainCurtain = MatLoader.LoadMat("Map/Gravship/FakeTerrain");

		public GravshipRenderer()
		{
			manualTicker = new EventQueue(1f / 60f);
		}

		public void Init(Map map)
		{
			if (ModLister.CheckOdyssey("Gravship"))
			{
				this.map = map;
				exhaustFleckSystem = new FleckSystemThrown(map.flecks);
				flareBlock = new MaterialPropertyBlock();
				distortionBlock = new MaterialPropertyBlock();
				thrusterFlameBlock = new MaterialPropertyBlock();
			}
		}

		public void BeginCutscene(Gravship gravship, Vector3 takeoffOrLandingCenter, Vector3 takeoffOrLandingEnginePos, Rot4 landingRotation)
		{
			gravshipOffset = Vector3.zero;
			this.gravship = gravship;
			takeoffOrLandingPosition = takeoffOrLandingCenter;
			this.takeoffOrLandingEnginePos = takeoffOrLandingEnginePos;
			this.landingRotation = landingRotation;
			exhaustFleckSystem.RemoveAllFlecks((IFleck _) => true);
			exhaustTimers.Clear();
			foreach (Thing thruster in gravship.Thrusters)
			{
				if (thruster.TryGetComp(out CompGravshipThruster comp) && comp.Props.exhaustSettings != null && comp.Props.exhaustSettings.enabled && comp.Props.exhaustSettings.ExhaustFleckDef != null)
				{
					exhaustFleckSystem.handledDefs.AddUnique(comp.Props.exhaustSettings.ExhaustFleckDef);
					exhaustTimers.Add(thruster, new EventQueue(1f / comp.Props.exhaustSettings.emissionsPerSecond));
				}
			}
		}

		public void BeginUpdate()
		{
			exhaustFleckSystem.parent = Find.CurrentMap.flecks;
			manualTicker.Push(Time.deltaTime);
			while (manualTicker.Pop())
			{
				exhaustFleckSystem.Tick();
			}
			exhaustFleckSystem.Update(Time.deltaTime);
		}

		public void UpdateTakeoff(float progress)
		{
			float num = Mathf.Pow(progress, 3.5f);
			Vector3 vector = new Vector3(0f, 0f, Find.Camera.orthographicSize * 2.5f * num);
			Vector3 normalized = gravship.launchDirection.ToVector3().normalized;
			if (Mathf.Approximately(normalized.z, -1f))
			{
				vector = -vector;
			}
			float num2 = ((Mathf.Approximately(normalized.x, 1f) || Mathf.Approximately(normalized.z, -1f)) ? (-1f) : 1f);
			float y = Mathf.SmoothStep(0f, num2 * 20f, Mathf.InverseLerp(0.2f, 1f, progress + 0.02f * Mathf.Sin(23.561945f * progress)));
			Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
			gravshipOffset += quaternion * normalized * (25f * Time.deltaTime * Mathf.InverseLerp(0.2f, 1f, progress));
			DrawGravshipGroundEffects(gravship.capture, Find.Camera.transform.position, takeoffOrLandingPosition + gravshipOffset, quaternion, progress, num, isTakeoff: true);
			DrawGravship(gravship.capture, Find.Camera.transform.position, takeoffOrLandingPosition + gravshipOffset + vector, takeoffOrLandingEnginePos + gravshipOffset + vector, quaternion, progress, num, isTakeoff: true);
		}

		public void UpdateLanding(float progress, bool isPollutedLanding)
		{
			progress = progress.RemapClamped(0f, 0.95f, 0f, 1f);
			float num = Mathf.Pow(1f - progress, 5f);
			Vector3 vector;
			Vector3 vector2;
			if (landingRotation == Rot4.North || landingRotation == Rot4.South)
			{
				vector = new Vector3(0f, 0f, 100f * num);
				vector2 = landingRotation.AsQuat * -gravship.launchDirection.ToVector3().normalized * 200f * num;
			}
			else
			{
				vector = new Vector3(0f, 0f, 200f * num);
				vector2 = landingRotation.AsQuat * -gravship.launchDirection.ToVector3().normalized * 100f * Mathf.Pow(1f - progress, 9f);
			}
			Quaternion identity = Quaternion.identity;
			DrawGravshipGroundEffects(gravship.capture, Find.Camera.transform.position, takeoffOrLandingPosition + vector2, identity, progress, num, isTakeoff: false, landingRotation);
			DrawGravship(gravship.capture, Find.Camera.transform.position, takeoffOrLandingPosition + vector + vector2, takeoffOrLandingEnginePos + vector + vector2, identity, progress, num, isTakeoff: false, landingRotation, isPollutedLanding);
		}

		public void EndUpdate()
		{
			exhaustFleckSystem.ForceDraw(drawBatch);
			drawBatch.Flush();
		}

		private void DrawGravship(Capture capture, Vector3 cameraPosition, Vector3 gravshipCenter, Vector3 gravEnginePos, Quaternion gravshipRotation, float cutsceneProgressPercent, float cutsceneHeightPercent, bool isTakeoff, Rot4 landingRotation = default(Rot4), bool isPollutedLanding = false)
		{
			Vector3 position = gravEnginePos + gravship.Engine.def.Size.ToVector3() * 0.25f;
			Vector3 vector = Find.Camera.WorldToViewportPoint(RotateAroundPivot(position, gravshipCenter, gravshipRotation));
			distortionBlock.SetFloat(ShaderPropertyIDs.Progress, cutsceneProgressPercent);
			distortionBlock.SetFloat(ShaderPropertyGravshipHeight, cutsceneHeightPercent);
			distortionBlock.SetVector(ShaderPropertyIDs.DrawPos, vector);
			distortionBlock.SetFloat(ShaderPropertyIsTakeoff, isTakeoff ? 1f : 0f);
			DrawLayer(MatGravshipDistortion, cameraPosition.SetToAltitude(AltitudeLayer.Weather).WithYOffset(0.07317074f), distortionBlock);
			MatGravship.SetFloat(ShaderPropertyIDs.Progress, cutsceneProgressPercent);
			MatGravship.SetFloat(ShaderPropertyGravshipHeight, cutsceneHeightPercent);
			MatGravship.SetFloat(ShaderPropertyIsTakeoff, isTakeoff ? 1f : 0f);
			MatGravship.color = (isPollutedLanding ? Color.white.WithAlpha(cutsceneProgressPercent.Remap(0.9f, 1f, 1f, 0f)) : Color.white);
			MatGravship.mainTexture = (Texture2D)capture.capture;
			GenDraw.DrawQuad(MatGravship, gravshipCenter.SetToAltitude(AltitudeLayer.Skyfaller), gravshipRotation, capture.drawSize);
			foreach (LayerSubMesh bakedIndoorMask in gravship.bakedIndoorMasks)
			{
				Graphics.DrawMesh(bakedIndoorMask.mesh, Matrix4x4.TRS(gravshipCenter + Altitudes.AltIncVect * 2f, gravshipRotation, Vector3.one), bakedIndoorMask.material, 0);
			}
			if ((isTakeoff && cutsceneProgressPercent <= 0f) || (!isTakeoff && cutsceneProgressPercent >= 1f))
			{
				return;
			}
			Color value = new Color(1f, 1f, 1f, 1f);
			value *= Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D(cutsceneProgressPercent * 100f));
			value.a = Mathf.InverseLerp(0f, 0.1f, isTakeoff ? cutsceneProgressPercent : (1f - cutsceneProgressPercent));
			MatGravshipLensFlare.SetColor(ShaderPropertyColor2, value);
			foreach (KeyValuePair<Thing, PositionData.Data> thrusterPlacement in gravship.ThrusterPlacements)
			{
				thrusterPlacement.Deconstruct(out var key, out var value2);
				Thing thing = key;
				PositionData.Data data = value2;
				IntVec3 intVec = -gravship.launchDirection;
				Rot4 rotation = data.rotation;
				if (rotation.AsIntVec3 == intVec)
				{
					continue;
				}
				CompProperties_GravshipThruster props = thing.TryGetComp<CompGravshipThruster>().Props;
				float num = (float)thing.def.size.x * props.flameSize;
				Vector3 vector2 = thing.Rotation.AsQuat * props.flameOffsetsPerDirection[thing.Rotation.AsInt];
				Vector3 vector3 = GenThing.TrueCenter(data.local, thing.Rotation, thing.def.size, 0f) - thing.Rotation.AsIntVec3.ToVector3() * ((float)thing.def.size.z * 0.5f + num * 0.5f) + vector2;
				Vector3 position2 = RotateAroundPivot(gravEnginePos + vector3, gravshipCenter, gravshipRotation).SetToAltitude(AltitudeLayer.Skyfaller).WithYOffset(0.07317074f);
				MaterialRequest req = new MaterialRequest(props.FlameShaderType.Shader);
				req.renderQueue = 3201;
				Material mat = MaterialPool.MatFrom(req);
				thrusterFlameBlock.Clear();
				thrusterFlameBlock.SetColor(ShaderPropertyColor2, value);
				foreach (ShaderParameter flameShaderParameter in props.flameShaderParameters)
				{
					flameShaderParameter.Apply(thrusterFlameBlock);
				}
				rotation = data.rotation;
				GenDraw.DrawQuad(mat, position2, gravshipRotation * rotation.AsQuat, num, thrusterFlameBlock);
				Vector3 vector4 = Find.Camera.WorldToViewportPoint(position2);
				flareBlock.SetVector(ShaderPropertyIDs.DrawPos, vector4);
				DrawLayer(MatGravshipLensFlare, cameraPosition.SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.03658537f), flareBlock);
				if (props.exhaustSettings.enabled)
				{
					EventQueue eventQueue = exhaustTimers[thing];
					eventQueue.Push(Time.deltaTime);
					while (eventQueue.Pop())
					{
						CompProperties_GravshipThruster.ExhaustSettings exhaustSettings = props.exhaustSettings;
						rotation = data.rotation;
						EmitSmoke(exhaustSettings, position2, gravshipRotation, rotation.AsQuat);
					}
				}
			}
			MatGravFieldExtenderGlow.SetColor(ShaderPropertyColor2, value);
			foreach (IntVec3 gravFieldExtenderPosition in gravship.GravFieldExtenderPositions)
			{
				Vector3 vector5 = gravFieldExtenderPosition.ToVector3() + ThingDefOf.GravFieldExtender.graphicData.drawSize.ToVector3() * 0.5f;
				Vector3 position3 = RotateAroundPivot(gravEnginePos + vector5, gravshipCenter, gravshipRotation).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
				GenDraw.DrawQuad(MatGravFieldExtenderGlow, position3, Quaternion.identity, 8f);
			}
			MatGravEngineGlow.SetColor(ShaderPropertyColor2, value);
			Vector3 position4 = RotateAroundPivot(position, gravshipCenter, gravshipRotation).SetToAltitude(AltitudeLayer.MetaOverlays).WithYOffset(0.07317074f);
			GenDraw.DrawQuad(MatGravEngineGlow, position4, Quaternion.identity, 12.5f);
		}

		private void DrawGravshipGroundEffects(Capture capture, Vector3 cameraCenter, Vector3 groundCenter, Quaternion rotation, float progress, float height, bool isTakeoff, Rot4 landingRotation = default(Rot4))
		{
			if (progress > 0f && !map.Biome.inVacuum)
			{
				MatGravshipDownwash.SetFloat(ShaderPropertyIDs.Progress, progress);
				MatGravshipDownwash.SetFloat(ShaderPropertyGravshipHeight, height);
				MatGravshipDownwash.SetVector(ShaderPropertyIDs.DrawPos, Find.Camera.WorldToViewportPoint(groundCenter));
				MatGravshipDownwash.SetFloat(ShaderPropertyIsTakeoff, isTakeoff ? 1f : 0f);
				DrawLayer(MatGravshipDownwash, cameraCenter.SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f));
			}
			MatGravshipShadow.SetFloat(ShaderPropertyIDs.Progress, 1f - progress);
			MatGravshipShadow.SetFloat(ShaderPropertyGravshipHeight, height);
			MatGravshipShadow.SetFloat(ShaderPropertyIsTakeoff, isTakeoff ? 1f : 0f);
			MatGravshipShadow.color = MatGravshipShadow.color.WithAlpha(progress.RemapClamped(0.9f, 1f, 1f, 0f));
			MatGravshipShadow.mainTexture = (Texture2D)capture.capture;
			GenDraw.DrawQuad(MatGravshipShadow, groundCenter.SetToAltitude(AltitudeLayer.Gas).WithYOffset(0.03658537f), rotation, capture.drawSize * 1.05f);
		}

		private void EmitSmoke(CompProperties_GravshipThruster.ExhaustSettings settings, Vector3 position, Quaternion gravshipRotation, Quaternion thrusterRotation)
		{
			Quaternion quaternion = Quaternion.identity;
			if (settings.inheritThrusterRotation)
			{
				quaternion = thrusterRotation * quaternion;
			}
			if (settings.inheritGravshipRotation)
			{
				quaternion = gravshipRotation * quaternion;
			}
			exhaustFleckSystem.CreateFleck(new FleckCreationData
			{
				def = settings.ExhaustFleckDef,
				spawnPosition = position + quaternion * settings.spawnOffset + Random.insideUnitSphere.WithY(0f).normalized * settings.spawnRadiusRange.RandomInRange,
				scale = settings.scaleRange.RandomInRange,
				velocity = quaternion * Quaternion.Euler(0f, settings.velocityRotationRange.RandomInRange, 0f) * (settings.velocity * settings.velocityMultiplierRange.RandomInRange),
				rotationRate = settings.rotationOverTimeRange.RandomInRange,
				ageTicksOverride = -1
			});
		}

		private Vector3 RotateAroundPivot(Vector3 position, Vector3 pivot, Quaternion rotation)
		{
			return rotation * (position - pivot) + pivot;
		}

		private void DrawLayer(Material mat, Vector3 position)
		{
			DrawLayer(mat, position, Quaternion.identity);
		}

		private void DrawLayer(Material mat, Vector3 position, Quaternion rotation)
		{
			float num = Find.Camera.orthographicSize * 2f;
			Vector3 s = new Vector3(num * Find.Camera.aspect, 1f, num);
			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		private void DrawLayer(Material mat, Vector3 position, MaterialPropertyBlock props)
		{
			float num = Find.Camera.orthographicSize * 2f;
			Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(num * Find.Camera.aspect, 1f, num), pos: position, q: Quaternion.identity);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0, null, 0, props);
		}
	}
}
