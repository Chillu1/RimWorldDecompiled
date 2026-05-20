using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace RimWorld.Planet;

public class WorldCameraDriver : MonoBehaviour
{
	public WorldCameraConfig config = new WorldCameraConfig_Normal();

	public Quaternion sphereRotation = Quaternion.identity;

	private Vector2 rotationVelocity;

	private Vector2 desiredRotation;

	private Vector2 desiredRotationRaw;

	private float desiredAltitude;

	public float altitude;

	private List<CameraDriver.DragTimeStamp> dragTimeStamps = new List<CameraDriver.DragTimeStamp>();

	private bool releasedLeftWhileHoldingMiddle;

	private float layerAltitudeOffset;

	private float layerOffsetVel;

	private Vector3 layerOriginOffset;

	private Vector3 layerOriginOffsetVel;

	private float altitudeDecay;

	private float altitudeDecayVel;

	private PlanetLayer prevSelectedLayer;

	private Camera cachedCamera;

	private bool mouseCoveredByUI;

	private float mouseTouchingScreenBottomEdgeStartTime = -1f;

	private float fixedTimeStepBuffer;

	private Quaternion rotationAnimation_prevSphereRotation = Quaternion.identity;

	private float rotationAnimation_lerpFactor = 1f;

	private const float SphereRadius = 100f;

	private const float ScreenDollyEdgeWidth = 20f;

	private const float ScreenDollyEdgeWidth_BottomFullscreen = 6f;

	private const float MinDurationForMouseToTouchScreenBottomEdgeToDolly = 0.28f;

	private const float MaxXRotationAtMinAltitude = 88.6f;

	private const float MaxXRotationAtMaxAltitude = 78f;

	private const float TileSizeToRotationSpeed = 0.273f;

	private const float VelocityFromMouseDragInitialFactor = 5f;

	private const float StartingAltitude_Playing = 160f;

	private const float StartingAltitude_Entry = 550f;

	private const float MaxAltitude = 1100f;

	private const float ZoomTightness = 0.4f;

	private const float ZoomScaleFromAltDenominator = 12f;

	private const float PageKeyZoomRate = 2f;

	private const float ScrollWheelZoomRate = 0.1f;

	private PlanetLayer lastSelectedLayer;

	public static float MinAltitude => 100f + (SteamDeck.IsSteamDeck ? 17f : 25f);

	public float TrueAltitude => layerAltitudeOffset + altitude;

	private Camera MyCamera
	{
		get
		{
			if (cachedCamera == null)
			{
				cachedCamera = GetComponent<Camera>();
			}
			return cachedCamera;
		}
	}

	public WorldCameraZoomRange CurrentZoom
	{
		get
		{
			float altitudePercent = AltitudePercent;
			if (altitudePercent < 0.025f)
			{
				return WorldCameraZoomRange.VeryClose;
			}
			if (altitudePercent < 0.042f)
			{
				return WorldCameraZoomRange.Close;
			}
			if (altitudePercent < 0.125f)
			{
				return WorldCameraZoomRange.Far;
			}
			return WorldCameraZoomRange.VeryFar;
		}
	}

	private float ScreenDollyEdgeWidthBottom
	{
		get
		{
			if (Screen.fullScreen || ResolutionUtility.BorderlessFullscreen)
			{
				return 6f;
			}
			return 20f;
		}
	}

	public Vector3 CameraPosition => MyCamera.transform.position;

	public float AltitudePercent => Mathf.InverseLerp(MinAltitude, 1100f, altitude);

	public Vector3 CurrentlyLookingAtPointOnSphere => -(Quaternion.Inverse(sphereRotation) * Vector3.forward);

	private bool AnythingPreventsCameraMotion
	{
		get
		{
			if (!Find.WindowStack.WindowsPreventCameraMotion)
			{
				return !WorldRendererUtility.WorldSelected;
			}
			return true;
		}
	}

	public void Awake()
	{
		ResetAltitude();
		ApplyPositionToGameObject();
	}

