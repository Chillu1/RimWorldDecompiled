using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public class CameraDriver : MonoBehaviour
{
	public struct DragTimeStamp
	{
		public Vector2 posDelta;

		public float time;
	}

	public CameraShaker shaker = new CameraShaker();

	private CameraPanner panner;

	private Camera cachedCamera;

	private GameObject reverbDummy;

	public CameraMapConfig config = new CameraMapConfig_Normal();

	private Vector3 velocity;

	private Vector3 rootPos;

	private float desiredSize;

	private Vector2 desiredDolly = Vector2.zero;

	private Vector2 desiredDollyRaw = Vector2.zero;

	private List<DragTimeStamp> dragTimeStamps = new List<DragTimeStamp>();

	private bool releasedLeftWhileHoldingMiddle;

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

	private const float ZoomTightness = 0.4f;

	private const float ZoomScaleFromAltDenominator = 35f;

	private const float PageKeyZoomRate = 4f;

	public const float MinAltitude = 15f;

	private const float NearClipPlane = 0.5f;

	private const float MaxAltitude = 65f;

	private const float ReverbDummyAltitude = 65f;

	private float rootSize;

	public const float FullDurationPanDistance = 70f;

	private static float ScrollWheelZoomRate
	{
		get
		{
			if (!SteamDeck.IsSteamDeck)
			{
				return 0.35f;
			}
			return 0.55f;
		}
	}

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
			if (Screen.fullScreen || ResolutionUtility.BorderlessFullscreen)
			{
				return 6f;
			}
			return 20f;
		}
	}

	public float RootSize
	{
		get
		{
			return rootSize;
		}
		private set
		{
			if (rootSize != value)
			{
				if (Current.ProgramState != ProgramState.Playing || LongEventHandler.ShouldWaitForEvent || Find.Camera == null || !Prefs.ZoomToMouse)
				{
					rootSize = value;
					return;
				}
				ApplyPositionToGameObject();
				Vector3 vector = UI.MouseMapPosition();
				rootSize = value;
				ApplyPositionToGameObject();
				rootPos += vector - UI.MouseMapPosition();
			}
		}
	}

	public CameraZoomRange CurrentZoom
	{
		get
		{
			if (RootSize < config.sizeRange.min + 1f)
			{
				return CameraZoomRange.Closest;
			}
			if (RootSize < config.sizeRange.max * 0.23f)
			{
				return CameraZoomRange.Close;
			}
			if (RootSize < config.sizeRange.max * 0.7f)
			{
				return CameraZoomRange.Middle;
			}
			if (RootSize < config.sizeRange.max * 0.95f)
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
			if (!Find.WindowStack.WindowsPreventCameraMotion && !WorldRendererUtility.WorldSelected)
			{
				return !Current.Game.PlayerHasControl;
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

	public Vector2 ViewSpacePosition
	{
		get
		{
			Vector3 currentRealPosition = CurrentRealPosition;
			float x = Mathf.InverseLerp(2f, (float)Find.CurrentMap.Size.x + -2f, currentRealPosition.x);
			float y = Mathf.InverseLerp(2f, (float)Find.CurrentMap.Size.z + -2f, currentRealPosition.z);
			return new Vector2(x, y);
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
				lastViewRect.minX = Mathf.FloorToInt(currentRealPosition.x - RootSize * num - 1f);
				lastViewRect.maxX = Mathf.CeilToInt(currentRealPosition.x + RootSize * num);
				lastViewRect.minZ = Mathf.FloorToInt(currentRealPosition.z - RootSize - 1f);
				lastViewRect.maxZ = Mathf.CeilToInt(currentRealPosition.z + RootSize);
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

	public float CellSizePixels => (float)UI.screenHeight / (RootSize * 2f);

	public float ZoomRootSize => RootSize;

	public Vector3 InverseFovScale
	{
		get
		{
			float num = 1f - (float)UI.screenHeight / (4f * RootSize * RootSize);
			return new Vector3(num, num, num);
		}
	}

	public bool InViewOf(Thing thing)
	{
		CellRect cellRect = CurrentViewRect.ExpandedBy(1);
		cellRect.ClipInsideMap(thing.MapHeld);
		return cellRect.Overlaps(thing.OccupiedDrawRect());
	}

	public void Awake()
	{
		ResetSize();
		reverbDummy = GameObject.Find("ReverbZoneDummy");
		ApplyPositionToGameObject();
		MyCamera.nearClipPlane = 0.5f;
		MyCamera.farClipPlane = 65.5f;
	}

	public void OnPreCull()
	{
		if (!LongEventHandler.ShouldWaitForEvent && Find.CurrentMap != null && WorldRendererUtility.DrawingMap)
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
		if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(2))
		{
			releasedLeftWhileHoldingMiddle = true;
		}
		else if (Event.current.rawType == EventType.MouseDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
		{
			releasedLeftWhileHoldingMiddle = false;
		}
		mouseCoveredByUI = Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null;
		if (AnythingPreventsCameraMotion)
		{
			return;
		}
		if (UnityGUIBugsFixer.MouseDrag(2) && (!SteamDeck.IsSteamDeck || !Find.Selector.AnyPawnSelected))
		{
			Vector2 currentEventDelta = UnityGUIBugsFixer.CurrentEventDelta;
			if (Event.current.type == EventType.MouseDrag)
			{
				Event.current.Use();
			}
			if (currentEventDelta != Vector2.zero)
			{
				currentEventDelta.x *= -1f;
				desiredDollyRaw += currentEventDelta / UI.CurUICellSize() * Prefs.MapDragSensitivity;
				panner.JumpOnNextUpdate();
			}
		}
		float num = 0f;
		if (Event.current.type == EventType.ScrollWheel)
		{
			num -= Event.current.delta.y * ScrollWheelZoomRate;
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
		desiredSize -= num * config.zoomSpeed * RootSize / 35f;
		desiredSize = Mathf.Clamp(desiredSize, config.sizeRange.min, config.sizeRange.max);
		desiredDolly = Vector2.zero;
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
		if (desiredDolly != Vector2.zero)
		{
			panner.JumpOnNextUpdate();
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
			Vector2 vector = CalculateCurInputDollyVect();
			if (vector != Vector2.zero)
			{
				float num = (RootSize - config.sizeRange.min) / (config.sizeRange.max - config.sizeRange.min) * 0.7f + 0.3f;
				velocity = new Vector3(vector.x, 0f, vector.y) * num;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraDolly, KnowledgeAmount.FrameInteraction);
			}
			if ((Input.GetMouseButtonUp(2) || (SteamDeck.IsSteamDeck && releasedLeftWhileHoldingMiddle)) && dragTimeStamps.Any())
			{
				Vector2 extraVelocityFromReleasingDragButton = GetExtraVelocityFromReleasingDragButton(dragTimeStamps, 0.75f);
				velocity += new Vector3(extraVelocityFromReleasingDragButton.x, 0f, extraVelocityFromReleasingDragButton.y);
				dragTimeStamps.Clear();
			}
			if (!AnythingPreventsCameraMotion)
			{
				float num2 = Time.deltaTime * HitchReduceFactor;
				rootPos += velocity * (num2 * config.moveSpeedScale);
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
			int num3 = Gen.FixedTimeStepUpdate(ref fixedTimeStepBuffer, 60f);
			for (int i = 0; i < num3; i++)
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
					float num4 = Mathf.Lerp(RootSize, desiredSize, 0.05f);
					desiredSize += (num4 - RootSize) * config.zoomPreserveFactor;
					RootSize = num4;
				}
				else
				{
					float num5 = (desiredSize - RootSize) * 0.4f;
					desiredSize += config.zoomPreserveFactor * num5;
					RootSize += num5;
				}
				config.ConfigFixedUpdate_60(ref rootPos, ref velocity);
			}
			CameraPanner.Interpolant? interpolant = panner.Update();
			if (interpolant.HasValue)
			{
				CameraPanner.Interpolant valueOrDefault = interpolant.GetValueOrDefault();
				rootPos = valueOrDefault.Position;
				RootSize = valueOrDefault.Size;
				desiredSize = RootSize;
			}
			shaker.Update();
			ApplyPositionToGameObject();
			Current.SubcameraDriver.UpdatePositions(MyCamera);
			if (Find.CurrentMap != null)
			{
				RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
				rememberedCameraPos.rootPos = rootPos;
				rememberedCameraPos.rootSize = RootSize;
			}
		}
	}

	private void ApplyPositionToGameObject()
	{
		if (!(base.gameObject == null))
		{
			rootPos.y = 15f + (RootSize - config.sizeRange.min) / (config.sizeRange.max - config.sizeRange.min) * 50f;
			MyCamera.orthographicSize = RootSize;
			MyCamera.transform.position = rootPos + shaker.ShakeOffset;
			if (reverbDummy != null)
			{
				Vector3 position = base.transform.position;
				position.y = 65f;
				reverbDummy.transform.position = position;
			}
		}
	}

	private Vector2 CalculateCurInputDollyVect()
	{
		Vector2 result = desiredDolly;
		bool flag = false;
		if ((UnityData.isEditor || Screen.fullScreen || ResolutionUtility.BorderlessFullscreen) && Prefs.EdgeScreenScroll && !mouseCoveredByUI)
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
				RootSize = desiredSize;
				shaker.Expose();
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
		RootSize = desiredSize;
	}

	public void JumpToCurrentMapLoc(IntVec3 cell)
	{
		JumpToCurrentMapLoc(cell.ToVector3Shifted());
	}

	public void JumpToCurrentMapLoc(Vector3 loc)
	{
		rootPos = new Vector3(loc.x, rootPos.y, loc.z);
	}

	public void PanToMapLoc(IntVec3 cell)
	{
		Vector3 a = cell.ToVector3Shifted();
		float x = Vector3.Distance(a, CurrentRealPosition);
		float duration = GenMath.LerpDoubleClamped(0f, 70f, 0f, 0.25f, x);
		panner.PanTo(new CameraPanner.Interpolant(rootPos, RootSize), new CameraPanner.Interpolant(new Vector3(a.x, rootPos.y, a.z), RootSize), duration);
	}

	public void PanToMapLocAndSize(Vector3 loc, float size, float duration = 0.25f, PanCompletionCallback onComplete = null)
	{
		float x = Vector3.Distance(loc, CurrentRealPosition);
		float duration2 = GenMath.LerpDoubleClamped(0f, 70f, 0f, duration, x);
		panner.PanTo(new CameraPanner.Interpolant(rootPos, RootSize), new CameraPanner.Interpolant(new Vector3(loc.x, rootPos.y, loc.z), size), duration2, onComplete);
	}

	public void SetRootPosAndSize(Vector3 rootPos, float rootSize)
	{
		this.rootPos = rootPos;
		this.rootSize = rootSize;
		desiredDolly = Vector2.zero;
		desiredDollyRaw = Vector2.zero;
		desiredSize = RootSize;
		dragTimeStamps.Clear();
		LongEventHandler.ExecuteWhenFinished(ApplyPositionToGameObject);
	}

	public void SetRootSize(float size)
	{
		rootSize = size;
		desiredSize = RootSize;
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
