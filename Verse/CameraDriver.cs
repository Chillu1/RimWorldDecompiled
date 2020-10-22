using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class CameraDriver : MonoBehaviour
	{
		public struct DragTimeStamp
		{
			public Vector2 posDelta;

			public float time;
		}

		public CameraShaker shaker = new CameraShaker();

		private Camera cachedCamera;

		private GameObject reverbDummy;

		public CameraMapConfig config = new CameraMapConfig_Normal();

		private Vector3 velocity;

		private Vector3 rootPos;

		private float rootSize;

		private float desiredSize;

		private Vector2 desiredDolly = Vector2.zero;

		private Vector2 desiredDollyRaw = Vector2.zero;

		private List<DragTimeStamp> dragTimeStamps = new List<DragTimeStamp>();

		private bool mouseCoveredByUI;

		private float mouseTouchingScreenBottomEdgeStartTime = -1f;

		private float fixedTimeStepBuffer;

		private static int lastViewRectGetFrame = -1;

		private static CellRect lastViewRect;

		public const float MaxDeltaTime = 0.1f;

		private const float ScreenDollyEdgeWidth = 20f;

		private const float ScreenDollyEdgeWidth_BottomFullscreen = 6f;

		private const float MinDurationForMouseToTouchScreenBottomEdgeToDolly = 0.28f;

		private const float DragTimeStampExpireSeconds = 0.05f;

		private const float VelocityFromMouseDragInitialFactor = 0.75f;

		private const float MapEdgeClampMarginCells = -2f;

		public const float StartingSize = 24f;

		private const float MaxSize = 60f;

		private const float ZoomTightness = 0.4f;

		private const float ZoomScaleFromAltDenominator = 35f;

		private const float PageKeyZoomRate = 4f;

		private const float ScrollWheelZoomRate = 0.35f;

		public const float MinAltitude = 15f;

		private const float MaxAltitude = 65f;

		private const float ReverbDummyAltitude = 65f;

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

		public CameraZoomRange CurrentZoom
		{
			get
			{
				if (rootSize < config.minSize + 1f)
				{
					return CameraZoomRange.Closest;
				}
				if (rootSize < 13.8f)
				{
					return CameraZoomRange.Close;
				}
				if (rootSize < 42f)
				{
					return CameraZoomRange.Middle;
				}
				if (rootSize < 57f)
				{
					return CameraZoomRange.Far;
				}
				return CameraZoomRange.Furthest;
			}
		}

		private Vector3 CurrentRealPosition => MyCamera.transform.position;

		private bool AnythingPreventsCameraMotion
		{
			get
			{
				if (!Find.WindowStack.WindowsPreventCameraMotion)
				{
					return WorldRendererUtility.WorldRenderedNow;
				}
				return true;
			}
		}

		public IntVec3 MapPosition
		{
			get
			{
				IntVec3 result = CurrentRealPosition.ToIntVec3();
				result.y = 0;
				return result;
			}
		}

		public CellRect CurrentViewRect
		{
			get
			{
				if (Time.frameCount != lastViewRectGetFrame)
				{
					lastViewRect = default(CellRect);
					float num = (float)UI.screenWidth / (float)UI.screenHeight;
					Vector3 currentRealPosition = CurrentRealPosition;
					lastViewRect.minX = Mathf.FloorToInt(currentRealPosition.x - rootSize * num - 1f);
					lastViewRect.maxX = Mathf.CeilToInt(currentRealPosition.x + rootSize * num);
					lastViewRect.minZ = Mathf.FloorToInt(currentRealPosition.z - rootSize - 1f);
					lastViewRect.maxZ = Mathf.CeilToInt(currentRealPosition.z + rootSize);
					lastViewRectGetFrame = Time.frameCount;
				}
				return lastViewRect;
			}
		}

		public static float HitchReduceFactor
		{
			get
			{
				float result = 1f;
				if (Time.deltaTime > 0.1f)
				{
					result = 0.1f / Time.deltaTime;
				}
				return result;
			}
		}

		public float CellSizePixels => (float)UI.screenHeight / (rootSize * 2f);

		public void Awake()
		{
			ResetSize();
			reverbDummy = GameObject.Find("ReverbZoneDummy");
			ApplyPositionToGameObject();
			MyCamera.farClipPlane = 71.5f;
		}

		public void OnPreRender()
		{
			if (!LongEventHandler.ShouldWaitForEvent)
			{
				_ = Find.CurrentMap;
			}
		}

		public void OnPreCull()
		{
			if (!LongEventHandler.ShouldWaitForEvent && Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				Find.CurrentMap.weatherManager.DrawAllWeather();
			}
		}

		public void CameraDriverOnGUI()
		{
			if (Find.CurrentMap == null)
			{
				return;
			}
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
					currentEventDelta.x *= -1f;
					desiredDollyRaw += currentEventDelta / UI.CurUICellSize() * Prefs.MapDragSensitivity;
				}
			}
			float num = 0f;
			if (Event.current.type == EventType.ScrollWheel)
			{
				num -= Event.current.delta.y * 0.35f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.TinyInteraction);
			}
			if (KeyBindingDefOf.MapZoom_In.KeyDownEvent)
			{
				num += 4f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
			}
			if (KeyBindingDefOf.MapZoom_Out.KeyDownEvent)
			{
				num -= 4f;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
			}
			desiredSize -= num * config.zoomSpeed * rootSize / 35f;
			desiredSize = Mathf.Clamp(desiredSize, config.minSize, 60f);
			desiredDolly = Vector3.zero;
			if (KeyBindingDefOf.MapDolly_Left.IsDown)
			{
				desiredDolly.x = 0f - config.dollyRateKeys;
			}
			if (KeyBindingDefOf.MapDolly_Right.IsDown)
			{
				desiredDolly.x = config.dollyRateKeys;
			}
			if (KeyBindingDefOf.MapDolly_Up.IsDown)
			{
				desiredDolly.y = config.dollyRateKeys;
			}
			if (KeyBindingDefOf.MapDolly_Down.IsDown)
			{
				desiredDolly.y = 0f - config.dollyRateKeys;
			}
			config.ConfigOnGUI();
		}

		public void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				if (Current.SubcameraDriver != null)
				{
					Current.SubcameraDriver.UpdatePositions(MyCamera);
				}
			}
			else
			{
				if (Find.CurrentMap == null)
				{
					return;
				}
				Vector2 lhs = CalculateCurInputDollyVect();
				if (lhs != Vector2.zero)
				{
					float d = (rootSize - config.minSize) / (60f - config.minSize) * 0.7f + 0.3f;
					velocity = new Vector3(lhs.x, 0f, lhs.y) * d;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraDolly, KnowledgeAmount.FrameInteraction);
				}
				if (!Input.GetMouseButton(2) && dragTimeStamps.Any())
				{
					Vector2 extraVelocityFromReleasingDragButton = GetExtraVelocityFromReleasingDragButton(dragTimeStamps, 0.75f);
					velocity += new Vector3(extraVelocityFromReleasingDragButton.x, 0f, extraVelocityFromReleasingDragButton.y);
					dragTimeStamps.Clear();
				}
				if (!AnythingPreventsCameraMotion)
				{
					float d2 = Time.deltaTime * HitchReduceFactor;
					rootPos += velocity * d2 * config.moveSpeedScale;
					rootPos += new Vector3(desiredDollyRaw.x, 0f, desiredDollyRaw.y);
					dragTimeStamps.Add(new DragTimeStamp
					{
						posDelta = desiredDollyRaw,
						time = Time.time
					});
					rootPos.x = Mathf.Clamp(rootPos.x, 2f, (float)Find.CurrentMap.Size.x + -2f);
					rootPos.z = Mathf.Clamp(rootPos.z, 2f, (float)Find.CurrentMap.Size.z + -2f);
				}
				desiredDollyRaw = Vector2.zero;
				int num = Gen.FixedTimeStepUpdate(ref fixedTimeStepBuffer, 60f);
				for (int i = 0; i < num; i++)
				{
					if (velocity != Vector3.zero)
					{
						velocity *= config.camSpeedDecayFactor;
						if (velocity.magnitude < 0.1f)
						{
							velocity = Vector3.zero;
						}
					}
					if (config.smoothZoom)
					{
						float num2 = Mathf.Lerp(rootSize, desiredSize, 0.05f);
						desiredSize += (num2 - rootSize) * config.zoomPreserveFactor;
						rootSize = num2;
					}
					else
					{
						float num3 = (desiredSize - rootSize) * 0.4f;
						desiredSize += config.zoomPreserveFactor * num3;
						rootSize += num3;
					}
					config.ConfigFixedUpdate_60(ref velocity);
				}
				shaker.Update();
				ApplyPositionToGameObject();
				Current.SubcameraDriver.UpdatePositions(MyCamera);
				if (Find.CurrentMap != null)
				{
					RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
					rememberedCameraPos.rootPos = rootPos;
					rememberedCameraPos.rootSize = rootSize;
				}
			}
		}

		private void ApplyPositionToGameObject()
		{
			rootPos.y = 15f + (rootSize - config.minSize) / (60f - config.minSize) * 50f;
			MyCamera.orthographicSize = rootSize;
			MyCamera.transform.position = rootPos + shaker.ShakeOffset;
			Vector3 position = base.transform.position;
			position.y = 65f;
			reverbDummy.transform.position = position;
		}

		private Vector2 CalculateCurInputDollyVect()
		{
			Vector2 result = desiredDolly;
			bool flag = false;
			if ((UnityData.isEditor || Screen.fullScreen) && Prefs.EdgeScreenScroll && !mouseCoveredByUI)
			{
				Vector2 mousePositionOnUI = UI.MousePositionOnUI;
				Vector2 point = mousePositionOnUI;
				point.y = (float)UI.screenHeight - point.y;
				Rect rect = new Rect(0f, 0f, 200f, 200f);
				Rect rect2 = new Rect(UI.screenWidth - 250, 0f, 255f, 255f);
				Rect rect3 = new Rect(0f, UI.screenHeight - 250, 225f, 255f);
				Rect rect4 = new Rect(UI.screenWidth - 250, UI.screenHeight - 250, 255f, 255f);
				MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
				if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect && mainTabWindow_Inspect.RecentHeight > rect3.height)
				{
					rect3.yMin = (float)UI.screenHeight - mainTabWindow_Inspect.RecentHeight;
				}
				if (!rect.Contains(point) && !rect3.Contains(point) && !rect2.Contains(point) && !rect4.Contains(point))
				{
					Vector2 vector = new Vector2(0f, 0f);
					if (mousePositionOnUI.x >= 0f && mousePositionOnUI.x < 20f)
					{
						vector.x -= config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.x <= (float)UI.screenWidth && mousePositionOnUI.x > (float)UI.screenWidth - 20f)
					{
						vector.x += config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y <= (float)UI.screenHeight && mousePositionOnUI.y > (float)UI.screenHeight - 20f)
					{
						vector.y += config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y >= 0f && mousePositionOnUI.y < ScreenDollyEdgeWidthBottom)
					{
						if (mouseTouchingScreenBottomEdgeStartTime < 0f)
						{
							mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
						}
						if (Time.realtimeSinceStartup - mouseTouchingScreenBottomEdgeStartTime >= 0.28f)
						{
							vector.y -= config.dollyRateScreenEdge;
						}
						flag = true;
					}
					result += vector;
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

		public void Expose()
		{
			if (Scribe.EnterNode("cameraMap"))
			{
				try
				{
					Scribe_Values.Look(ref rootPos, "camRootPos");
					Scribe_Values.Look(ref desiredSize, "desiredSize", 0f);
					rootSize = desiredSize;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public void ResetSize()
		{
			desiredSize = 24f;
			rootSize = desiredSize;
		}

		public void JumpToCurrentMapLoc(IntVec3 cell)
		{
			JumpToCurrentMapLoc(cell.ToVector3Shifted());
		}

		public void JumpToCurrentMapLoc(Vector3 loc)
		{
			rootPos = new Vector3(loc.x, rootPos.y, loc.z);
		}

		public void SetRootPosAndSize(Vector3 rootPos, float rootSize)
		{
			this.rootPos = rootPos;
			this.rootSize = rootSize;
			desiredDolly = Vector2.zero;
			desiredDollyRaw = Vector2.zero;
			desiredSize = rootSize;
			dragTimeStamps.Clear();
			LongEventHandler.ExecuteWhenFinished(ApplyPositionToGameObject);
		}

		public static Vector2 GetExtraVelocityFromReleasingDragButton(List<DragTimeStamp> dragTimeStamps, float velocityFromMouseDragInitialFactor)
		{
			float num = 0f;
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < dragTimeStamps.Count; i++)
			{
				if (dragTimeStamps[i].time < Time.time - 0.05f)
				{
					num = 0.05f;
					continue;
				}
				num = Mathf.Max(num, Time.time - dragTimeStamps[i].time);
				zero += dragTimeStamps[i].posDelta;
			}
			if (zero != Vector2.zero && num > 0f)
			{
				return zero / num * velocityFromMouseDragInitialFactor;
			}
			return Vector2.zero;
		}
	}
}