	public void WorldCameraDriverOnGUI()
	{
		if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(2))
		{
			releasedLeftWhileHoldingMiddle = true;
		}
		else if (Event.current.rawType == EventType.MouseDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
		{
			releasedLeftWhileHoldingMiddle = false;
		}
		mouseCoveredByUI = false;
		if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
		{
			mouseCoveredByUI = true;
		}
		if (WorldRendererUtility.WorldBackgroundNow)
		{
			ApplyPositionToGameObject();
		}
		else
		{
			if (Find.World == null)
			{
				return;
			}
			if (prevSelectedLayer != null && Find.WorldSelector.SelectedLayer != prevSelectedLayer)
			{
				altitudeDecay = 0f;
			}
			if (AnythingPreventsCameraMotion)
			{
				return;
			}
			if (UnityGUIBugsFixer.MouseDrag(2) && (!SteamDeck.IsSteamDeck || !Find.WorldSelector.AnyCaravanSelected))
			{
				Vector2 currentEventDelta = UnityGUIBugsFixer.CurrentEventDelta;
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
				}
				if (currentEventDelta != Vector2.zero)
				{
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.FrameInteraction);
					currentEventDelta.x *= -1f;
					desiredRotationRaw += currentEventDelta / GenWorldUI.CurUITileSize() * (0.273f * Prefs.MapDragSensitivity);
				}
			}
			float num = 0f;
			if (Event.current.type == EventType.ScrollWheel)
			{
				num -= Event.current.delta.y * 0.1f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			if (KeyBindingDefOf.MapZoom_In.KeyDownEvent)
			{
				num += 2f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			if (KeyBindingDefOf.MapZoom_Out.KeyDownEvent)
			{
				num -= 2f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			float num2 = desiredAltitude - num * config.zoomSpeed * TrueAltitude / 12f;
			if (Current.ProgramState == ProgramState.Playing && Prefs.ZoomSwitchWorldLayer)
			{
				if (num2 < MinAltitude)
				{
					PlanetLayer planetLayer = PlanetLayer.Selected?.zoomInToLayer;
					if (planetLayer != null)
					{
						float num3 = layerAltitudeOffset + num2 - planetLayer.ExtraCameraAltitude;
						num2 = (altitude = num3);
						altitudeDecay = planetLayer.ExtraCameraAltitude - layerAltitudeOffset;
						PlanetLayer.Selected = planetLayer;
					}
				}
				else if (num2 > 1100f)
				{
					PlanetLayer planetLayer2 = PlanetLayer.Selected?.zoomOutToLayer;
					if (planetLayer2 != null)
					{
						float num4 = layerAltitudeOffset + num2 - planetLayer2.ExtraCameraAltitude;
						num2 = (altitude = num4);
						altitudeDecay = planetLayer2.ExtraCameraAltitude - layerAltitudeOffset;
						PlanetLayer.Selected = planetLayer2;
					}
				}
			}
			desiredAltitude = Mathf.Clamp(num2, MinAltitude, 1100f);
			desiredRotation = Vector2.zero;
			if (KeyBindingDefOf.MapDolly_Left.IsDown)
			{
				desiredRotation.x = 0f - config.dollyRateKeys;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			if (KeyBindingDefOf.MapDolly_Right.IsDown)
			{
				desiredRotation.x = config.dollyRateKeys;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			if (KeyBindingDefOf.MapDolly_Up.IsDown)
			{
				desiredRotation.y = config.dollyRateKeys;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			if (KeyBindingDefOf.MapDolly_Down.IsDown)
			{
				desiredRotation.y = 0f - config.dollyRateKeys;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.SpecificInteraction);
			}
			config.ConfigOnGUI();
			prevSelectedLayer = Find.WorldSelector.SelectedLayer;
		}
	}

	public void Update()
	{
		if (LongEventHandler.ShouldWaitForEvent)
		{
			return;
		}
		if (Find.World == null)
		{
			MyCamera.gameObject.SetActive(value: false);
			return;
		}
		if (!Find.WorldInterface.everReset)
		{
			Find.WorldInterface.Reset();
		}
		if (WorldRendererUtility.WorldBackgroundNow)
		{
			ApplyMapPositionToGameObject();
			return;
		}
		Vector2 vector = CalculateCurInputDollyVect();
		if (vector != Vector2.zero)
		{
			float num = (altitude - MinAltitude) / (1100f - MinAltitude) * 0.85f + 0.15f;
			rotationVelocity = new Vector2(vector.x, vector.y) * num;
		}
		if ((Input.GetMouseButtonUp(2) || (SteamDeck.IsSteamDeck && releasedLeftWhileHoldingMiddle)) && dragTimeStamps.Any())
		{
			rotationVelocity += CameraDriver.GetExtraVelocityFromReleasingDragButton(dragTimeStamps, 5f);
			dragTimeStamps.Clear();
		}
		if (!AnythingPreventsCameraMotion)
		{
			float num2 = Time.deltaTime * CameraDriver.HitchReduceFactor;
			sphereRotation *= Quaternion.AngleAxis(rotationVelocity.x * num2 * config.rotationSpeedScale, MyCamera.transform.up);
			sphereRotation *= Quaternion.AngleAxis((0f - rotationVelocity.y) * num2 * config.rotationSpeedScale, MyCamera.transform.right);
			if (desiredRotationRaw != Vector2.zero)
			{
				sphereRotation *= Quaternion.AngleAxis(desiredRotationRaw.x, MyCamera.transform.up);
				sphereRotation *= Quaternion.AngleAxis(0f - desiredRotationRaw.y, MyCamera.transform.right);
			}
			dragTimeStamps.Add(new CameraDriver.DragTimeStamp
			{
				posDelta = desiredRotationRaw,
				time = Time.time
			});
		}
		desiredRotationRaw = Vector2.zero;
		int num3 = Gen.FixedTimeStepUpdate(ref fixedTimeStepBuffer, 60f);
		for (int i = 0; i < num3; i++)
		{
			if (rotationVelocity != Vector2.zero)
			{
				rotationVelocity *= config.camRotationDecayFactor;
				if (rotationVelocity.magnitude < 0.05f)
				{
					rotationVelocity = Vector2.zero;
				}
			}
			if (config.smoothZoom)
			{
				float num4 = Mathf.Lerp(altitude, desiredAltitude, 0.05f);
				desiredAltitude += (num4 - altitude) * config.zoomPreserveFactor;
				altitude = num4;
			}
			else
			{
				float num5 = (desiredAltitude - altitude) * 0.4f;
				desiredAltitude += config.zoomPreserveFactor * num5;
				altitude += num5;
			}
		}
		rotationAnimation_lerpFactor += Time.deltaTime * 8f;
		if (Find.PlaySettings.lockNorthUp)
		{
			RotateSoNorthIsUp(interpolate: false);
			ClampXRotation(ref sphereRotation);
		}
		for (int j = 0; j < num3; j++)
		{
			config.ConfigFixedUpdate_60(ref rotationVelocity);
		}
		ApplyPositionToGameObject();
	}

	public void ApplyMapPositionToGameObject()
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap == null)
		{
			return;
		}
		Vector3 vector = ((!(currentMap.ParentHolder is MapParent mapParent)) ? Find.WorldGrid.GetTileCenter(currentMap.Tile) : mapParent.WorldCameraPosition);
		if (vector == Vector3.zero)
		{
			return;
		}
		Vector3 vector2 = -vector.normalized;
		vector += -vector2 * currentMap.Tile.Layer.BackgroundWorldCameraOffset;
		Transform transform = MyCamera.transform;
		Quaternion rotation = Quaternion.LookRotation(vector2, Vector3.up);
		transform.rotation = rotation;
		float num = currentMap.Tile.Layer.BackgroundWorldCameraParallaxDistancePer100Cells;
		if (num == 0f)
		{
			transform.position = vector;
			return;
		}
		Vector2 viewSpacePosition = Find.CameraDriver.ViewSpacePosition;
		IntVec3 size = Find.CurrentMap.Size;
		float num2 = 1f;
		float num3 = 1f;
		if (size.x > size.z)
		{
			num3 = (float)size.z / (float)size.x;
			num = num * (float)size.x / 100f;
		}
		else if (size.z > size.x)
		{
			num2 = (float)size.x / (float)size.z;
			num = num * (float)size.z / 100f;
		}
		Vector3 up = transform.up;
		Vector3 right = transform.right;
		Vector3 vector3 = up * (viewSpacePosition.y * num * num3) - up * num / 2f * num3;
		Vector3 vector4 = right * (viewSpacePosition.x * num * num2) - right * num / 2f * num2;
		transform.position = vector + vector3 + vector4 + currentMap.Tile.Layer.Origin;
	}

	private void ApplyPositionToGameObject()
	{
		Quaternion invRot = ((!(rotationAnimation_lerpFactor < 1f)) ? sphereRotation : Quaternion.Lerp(rotationAnimation_prevSphereRotation, sphereRotation, rotationAnimation_lerpFactor));
		if (Find.PlaySettings.lockNorthUp)
		{
			ClampXRotation(ref invRot);
		}
		float target = 0f;
		Vector3 target2 = Vector3.zero;
		if (PlanetLayer.Selected != null)
		{
			target = PlanetLayer.Selected.ExtraCameraAltitude;
			target2 = PlanetLayer.Selected.Origin;
		}
		float num = layerAltitudeOffset;
		layerAltitudeOffset = Mathf.SmoothDamp(layerAltitudeOffset, target, ref layerOffsetVel, 0.1f);
		layerOriginOffset = Vector3.SmoothDamp(layerOriginOffset, target2, ref layerOriginOffsetVel, 0.1f);
		float num2 = layerAltitudeOffset - num;
		if (!Mathf.Approximately(altitudeDecay, 0f))
		{
			altitudeDecay -= num2;
		}
		else
		{
			altitudeDecay = 0f;
		}
		Transform obj = MyCamera.transform;
		obj.rotation = Quaternion.Inverse(invRot);
		Vector3 vector = obj.rotation * Vector3.forward;
		obj.position = -vector * (altitude + layerAltitudeOffset + altitudeDecay) + layerOriginOffset;
	}

	private Vector2 CalculateCurInputDollyVect()
	{
		Vector2 result = desiredRotation;
		bool flag = false;
		if ((UnityData.isEditor || Screen.fullScreen || ResolutionUtility.BorderlessFullscreen) && Prefs.EdgeScreenScroll && !mouseCoveredByUI)
		{
			Vector2 mousePositionOnUI = UI.MousePositionOnUI;
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			Rect rect = new Rect(UI.screenWidth - 250, 0f, 255f, 255f);
			Rect rect2 = new Rect(0f, UI.screenHeight - 250, 225f, 255f);
			Rect rect3 = new Rect(UI.screenWidth - 250, UI.screenHeight - 250, 255f, 255f);
			WorldInspectPane inspectPane = Find.World.UI.inspectPane;
			if (Find.WindowStack.IsOpen<WorldInspectPane>() && inspectPane.RecentHeight > rect2.height)
			{
				rect2.yMin = (float)UI.screenHeight - inspectPane.RecentHeight;
			}
			if (!rect2.Contains(mousePositionOnUIInverted) && !rect3.Contains(mousePositionOnUIInverted) && !rect.Contains(mousePositionOnUIInverted))
			{
				Vector2 zero = Vector2.zero;
				if (mousePositionOnUI.x >= 0f && mousePositionOnUI.x < 20f)
				{
					zero.x -= config.dollyRateScreenEdge;
				}
				if (mousePositionOnUI.x <= (float)UI.screenWidth && mousePositionOnUI.x > (float)UI.screenWidth - 20f)
				{
					zero.x += config.dollyRateScreenEdge;
				}
				if (mousePositionOnUI.y <= (float)UI.screenHeight && mousePositionOnUI.y > (float)UI.screenHeight - 20f)
				{
					zero.y += config.dollyRateScreenEdge;
				}
				if (mousePositionOnUI.y >= 0f && mousePositionOnUI.y < ScreenDollyEdgeWidthBottom)
				{
					if (mouseTouchingScreenBottomEdgeStartTime < 0f)
					{
						mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
					}
					if (Time.realtimeSinceStartup - mouseTouchingScreenBottomEdgeStartTime >= 0.28f)
					{
						zero.y -= config.dollyRateScreenEdge;
					}
					flag = true;
				}
				result += zero;
			}
		}
		if (!flag)
		{
			mouseTouchingScreenBottomEdgeStartTime = -1f;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			result *= 2.4f;
		}
		return result;
	}

	public void ResetAltitude()
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			altitude = 160f;
		}
		else
		{
			altitude = 550f;
		}
		desiredAltitude = altitude;
	}

	public void JumpTo(Vector3 newLookAt)
	{
		if (!Find.WorldInterface.everReset)
		{
			Find.WorldInterface.Reset();
		}
		if (newLookAt != Vector3.zero)
		{
			sphereRotation = Quaternion.Inverse(Quaternion.LookRotation(-newLookAt.normalized));
		}
	}

	public void JumpTo(PlanetTile tile)
	{
		JumpTo(Find.WorldGrid.GetTileCenter(tile));
		PlanetLayer.Selected = tile.Layer;
	}

	public void RotateSoNorthIsUp(bool interpolate = true)
	{
		if (interpolate)
		{
			rotationAnimation_prevSphereRotation = sphereRotation;
		}
		sphereRotation = Quaternion.Inverse(Quaternion.LookRotation(Quaternion.Inverse(sphereRotation) * Vector3.forward));
		if (interpolate)
		{
			rotationAnimation_lerpFactor = 0f;
		}
	}

	private void ClampXRotation(ref Quaternion invRot)
	{
		Vector3 eulerAngles = Quaternion.Inverse(invRot).eulerAngles;
		float altitudePercent = AltitudePercent;
		float num = Mathf.Lerp(88.6f, 78f, altitudePercent);
		bool flag = false;
		if (eulerAngles.x <= 90f)
		{
			if (eulerAngles.x > num)
			{
				eulerAngles.x = num;
				flag = true;
			}
		}
		else if (eulerAngles.x < 360f - num)
		{
			eulerAngles.x = 360f - num;
			flag = true;
		}
		if (flag)
		{
			invRot = Quaternion.Inverse(Quaternion.Euler(eulerAngles));
		}
	}
}
