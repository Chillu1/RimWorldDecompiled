using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Skyfaller : Thing, IThingHolder
	{
		public ThingOwner innerContainer;

		public int ticksToImpact;

		public float angle;

		public float shrapnelDirection;

		private int ticksToImpactMax = 220;

		private Material cachedShadowMaterial;

		private bool anticipationSoundPlayed;

		private static MaterialPropertyBlock shadowPropertyBlock = new MaterialPropertyBlock();

		public const float DefaultAngle = -33.7f;

		private const int RoofHitPreDelay = 15;

		private const int LeaveMapAfterTicks = 220;

		public override Graphic Graphic
		{
			get
			{
				Thing thingForGraphic = GetThingForGraphic();
				if (thingForGraphic == this)
				{
					return base.Graphic;
				}
				return thingForGraphic.Graphic.ExtractInnerGraphicFor(thingForGraphic).GetShadowlessGraphic();
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				switch (def.skyfaller.movementType)
				{
				case SkyfallerMovementType.Accelerate:
					return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed);
				case SkyfallerMovementType.ConstantSpeed:
					return SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, ticksToImpact, angle, CurrentSpeed);
				case SkyfallerMovementType.Decelerate:
					return SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed);
				default:
					Log.ErrorOnce("SkyfallerMovementType not handled: " + def.skyfaller.movementType, thingIDNumber ^ 0x7424EBC7);
					return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpact, angle, CurrentSpeed);
				}
			}
		}

		private Material ShadowMaterial
		{
			get
			{
				if (cachedShadowMaterial == null && !def.skyfaller.shadow.NullOrEmpty())
				{
					cachedShadowMaterial = MaterialPool.MatFrom(def.skyfaller.shadow, ShaderDatabase.Transparent);
				}
				return cachedShadowMaterial;
			}
		}

		private float TimeInAnimation
		{
			get
			{
				if (def.skyfaller.reversed)
				{
					return (float)ticksToImpact / 220f;
				}
				return 1f - (float)ticksToImpact / (float)ticksToImpactMax;
			}
		}

		private float CurrentSpeed
		{
			get
			{
				if (def.skyfaller.speedCurve == null)
				{
					return def.skyfaller.speed;
				}
				return def.skyfaller.speedCurve.Evaluate(TimeInAnimation) * def.skyfaller.speed;
			}
		}

		private bool SpawnTimedMotes
		{
			get
			{
				if (def.skyfaller.moteSpawnTime == float.MinValue)
				{
					return false;
				}
				return Mathf.Approximately(def.skyfaller.moteSpawnTime, TimeInAnimation);
			}
		}

		public Skyfaller()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref ticksToImpact, "ticksToImpact", 0);
			Scribe_Values.Look(ref ticksToImpactMax, "ticksToImpactMax", 220);
			Scribe_Values.Look(ref angle, "angle", 0f);
			Scribe_Values.Look(ref shrapnelDirection, "shrapnelDirection", 0f);
		}

		public override void PostMake()
		{
			base.PostMake();
			if (def.skyfaller.MakesShrapnel)
			{
				shrapnelDirection = Rand.Range(0f, 360f);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (respawningAfterLoad)
			{
				return;
			}
			ticksToImpact = (ticksToImpactMax = def.skyfaller.ticksToImpactRange.RandomInRange);
			if (def.skyfaller.MakesShrapnel)
			{
				float num = GenMath.PositiveMod(shrapnelDirection, 360f);
				if (num < 270f && num >= 90f)
				{
					angle = Rand.Range(0f, 33f);
				}
				else
				{
					angle = Rand.Range(-33f, 0f);
				}
			}
			else if (def.skyfaller.angleCurve != null)
			{
				angle = def.skyfaller.angleCurve.Evaluate(0f);
			}
			else
			{
				angle = -33.7f;
			}
			if (def.rotatable && innerContainer.Any)
			{
				base.Rotation = innerContainer[0].Rotation;
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			innerContainer.ClearAndDestroyContents();
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Thing thingForGraphic = GetThingForGraphic();
			float num = 0f;
			if (def.skyfaller.rotateGraphicTowardsDirection)
			{
				num = angle;
			}
			if (def.skyfaller.angleCurve != null)
			{
				angle = def.skyfaller.angleCurve.Evaluate(TimeInAnimation);
			}
			if (def.skyfaller.rotationCurve != null)
			{
				num += def.skyfaller.rotationCurve.Evaluate(TimeInAnimation);
			}
			if (def.skyfaller.xPositionCurve != null)
			{
				drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
			}
			if (def.skyfaller.zPositionCurve != null)
			{
				drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
			}
			Graphic.Draw(drawLoc, flip ? thingForGraphic.Rotation.Opposite : thingForGraphic.Rotation, thingForGraphic, num);
			DrawDropSpotShadow();
		}

		public override void Tick()
		{
			innerContainer.ThingOwnerTick();
			if (SpawnTimedMotes)
			{
				CellRect cellRect = this.OccupiedRect();
				for (int i = 0; i < cellRect.Area * def.skyfaller.motesPerCell; i++)
				{
					MoteMaker.ThrowDustPuff(cellRect.RandomVector3, base.Map, 2f);
				}
			}
			if (def.skyfaller.reversed)
			{
				ticksToImpact++;
				if (!anticipationSoundPlayed && def.skyfaller.anticipationSound != null && ticksToImpact > def.skyfaller.anticipationSoundTicks)
				{
					anticipationSoundPlayed = true;
					def.skyfaller.anticipationSound.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				if (ticksToImpact == 220)
				{
					LeaveMap();
				}
				else if (ticksToImpact > 220)
				{
					Log.Error("ticksToImpact > LeaveMapAfterTicks. Was there an exception? Destroying skyfaller.");
					Destroy();
				}
				return;
			}
			ticksToImpact--;
			if (ticksToImpact == 15)
			{
				HitRoof();
			}
			if (!anticipationSoundPlayed && def.skyfaller.anticipationSound != null && ticksToImpact < def.skyfaller.anticipationSoundTicks)
			{
				anticipationSoundPlayed = true;
				def.skyfaller.anticipationSound.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			if (ticksToImpact == 0)
			{
				Impact();
			}
			else if (ticksToImpact < 0)
			{
				Log.Error("ticksToImpact < 0. Was there an exception? Destroying skyfaller.");
				Destroy();
			}
		}

		protected virtual void HitRoof()
		{
			if (!def.skyfaller.hitRoof)
			{
				return;
			}
			CellRect cr = this.OccupiedRect();
			if (cr.Cells.Any((IntVec3 x) => x.Roofed(base.Map)))
			{
				RoofDef roof = cr.Cells.First((IntVec3 x) => x.Roofed(base.Map)).GetRoof(base.Map);
				if (!roof.soundPunchThrough.NullOrUndefined())
				{
					roof.soundPunchThrough.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				RoofCollapserImmediate.DropRoofInCells(cr.ExpandedBy(1).ClipInsideMap(base.Map).Cells.Where(delegate(IntVec3 c)
				{
					if (!c.InBounds(base.Map))
					{
						return false;
					}
					if (cr.Contains(c))
					{
						return true;
					}
					if (c.GetFirstPawn(base.Map) != null)
					{
						return false;
					}
					Building edifice = c.GetEdifice(base.Map);
					return (edifice == null || !edifice.def.holdsRoof) ? true : false;
				}), base.Map);
			}
		}

		protected virtual void SpawnThings()
		{
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				GenPlace.TryPlaceThing(innerContainer[num], base.Position, base.Map, ThingPlaceMode.Near, delegate(Thing thing, int count)
				{
					PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
					if (thing.def.Fillage == FillCategory.Full && def.skyfaller.CausesExplosion && def.skyfaller.explosionDamage.isExplosive && thing.Position.InHorDistOf(base.Position, def.skyfaller.explosionRadius))
					{
						base.Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
					}
				}, null, innerContainer[num].def.defaultPlacingRot);
			}
		}

		protected virtual void Impact()
		{
			if (def.skyfaller.CausesExplosion)
			{
				GenExplosion.DoExplosion(base.Position, base.Map, def.skyfaller.explosionRadius, def.skyfaller.explosionDamage, null, GenMath.RoundRandom((float)def.skyfaller.explosionDamage.defaultDamage * def.skyfaller.explosionDamageFactor), -1f, null, null, null, null, null, 0f, 1, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, (!def.skyfaller.damageSpawnedThings) ? innerContainer.ToList() : null);
			}
			SpawnThings();
			innerContainer.ClearAndDestroyContents();
			CellRect cellRect = this.OccupiedRect();
			for (int i = 0; i < cellRect.Area * def.skyfaller.motesPerCell; i++)
			{
				MoteMaker.ThrowDustPuff(cellRect.RandomVector3, base.Map, 2f);
			}
			if (def.skyfaller.MakesShrapnel)
			{
				SkyfallerShrapnelUtility.MakeShrapnel(base.Position, base.Map, shrapnelDirection, def.skyfaller.shrapnelDistanceFactor, def.skyfaller.metalShrapnelCountRange.RandomInRange, def.skyfaller.rubbleShrapnelCountRange.RandomInRange, spawnMotes: true);
			}
			if (def.skyfaller.cameraShake > 0f && base.Map == Find.CurrentMap)
			{
				Find.CameraDriver.shaker.DoShake(def.skyfaller.cameraShake);
			}
			if (def.skyfaller.impactSound != null)
			{
				def.skyfaller.impactSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
			}
			Destroy();
		}

		protected virtual void LeaveMap()
		{
			Destroy();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		private Thing GetThingForGraphic()
		{
			if (def.graphicData != null || !innerContainer.Any)
			{
				return this;
			}
			return innerContainer[0];
		}

		private void DrawDropSpotShadow()
		{
			Material shadowMaterial = ShadowMaterial;
			if (!(shadowMaterial == null))
			{
				DrawDropSpotShadow(base.DrawPos, base.Rotation, shadowMaterial, def.skyfaller.shadowSize, ticksToImpact);
			}
		}

		public static void DrawDropSpotShadow(Vector3 center, Rot4 rot, Material material, Vector2 shadowSize, int ticksToImpact)
		{
			if (rot.IsHorizontal)
			{
				Gen.Swap(ref shadowSize.x, ref shadowSize.y);
			}
			ticksToImpact = Mathf.Max(ticksToImpact, 0);
			Vector3 pos = center;
			pos.y = AltitudeLayer.Shadows.AltitudeFor();
			float num = 1f + (float)ticksToImpact / 100f;
			Vector3 s = new Vector3(num * shadowSize.x, 1f, num * shadowSize.y);
			Color white = Color.white;
			if (ticksToImpact > 150)
			{
				white.a = Mathf.InverseLerp(200f, 150f, ticksToImpact);
			}
			shadowPropertyBlock.SetColor(ShaderPropertyIDs.Color, white);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, rot.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10Back, matrix, material, 0, null, 0, shadowPropertyBlock);
		}
	}
}
