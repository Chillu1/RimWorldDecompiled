using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;

namespace Verse;

[StaticConstructorOnStartup]
public class WorldComponent_GravshipController : WorldComponent
{
	private Gravship gravship;

	private Map map;

	private PlanetTile takeoffTile = PlanetTile.Invalid;

	private PlanetTile landingTile = PlanetTile.Invalid;

	private static bool cutsceneInProgress = false;

	private float timeLeft = -1000f;

	private bool isTakeoff;

	public Map landingMap;

	private GravshipLandingMarker landingMarker;

	private FloatRange zoomRange;

	private GravshipCapturer gravshipCapturer;

	private GravshipRenderer gravshipRenderer;

	private Capture terrainCapture;

	private CellRect terrainCurtainBounds;

	private bool forceDrawTerrainCurtain;

	private LayerSubMesh terrainCurtainIndoorMask;

	private LayerSubMesh terrainCurtainShadowMask;

	private GravshipAudio gravshipAudio = new GravshipAudio();

	private bool mapHasGravAnchor;

	private const float InitialTime = 10f;

	private const float CameraSizeLerpSpeed = 5f;

	private const float CameraInitialPanDuration = 1f;

	public const int PlantDestroyRadius = 2;

	private static readonly int LayerGravshipExclude = LayerMask.NameToLayer("GravshipExclude");

	private static readonly int ShaderPropertyBackgroundTex = Shader.PropertyToID("_BackgroundTex");

	private static readonly int ShaderPropertyBackgroundColor = Shader.PropertyToID("_BackgroundColor");

	private static readonly Material IndoorMaskGravship = MatLoader.LoadMat("Misc/IndoorMaskGravship");

	private static readonly Material IndoorMaskTerrainCurtain = MatLoader.LoadMat("Misc/IndoorMaskTerrainCurtain");

	private static readonly Material GravshipShadowMaskTerrainCurtain = MatLoader.LoadMat("Misc/GravshipMaskTerrainCurtain");

