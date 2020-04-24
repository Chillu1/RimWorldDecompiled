using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
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

		public const float MinAltitude = 125f;

		private const float MaxAltitude = 1100f;

		private const float ZoomTightness = 0.4f;

		private const float ZoomScaleFromAltDenominator = 12f;

		private const float PageKeyZoomRate = 2f;

		private const float ScrollWheelZoomRate = 0.1f;

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
				if (Screen.fullScreen)
				{
					return 6f;
				}
				return 20f;
			}
		}

		private Vector3 CurrentRealPosition => MyCamera.transform.position;

		public float AltitudePercent => Mathf.InverseLerp(125f, 1100f, altitude);

		public Vector3 CurrentlyLookingAtPointOnSphere => -(Quaternion.Inverse(sphereRotation) * Vector3.forward);

		private bool AnythingPreventsCameraMotion
		{
			get
			{
				if (!Find.WindowStack.WindowsPreventCameraMotion)
				{
					return !WorldRendererUtility.WorldRenderedNow;
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
			mouseCoveredByUI = false;
			if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
			{
				mouseCoveredByUI = true;
			}
			if (AnythingPreventsCameraMotion)
			{
				return;
			}
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
			{
				Vector2 currentEventDelta = UnityGUIBugsFixer.CurrentEventDelta;
				Event.current.Use();
				if (currentEventDelta != Vector2.zero)
				{
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorldCameraMovement, KnowledgeAmount.FrameInteraction);
					currentEventDelta.x *= -1f;
					desiredRotationRaw += currentEventDelta / GenWorldUI.CurUITileSize() * 0.273f * Prefs.MapDragSensitivity;
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
			desiredAltitude -= num * config.zoomSpeed * altitude / 12f;
			desiredAltitude = Mathf.Clamp(desiredAltitude, 125f, 1100f);
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
			Vector2 lhs = CalculateCurInputDollyVect();
			if (lhs != Vector2.zero)
			{
				float d = (altitude - 125f) / 975f * 0.85f + 0.15f;
				rotationVelocity = new Vector2(lhs.x, lhs.y) * d;
			}
			if (!Input.GetMouseButton(2) && dragTimeStamps.Any())
			{
				rotationVelocity += CameraDriver.GetExtraVelocityFromReleasingDragButton(dragTimeStamps, 5f);
				dragTimeStamps.Clear();
			}
			if (!AnythingPreventsCameraMotion)
			{
				float num = Time.deltaTime * CameraDriver.HitchReduceFactor;
				sphereRotation *= Quaternion.AngleAxis(rotationVelocity.x * num * config.rotationSpeedScale, MyCamera.transform.up);
				sphereRotation *= Quaternion.AngleAxis((0f - rotationVelocity.y) * num * config.rotationSpeedScale, MyCamera.transform.right);
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
			int num2 = Gen.FixedTimeStepUpdate(ref fixedTimeStepBuffer, 60f);
			for (int i = 0; i < num2; i++)
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
					float num3 = Mathf.Lerp(altitude, desiredAltitude, 0.05f);
					desiredAltitude += (num3 - altitude) * config.zoomPreserveFactor;
					altitude = num3;
				}
				else
				{
					float num4 = (desiredAltitude - altitude) * 0.4f;
					desiredAltitude += config.zoomPreserveFactor * num4;
					altitude += num4;
				}
			}
			rotationAnimation_lerpFactor += Time.deltaTime * 8f;
			if (Find.PlaySettings.lockNorthUp)
			{
				RotateSoNorthIsUp(interpolate: false);
				ClampXRotation(ref sphereRotation);
			}
			for (int j = 0; j < num2; j++)
			{
				config.ConfigFixedUpdate_60(ref rotationVelocity);
			}
			ApplyPositionToGameObject();
		}

		private void ApplyPositionToGameObject()
		{
			Quaternion invRot = (!(rotationAnimation_lerpFactor < 1f)) ? sphereRotation : Quaternion.Lerp(rotationAnimation_prevSphereRotation, sphereRotation, rotationAnimation_lerpFactor);
			if (Find.PlaySettings.lockNorthUp)
			{
				ClampXRotation(ref invRot);
			}
			MyCamera.transform.rotation = Quaternion.Inverse(invRot);
			Vector3 a = MyCamera.transform.rotation * Vector3.forward;
			MyCamera.transform.position = -a * altitude;
		}

		private Vector2 CalculateCurInputDollyVect()
		{
			Vector2 result = desiredRotation;
			bool flag = false;
			if ((UnityData.isEditor || Screen.fullScreen) && Prefs.EdgeScreenScroll && !mouseCoveredByUI)
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
			sphereRotation = Quaternion.Inverse(Quaternion.LookRotation(-newLookAt.normalized));
		}

		public void JumpTo(int tile)
		{
			JumpTo(Find.WorldGrid.GetTileCenter(tile));
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
}