	private static readonly Vector3[] CornerOffsetsPerRotation = new Vector3[4]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 0f, 0f)
	};

	private bool isPollutedLanding;

	private Designator_MoveGravship moveDesignator;

	private bool landingMarkerSpawned;

	public static bool CutsceneInProgress => cutsceneInProgress;

	public static bool GravshipRenderInProgess => GravshipCapturer.IsGravshipRenderInProgress;

	public static bool DisableDrawingPollution { get; private set; }

	public static CellRect GravshipRenderBounds => GravshipCapturer.GravshipCaptureBounds;

	public bool IsGravshipTravelling => gravship != null;

	public bool LandingAreaConfirmationInProgress => landingMarker != null;

	public WorldComponent_GravshipController(World world)
		: base(world)
	{
		if (ModsConfig.OdysseyActive)
		{
			gravshipCapturer = new GravshipCapturer();
			gravshipRenderer = new GravshipRenderer();
		}
	}

	public void InitiateTakeoff(Building_GravEngine engine, PlanetTile targetTile)
	{
		if (ModsConfig.OdysseyActive)
		{
			map = engine.Map;
			Current.Game.CurrentMap = map;
			takeoffTile = map.Tile;
			landingTile = targetTile;
			mapHasGravAnchor = map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor);
			SectionLayer_GravshipMask.Engine = engine;
			RegenerateGravshipMask();
			cutsceneInProgress = true;
			isTakeoff = true;
			gravshipRenderer.Init(map);
			gravshipCapturer.BeginGravshipRender(engine, OnGravshipCaptureComplete);
		}
	}

	public static void DestroyTreesAroundSubstructure(Map map, HashSet<IntVec3> substructure, int radius = 2, HashSet<IntVec3> borderCells = null)
	{
		if (!ModLister.CheckOdyssey("Gravship"))
		{
			return;
		}
		if (borderCells == null)
		{
			borderCells = new HashSet<IntVec3>(substructure);
		}
		HashSet<IntVec3> hashSet = ((radius > 1) ? new HashSet<IntVec3>() : null);
		foreach (IntVec3 borderCell in borderCells)
		{
			foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(borderCell))
			{
				if (substructure.Contains(item) || !item.InBounds(map))
				{
					continue;
				}
				hashSet?.Add(item);
				List<Thing> list = map.thingGrid.ThingsListAtFast(item);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					Thing thing = list[num];
					if (thing.def.plant != null && (thing.def.plant.IsTree || Mathf.Max(thing.DrawSize.x, thing.DrawSize.y) > 1.5f))
					{
						list[num].DeSpawn();
					}
				}
			}
		}
		if (radius > 0)
		{
			DestroyTreesAroundSubstructure(map, substructure, radius - 1, hashSet);
		}
	}

	private static HashSet<Section> SectionsWithPollution(Map map, IEnumerable<IntVec3> cells)
	{
		HashSet<Section> hashSet = new HashSet<Section>();
		if (!ModsConfig.BiotechActive || map.pollutionGrid.TotalPollution <= 0)
		{
			return hashSet;
		}
		foreach (IntVec3 cell in cells)
		{
			if (cell.IsPolluted(map))
			{
				hashSet.Add(map.mapDrawer.SectionAt(cell));
			}
		}
		return hashSet;
	}

	private static void RegenerateTerrainInSections(IEnumerable<Section> sections)
	{
		foreach (Section section in sections)
		{
			section.RegenerateSingleLayer(section.GetLayer(typeof(SectionLayer_Terrain)));
		}
	}

	private void RegenerateGravshipMask()
	{
		map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipMask));
		map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipHull));
		map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_SubstructureProps));
	}

	public void OnGravshipCaptureComplete(Capture capture)
	{
		zoomRange = GetCutsceneZoomRange(capture);
		PanIf(Find.CameraDriver.config.gravshipPanOnCutsceneStart, capture.captureCenter, zoomRange.min, 1f, delegate
		{
			Delay.AfterNSeconds(0.5f, delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					HashSet<IntVec3> validSubstructure = capture.engine.ValidSubstructure;
					LayerSubMesh item = SectionLayer_IndoorMask.BakeGravshipIndoorMesh(map, validSubstructure, validSubstructure.Count, IndoorMaskGravship, capture.captureCenter);
					List<LayerSubMesh> collection = SectionLayer_GravshipHull.BakeGravshipIndoorMesh(map, capture.captureBounds, capture.captureCenter);
					gravship = RemoveGravshipFromMap(capture.engine);
					gravship.capture = capture;
					gravship.bakedIndoorMasks.Clear();
					gravship.bakedIndoorMasks.Add(item);
					gravship.bakedIndoorMasks.AddRange(collection);
					if (mapHasGravAnchor)
					{
						map.GetComponent<VacuumComponent>().RebuildData();
					}
					RegenerateGravshipMask();
					BeginTakeoffCutscene();
				}, "GeneratingGravship", doAsynchronously: false, null);
			});
		});
	}

	private void BeginTakeoffCutscene()
	{
		timeLeft = 10.5f;
		Find.CameraDriver.shaker.DoShake(0.2f, 120);
		gravshipRenderer.BeginCutscene(gravship, gravship.capture.captureCenter, gravship.originalPosition.ToVector3(), Rot4.North);
		gravshipAudio.BeginTakeoff();
	}

	public void InitiateLanding(Gravship gravship, Map map, IntVec3 landingPos, Rot4 landingRot)
	{
		this.map = map;
		this.gravship = null;
		cutsceneInProgress = true;
		isTakeoff = false;
		Current.Game.CurrentMap = map;
		Find.ScreenshotModeHandler.Active = true;
		int dist = Mathf.CeilToInt(Mathf.Max(15f, 15f));
		terrainCurtainBounds = CellRect.FromCellList(landingMarker.GravshipCells).ExpandedBy(dist).MovedBy(landingPos.x, landingPos.z);
		foreach (Pawn item2 in map.mapPawns.AllPawnsSpawned)
		{
			if (terrainCurtainBounds.Contains(item2.Position))
			{
				item2.pather.ResetToCurrentPosition();
			}
		}
		HashSet<IntVec3> landingMarkerCells = new HashSet<IntVec3>(landingMarker.GravshipCells.Select((IntVec3 c) => c + landingPos));
		DestroyTreesAroundSubstructure(map, landingMarkerCells);
		if (BiomeHasTransparency(map.Biome))
		{
			GravshipCapturer.CaptureWorldSkybox(delegate(Texture2D skybox)
			{
				GravshipRenderer.MatTerrainCurtain.SetTexture(ShaderPropertyBackgroundTex, skybox);
				CaptureAndBeginCutscene();
			});
		}
		else
		{
			CaptureAndBeginCutscene();
		}
		void CaptureAndBeginCutscene()
		{
			PanIf(Find.CameraDriver.config.gravshipPanOnCutsceneStart, terrainCurtainBounds.CenterVector3, zoomRange.max, 1f, delegate
			{
				terrainCurtainIndoorMask = SectionLayer_IndoorMask.BakeGravshipIndoorMesh(map, terrainCurtainBounds, terrainCurtainBounds.Area, IndoorMaskTerrainCurtain, terrainCurtainBounds.CenterVector3);
				LayerSubMesh gravshipShadowMask = null;
				if (BiomeHasTransparency(map.Biome))
				{
					gravshipShadowMask = SectionLayer_GravshipMask.BakeGravshipShadowMask(map, GravshipShadowMaskTerrainCurtain, terrainCurtainBounds);
				}
				else
				{
					gravshipShadowMask = SectionLayer_GravshipMask.BakeDummyShadowMask(GravshipShadowMaskTerrainCurtain, terrainCurtainBounds.CenterVector3 + Vector3.up * AltitudeLayer.MetaOverlays.AltitudeFor(), terrainCurtainBounds.Width, terrainCurtainBounds.Height);
				}
				gravshipCapturer.BeginTerrainRender(terrainCurtainBounds, delegate(Capture terrainCapture)
				{
					this.terrainCapture = terrainCapture;
					forceDrawTerrainCurtain = true;
					Building_GravEngine engine = null;
					HashSet<Section> pollutedSections = null;
					LongEventHandler.QueueLongEvent(delegate
					{
						SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.Gravship;
						pollutedSections = SectionsWithPollution(map, landingMarkerCells);
						isPollutedLanding = pollutedSections.Count > 0;
						DisableDrawingPollution = isPollutedLanding;
						engine = PlaceGravship(gravship, landingPos, map);
						SectionLayer_GravshipMask.Engine = engine;
						RegenerateGravshipMask();
					}, "PreparingForLanding", doAsynchronously: false, null, showExtraUIInfo: true, forceHideUI: false, delegate
					{
						Find.ScreenshotModeHandler.Active = true;
						this.gravship = gravship;
						gravshipRenderer.Init(map);
						gravshipCapturer.BeginGravshipRender(engine, delegate(Capture gravshipCapture)
						{
							HashSet<IntVec3> validSubstructure = gravshipCapture.engine.ValidSubstructure;
							LayerSubMesh item = SectionLayer_IndoorMask.BakeGravshipIndoorMesh(map, validSubstructure, validSubstructure.Count, IndoorMaskGravship, gravshipCapture.captureCenter);
							List<LayerSubMesh> collection = SectionLayer_GravshipHull.BakeGravshipIndoorMesh(map, gravshipCapture.captureBounds, gravshipCapture.captureCenter);
							gravship.capture = gravshipCapture;
							gravship.bakedIndoorMasks.Clear();
							gravship.bakedIndoorMasks.Add(item);
							gravship.bakedIndoorMasks.AddRange(collection);
							terrainCurtainShadowMask = gravshipShadowMask;
							SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.None;
							RegenerateGravshipMask();
							if (DisableDrawingPollution)
							{
								DisableDrawingPollution = false;
								RegenerateTerrainInSections(pollutedSections);
							}
							forceDrawTerrainCurtain = false;
							BeginLandingCutscene(landingPos, landingRot);
						});
					});
				});
			});
		}
	}

	private static bool BiomeHasTransparency(BiomeDef biome)
	{
		if (biome != BiomeDefOf.Space)
		{
			return biome == BiomeDefOf.Orbit;
		}
		return true;
	}

	private void BeginLandingCutscene(IntVec3 landingPos, Rot4 landingRot)
	{
		timeLeft = 10f;
		Find.CameraDriver.shaker.DoShake(0.2f, 120);
		Vector3 vector = CornerOffsetsPerRotation[landingRot.AsInt];
		Vector3 takeoffOrLandingCenter = landingPos.ToVector3() + landingRot.AsQuat * gravship.engineToCenter + vector;
		gravshipRenderer.BeginCutscene(gravship, takeoffOrLandingCenter, landingPos.ToVector3(), landingRot);
		gravshipAudio.BeginLanding();
	}

	private Gravship RemoveGravshipFromMap(Building_GravEngine engine)
	{
		if (!engine.Spawned)
		{
			Log.Error("Tried to make a gravship out of an unspawned engine");
			return null;
		}
		return GravshipUtility.GenerateGravship(engine);
	}

	private void TakeoffEnded()
	{
		if (ModsConfig.OdysseyActive)
		{
			cutsceneInProgress = false;
			Find.CameraDriver.shaker.StopAllShaking();
			if (!mapHasGravAnchor)
			{
				GravshipUtility.AbandonMap(map);
			}
			else
			{
				GravshipUtility.UpdateBillDestinations(map);
			}
			GravshipUtility.TravelTo(gravship, takeoffTile, landingTile);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
			gravshipAudio.EndTakeoff();
			map = null;
			ResetCutscene();
		}
	}

	public override void WorldComponentUpdate()
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		gravshipCapturer.Update();
		float progress = Mathf.Clamp01(1f - timeLeft / 10f);
		gravshipAudio.Update(10f - timeLeft, progress, isTakeoff);
		if (forceDrawTerrainCurtain)
		{
			DrawTerrainCurtain(0f);
		}
		if (!gravshipCapturer.IsCaptureComplete || !cutsceneInProgress || gravship == null)
		{
			return;
		}
		if (timeLeft > 0f && Prefs.GravshipCutscenes)
		{
			timeLeft -= Time.deltaTime;
			if (!isTakeoff && !forceDrawTerrainCurtain)
			{
				DrawTerrainCurtain(progress);
			}
			DrawGravship(progress);
		}
		else if (DebugSettings.loopGravshipCutscene)
		{
			BeginTakeoffCutscene();
		}
		else if (isTakeoff)
		{
			TakeoffEnded();
		}
		else
		{
			LandingEnded();
		}
	}

	private float EaseInOut(float x)
	{
		if (!(x < 0.5f))
		{
			return 1f - Mathf.Pow(-2f * x + 2f, 3f) * 0.5f;
		}
		return 4f * x * x * x;
	}

	private void PanIf(bool panCondition, Vector3 loc, float size, float duration = 0.25f, PanCompletionCallback onComplete = null)
	{
		if (panCondition)
		{
			Find.CameraDriver.PanToMapLocAndSize(loc, size, duration, onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	private FloatRange GetCutsceneZoomRange(Capture capture)
	{
		CameraMapConfig config = Find.CameraDriver.config;
		if (config.gravshipEnableOverrideSizeRange)
		{
			return config.gravshipOverrideSizeRange;
		}
		return new FloatRange(capture.minCameraSize, capture.maxCameraSize);
	}

	private void DrawTerrainCurtain(float progress)
	{
		GravshipRenderer.MatTerrainCurtain.mainTexture = (Texture2D)terrainCapture.capture;
		float a = (isPollutedLanding ? progress.RemapClamped(0.8f, 0.9f, 1f, 0f) : progress.RemapClamped(0.9f, 1f, 1f, 0f));
		Color color = new Color(1f, 1f, 1f, a);
		float num = (BiomeHasTransparency(map.Biome) ? 1f : 0f);
		Color value = ((map.Biome != null && map.Biome.disableSkyLighting) ? new Color(1f, 1f, 1f, num) : map.skyManager.CurSky.colors.sky.WithAlpha(num));
		GravshipRenderer.MatTerrainCurtain.color = color;
		GravshipRenderer.MatTerrainCurtain.SetColor(ShaderPropertyBackgroundColor, value);
		GenDraw.DrawCellRect(terrainCurtainBounds, Vector3.up * (AltitudeLayer.Skyfaller.AltitudeFor() - 0.03658537f), GravshipRenderer.MatTerrainCurtain, null, LayerGravshipExclude);
		if (terrainCurtainIndoorMask != null)
		{
			Graphics.DrawMesh(terrainCurtainIndoorMask.mesh, Matrix4x4.TRS(terrainCurtainBounds.CenterVector3, Quaternion.identity, Vector3.one), terrainCurtainIndoorMask.material, 0);
		}
		if (terrainCurtainShadowMask != null)
		{
			Graphics.DrawMesh(terrainCurtainShadowMask.mesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), terrainCurtainShadowMask.material, 0);
		}
	}

	private void DrawGravship(float progress)
	{
		gravshipRenderer.BeginUpdate();
		FloatRange cutsceneZoomRange = GetCutsceneZoomRange(gravship.capture);
		float b;
		if (isTakeoff)
		{
			gravshipRenderer.UpdateTakeoff(progress);
			b = Mathf.Lerp(cutsceneZoomRange.min, cutsceneZoomRange.max, EaseInOut(progress.RemapClamped(0f, 0.7f, 0f, 1f)));
		}
		else
		{
			gravshipRenderer.UpdateLanding(progress, isPollutedLanding);
			b = Mathf.Lerp(cutsceneZoomRange.max, cutsceneZoomRange.min, EaseInOut(progress.RemapClamped(0f, 0.8f, 0f, 1f)));
		}
		CameraDriver cameraDriver = Find.CameraDriver;
		if (!cameraDriver.config.gravshipFreeCam)
		{
			cameraDriver.SetRootSize(Mathf.Lerp(cameraDriver.RootSize, b, 5f * Time.deltaTime));
		}
		gravshipRenderer.EndUpdate();
	}

	public override void FinalizeInit(bool fromLoad)
	{
		if (!fromLoad)
		{
			ResetCutscene();
		}
	}

	private void ResetCutscene()
	{
		Find.ScreenshotModeHandler.Active = false;
		cutsceneInProgress = false;
		landingMap = null;
	}

	public void Notify_LandingAreaConfirmationStarted(GravshipLandingMarker marker)
	{
		landingMarker = marker;
	}

	private void LandingEnded()
	{
		Map map = this.map;
		SectionLayer_GravshipMask.Engine = gravship.Engine;
		SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.Gravship;
		RegenerateGravshipMask();
		SectionLayer_GravshipMask.OverrideMode = SectionLayer_GravshipMask.MaskOverrideMode.None;
		if (gravship.Engine.launchInfo.doNegativeOutcome)
		{
			DefDatabase<LandingOutcomeDef>.AllDefsListForReading.RandomElementByWeight((LandingOutcomeDef d) => d.weight).Worker.ApplyOutcome(gravship);
		}
		terrainCapture = null;
		gravship = null;
		Current.Game.Gravship = null;
		Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
		terrainCurtainIndoorMask = null;
		terrainCurtainShadowMask = null;
		this.map = null;
		moveDesignator = null;
		ResetCutscene();
		Find.Scenario.PostGravshipLanded(map);
	}

	private Building_GravEngine PlaceGravship(Gravship gravship, IntVec3 root, Map map)
	{
		GravshipPlacementUtility.PlaceGravshipInMap(gravship, root, map, out var spawned);
		GravshipPlacementUtility.ApplyTemperatureVacuumFromBase(gravship, root, map);
		map.listerFilthInHomeArea.RebuildAll();
		map.resourceCounter.UpdateResourceCounts();
		map.wealthWatcher.ForceRecount(allowDuringInit: true);
		map.powerNetManager.UpdatePowerNetsAndConnections_First();
		GravshipPlacementUtility.PostSwapMap(gravship, spawned);
		return gravship.Engine;
	}

	public override void WorldComponentOnGUI()
	{
		if (!LandingAreaConfirmationInProgress || Find.ScreenshotModeHandler.Active || !WorldRendererUtility.DrawingMap)
		{
			return;
		}
		float num = 430f;
		if (landingMap != null)
		{
			num = 640f;
		}
		Rect rect = new Rect((float)UI.screenWidth / 2f - num / 2f, (float)UI.screenHeight - 150f - 70f, num, 70f);
		Widgets.DrawWindowBackground(rect);
		Rect rect2 = new Rect(rect.xMin + 10f, rect.yMin + 10f, 200f, 50f);
		if (landingMap != null)
		{
			if (Widgets.ButtonText(rect2, "AbortLandGravship".Translate()))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("AbortLandConfirmation".Translate(), AbortLanding));
			}
			TooltipHandler.TipRegion(rect2, "AbortLandGravshipDesc".Translate());
			rect2.x += 210f;
		}
		Designator_MoveGravship designator_MoveGravship = MoveDesignator();
		if (Widgets.ButtonText(rect2, "DesignatorMoveGravship".Translate()))
		{
			Find.DesignatorManager.Select(designator_MoveGravship);
		}
		TooltipHandler.TipRegion(rect2, "DesignatorMoveGravshipDesc".Translate());
		rect2.x += 210f;
		if (Widgets.ButtonText(rect2, "ConfirmLandGravship".Translate()))
		{
			if (landingMarker.Spawned && Find.DesignatorManager.SelectedDesignator != designator_MoveGravship)
			{
				landingMarker.BeginLanding(this);
				landingMarker = null;
				SoundDefOf.Gravship_Land.PlayOneShotOnCamera();
			}
			else
			{
				Messages.Message("GravshipLandingMarkerNotPlaced".Translate(), MessageTypeDefOf.RejectInput);
			}
		}
		TooltipHandler.TipRegion(rect2, "ConfirmLandGravshipDesc".Translate());
	}

	private void AbortLanding()
	{
		PlanetLayer layer = landingMap.Tile.Layer;
		int num = 1;
		PlanetTile destinationTile = PlanetTile.Invalid;
		while (!destinationTile.Valid && num < 50)
		{
			FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(landingMap.Tile, 1f, num, FastTileFinder.LandmarkMode.Any, reachable: false);
			destinationTile = layer.FastTileFinder.Query(query).RandomElementWithFallback(PlanetTile.Invalid);
			num++;
		}
		if (!destinationTile.Valid)
		{
			Messages.Message("FailedToFindCrashSite".Translate(), MessageTypeDefOf.RejectInput);
			return;
		}
		landingMap = null;
		landingMarker.Destroy();
		landingMarker = null;
		Find.CurrentGravship.destinationTile = destinationTile;
		float quality = Find.CurrentGravship.Engine.launchInfo.quality;
		Find.CurrentGravship.Engine.launchInfo.doNegativeOutcome = Rand.Chance(GravshipUtility.NegativeLandingOutcomeFromQuality(quality) * 2f);
		LongEventHandler.QueueLongEvent(delegate
		{
			GravshipUtility.ArriveNewMap(Find.CurrentGravship);
		}, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
	}

	public Designator_MoveGravship MoveDesignator()
	{
		if (moveDesignator == null)
		{
			moveDesignator = new Designator_MoveGravship(landingMap, landingMarker);
		}
		moveDesignator.marker = landingMarker;
		return moveDesignator;
	}

	public override void ExposeData()
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		Scribe_Values.Look(ref cutsceneInProgress, "cutsceneInProgress", defaultValue: false);
		Scribe_Values.Look(ref takeoffTile, "takeoffTile", PlanetTile.Invalid);
		Scribe_Values.Look(ref landingTile, "targetTile", PlanetTile.Invalid);
		Scribe_References.Look(ref map, "takeoffMap");
		Scribe_References.Look(ref gravship, "gravship");
		Scribe_References.Look(ref landingMap, "landingMap");
		Scribe_Values.Look(ref zoomRange, "zoomRange");
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			landingMarkerSpawned = landingMarker?.Spawned ?? false;
		}
		Scribe_Values.Look(ref landingMarkerSpawned, "landingMarkerSpawned", defaultValue: false);
		if (landingMarkerSpawned)
		{
			Scribe_References.Look(ref landingMarker, "landingMarker");
		}
		else
		{
			Scribe_Deep.Look(ref landingMarker, "landingMarker");
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && landingMarker != null && !landingMarkerSpawned)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				CameraJumper.TryJump(landingMap.Center, landingMap, CameraJumper.MovementMode.Cut);
			});
		}
	}
}
