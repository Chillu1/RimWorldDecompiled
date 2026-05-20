using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class WorldRoutePlanner
{
	private bool active;

	private PlanetLayer layer;

	private CaravanTicksPerMoveUtility.CaravanInfo? caravanInfoFromFormCaravanDialog;

	private Dialog_FormCaravan currentFormCaravanDialog;

	private bool cantRemoveFirstWaypoint;

	private readonly List<WorldPath> paths = new List<WorldPath>();

	private readonly List<int> cachedTicksToWaypoint = new List<int>();

	public readonly List<RoutePlannerWaypoint> waypoints = new List<RoutePlannerWaypoint>();

	private const int MaxCount = 25;

	private static readonly Texture2D ButtonTex = ContentFinder<Texture2D>.Get("UI/Misc/WorldRoutePlanner");

	private static readonly Texture2D MouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/WaypointMouseAttachment");

	private static readonly Vector2 BottomWindowSize = new Vector2(500f, 95f);

	private static readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

	private const float BottomWindowBotMargin = 45f;

	private const float BottomWindowEntryExtraBotMargin = 22f;

	public bool Active => active;

	public bool FormingCaravan
	{
		get
		{
			if (Active)
			{
				return currentFormCaravanDialog != null;
			}
			return false;
		}
	}

	private bool ShouldStop
	{
		get
		{
			if (!active)
			{
				return true;
			}
			if (!WorldRendererUtility.WorldSelected)
			{
				return true;
			}
			if (Current.ProgramState == ProgramState.Playing && Find.TickManager.CurTimeSpeed != TimeSpeed.Paused)
			{
				return true;
			}
			return false;
		}
	}

	private int CaravanTicksPerMove
	{
		get
		{
			CaravanTicksPerMoveUtility.CaravanInfo? caravanInfo = CaravanInfo;
			if (caravanInfo.HasValue && caravanInfo.Value.pawns.Any())
			{
				return CaravanTicksPerMoveUtility.GetTicksPerMove(caravanInfo.Value);
			}
			return 3464;
		}
	}

	private CaravanTicksPerMoveUtility.CaravanInfo? CaravanInfo
	{
		get
		{
			if (currentFormCaravanDialog != null)
			{
				return caravanInfoFromFormCaravanDialog;
			}
			Caravan caravanAtTheFirstWaypoint = CaravanAtTheFirstWaypoint;
			if (caravanAtTheFirstWaypoint != null)
			{
				return new CaravanTicksPerMoveUtility.CaravanInfo(caravanAtTheFirstWaypoint);
			}
			return null;
		}
	}

	private Caravan CaravanAtTheFirstWaypoint
	{
		get
		{
			if (!waypoints.Any())
			{
				return null;
			}
			return Find.WorldObjects.PlayerControlledCaravanAt(waypoints[0].Tile);
		}
	}

	public void Start(PlanetLayer planetLayer)
	{
		if (active)
		{
			Stop();
		}
		layer = planetLayer;
		active = true;
		if (Current.ProgramState == ProgramState.Playing)
		{
			AmbientSoundManager.EnsureWorldAmbientSoundCreated();
			Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			Find.TickManager.Pause();
		}
	}

	public void Start(Dialog_FormCaravan formCaravanDialog)
	{
		if (active)
		{
			Stop();
		}
		currentFormCaravanDialog = formCaravanDialog;
		caravanInfoFromFormCaravanDialog = new CaravanTicksPerMoveUtility.CaravanInfo(formCaravanDialog);
		formCaravanDialog.choosingRoute = true;
		Find.WindowStack.TryRemove(formCaravanDialog, doCloseSound: false);
		Start(formCaravanDialog.CurrentTile.Layer);
		TryAddWaypoint(formCaravanDialog.CurrentTile);
		cantRemoveFirstWaypoint = true;
	}

	public void Stop()
	{
		active = false;
		layer = null;
		for (int i = 0; i < waypoints.Count; i++)
		{
			waypoints[i].Destroy();
		}
		waypoints.Clear();
		cachedTicksToWaypoint.Clear();
		if (currentFormCaravanDialog != null)
		{
			currentFormCaravanDialog.Notify_NoLongerChoosingRoute();
		}
		caravanInfoFromFormCaravanDialog = null;
		currentFormCaravanDialog = null;
		cantRemoveFirstWaypoint = false;
		ReleasePaths();
	}

	public void WorldRoutePlannerUpdate()
	{
		if (active && ShouldStop)
		{
			Stop();
		}
		if (active)
		{
			for (int i = 0; i < paths.Count; i++)
			{
				paths[i].DrawPath(null);
			}
		}
	}

	public void WorldRoutePlannerOnGUI()
	{
		if (!active)
		{
			return;
		}
		if (KeyBindingDefOf.Cancel.KeyDownEvent)
		{
			if (currentFormCaravanDialog != null)
			{
				Find.WindowStack.Add(currentFormCaravanDialog);
			}
			else
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			Stop();
			Event.current.Use();
			return;
		}
		GenUI.DrawMouseAttachment(MouseAttachment);
		if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
		{
			PlanetTile tile = (Find.WorldSelector.SelectableObjectsUnderMouse().FirstOrDefault() as Caravan)?.Tile ?? GenWorld.MouseTile(snapToExpandableWorldObjects: true);
			if (tile.Valid)
			{
				RoutePlannerWaypoint waypoint = MostRecentWaypointAt(tile);
				if (waypoint != null)
				{
					RoutePlannerWaypoint routePlannerWaypoint = waypoint;
					List<RoutePlannerWaypoint> list = waypoints;
					if (routePlannerWaypoint == list[list.Count - 1])
					{
						TryRemoveWaypoint(waypoint);
					}
					else
					{
						List<FloatMenuOption> list2 = new List<FloatMenuOption>();
						list2.Add(new FloatMenuOption("AddWaypoint".Translate(), delegate
						{
							TryAddWaypoint(tile);
						}));
						list2.Add(new FloatMenuOption("RemoveWaypoint".Translate(), delegate
						{
							TryRemoveWaypoint(waypoint);
						}));
						Find.WindowStack.Add(new FloatMenu(list2));
					}
				}
				else
				{
					TryAddWaypoint(tile);
				}
				Event.current.Use();
			}
		}
		DoRouteDetailsBox();
		if (!DoChooseRouteButton())
		{
			DoTileTooltips();
		}
	}

	private void DoRouteDetailsBox()
	{
		Rect rect = new Rect(((float)UI.screenWidth - BottomWindowSize.x) / 2f, (float)UI.screenHeight - BottomWindowSize.y - 45f, BottomWindowSize.x, BottomWindowSize.y);
		if (Current.ProgramState == ProgramState.Entry)
		{
			rect.y -= 22f;
		}
		Find.WindowStack.ImmediateWindow(1373514241, rect, WindowLayer.Dialog, delegate
		{
			if (active)
			{
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Small;
				float num = 6f;
				if (waypoints.Count >= 2)
				{
					Widgets.Label(new Rect(0f, num, rect.width, 25f), "RoutePlannerEstTimeToFinalDest".Translate(GetTicksToWaypoint(waypoints.Count - 1).ToStringTicksToDays("0.#")));
				}
				else if (cantRemoveFirstWaypoint)
				{
					Widgets.Label(new Rect(0f, num, rect.width, 25f), "RoutePlannerAddOneOrMoreWaypoints".Translate());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, rect.width, 25f), "RoutePlannerAddTwoOrMoreWaypoints".Translate());
				}
				num += 20f;
				if (!CaravanInfo.HasValue || !CaravanInfo.Value.pawns.Any())
				{
					GUI.color = new Color(0.8f, 0.6f, 0.6f);
					Widgets.Label(new Rect(0f, num, rect.width, 25f), "RoutePlannerUsingAverageTicksPerMoveWarning".Translate());
				}
				else if (currentFormCaravanDialog == null && CaravanAtTheFirstWaypoint != null)
				{
					GUI.color = Color.gray;
					Widgets.Label(new Rect(0f, num, rect.width, 25f), "RoutePlannerUsingTicksPerMoveOfCaravan".Translate(CaravanAtTheFirstWaypoint.LabelCap));
				}
				num += 20f;
				GUI.color = Color.gray;
				Widgets.Label(new Rect(0f, num, rect.width, 25f), SteamDeck.IsSteamDeckInNonKeyboardMode ? "RoutePlannerPressRMBToAddAndRemoveWaypointsController".Translate() : "RoutePlannerPressRMBToAddAndRemoveWaypoints".Translate());
				num += 20f;
				if (currentFormCaravanDialog != null)
				{
					Widgets.Label(new Rect(0f, num, rect.width, 25f), SteamDeck.IsSteamDeckInNonKeyboardMode ? "RoutePlannerPressEscapeToReturnToCaravanFormationDialogController".Translate().Resolve().AdjustedForKeys() : "RoutePlannerPressEscapeToReturnToCaravanFormationDialog".Translate().Resolve());
				}
				else
				{
					Widgets.Label(new Rect(0f, num, rect.width, 25f), SteamDeck.IsSteamDeckInNonKeyboardMode ? "RoutePlannerPressEscapeToExitController".Translate().Resolve().AdjustedForKeys() : "RoutePlannerPressEscapeToExit".Translate().Resolve());
				}
				num += 20f;
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		});
	}

	private bool DoChooseRouteButton()
	{
		if (currentFormCaravanDialog == null || waypoints.Count < 2)
		{
			return false;
		}
		if (Widgets.ButtonText(new Rect(((float)UI.screenWidth - BottomButtonSize.x) / 2f, (float)UI.screenHeight - BottomWindowSize.y - 45f - 10f - BottomButtonSize.y, BottomButtonSize.x, BottomButtonSize.y), "Accept".Translate()))
		{
			Find.WindowStack.Add(currentFormCaravanDialog);
			currentFormCaravanDialog.Notify_ChoseRoute(waypoints[1].Tile);
			SoundDefOf.Click.PlayOneShotOnCamera();
			Stop();
			return true;
		}
		return false;
	}

	private void DoTileTooltips()
	{
		if (Mouse.IsInputBlockedNow)
		{
			return;
		}
		PlanetTile planetTile = GenWorld.MouseTile(snapToExpandableWorldObjects: true);
		if (!planetTile.Valid)
		{
			return;
		}
		for (int i = 0; i < paths.Count; i++)
		{
			if (paths[i].NodesReversed.Contains(planetTile))
			{
				string str = GetTileTip(planetTile, i);
				Text.Font = GameFont.Small;
				Vector2 size = Text.CalcSize(str);
				size.x += 20f;
				size.y += 20f;
				Vector2 mouseAttachedWindowPos = GenUI.GetMouseAttachedWindowPos(size.x, size.y);
				Rect rect = new Rect(mouseAttachedWindowPos, size);
				Find.WindowStack.ImmediateWindow(1859615246, rect, WindowLayer.Super, delegate
				{
					Text.Font = GameFont.Small;
					Widgets.Label(rect.AtZero().ContractedBy(10f), str);
				});
				break;
			}
		}
	}

	private string GetTileTip(PlanetTile tile, int pathIndex)
	{
		int num = paths[pathIndex].NodesReversed.IndexOf(tile);
		PlanetTile planetTile = ((num > 0) ? paths[pathIndex].NodesReversed[num - 1] : ((pathIndex >= paths.Count - 1 || paths[pathIndex + 1].NodeCount < 2) ? PlanetTile.Invalid : paths[pathIndex + 1].NodesReversed[paths[pathIndex + 1].NodeCount - 2]));
		int num2 = cachedTicksToWaypoint[pathIndex] + CaravanArrivalTimeEstimator.EstimatedTicksToArrive(paths[pathIndex].FirstNode, tile, paths[pathIndex], 0f, CaravanTicksPerMove, GenTicks.TicksAbs + cachedTicksToWaypoint[pathIndex]);
		int num3 = GenTicks.TicksAbs + num2;
		StringBuilder stringBuilder = new StringBuilder();
		if (num2 != 0)
		{
			stringBuilder.AppendLine("EstimatedTimeToTile".Translate(num2.ToStringTicksToDays("0.##")));
		}
		stringBuilder.AppendLine("ForagedFoodAmount".Translate() + ": " + Find.WorldGrid[tile].PrimaryBiome.forageability.ToStringPercent());
		stringBuilder.Append(VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(tile, num3));
		if (planetTile.Valid)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			StringBuilder stringBuilder2 = new StringBuilder();
			float num4 = WorldPathGrid.CalculatedMovementDifficultyAt(planetTile, perceivedStatic: false, num3, stringBuilder2);
			float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(tile, planetTile, stringBuilder2);
			stringBuilder.Append("TileMovementDifficulty".Translate() + ":\n" + stringBuilder2.ToString().Indented("  "));
			stringBuilder.AppendLine();
			stringBuilder.Append("  = ");
			stringBuilder.Append((num4 * roadMovementDifficultyMultiplier).ToString("0.#"));
		}
		return stringBuilder.ToString();
	}

	public void DoRoutePlannerButton(ref float curBaseY)
	{
		float num = 57f;
		float num2 = 33f;
		Rect rect = new Rect((float)UI.screenWidth - 10f - num, curBaseY - 10f - num2, num, num2);
		if (Widgets.ButtonImage(rect, ButtonTex, Color.white, new Color(0.8f, 0.8f, 0.8f)))
		{
			if (active)
			{
				Stop();
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			else
			{
				Start(Find.WorldSelector.SelectedLayer);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		}
		TooltipHandler.TipRegionByKey(rect, "RoutePlannerButtonTip");
		curBaseY -= num2 + 20f;
	}

	public int GetTicksToWaypoint(int index)
	{
		return cachedTicksToWaypoint[index];
	}

	public void TryAddWaypoint(PlanetTile tile, bool playSound = true)
	{
		if (tile.Layer != layer)
		{
			Messages.Message("MessageCantAddWaypointBecauseUnreachable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (Find.World.Impassable(tile))
		{
			Messages.Message("MessageCantAddWaypointBecauseImpassable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (waypoints.Any())
		{
			WorldReachability worldReachability = Find.WorldReachability;
			List<RoutePlannerWaypoint> list = waypoints;
			if (!worldReachability.CanReach(list[list.Count - 1].Tile, tile))
			{
				Messages.Message("MessageCantAddWaypointBecauseUnreachable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
		}
		if (waypoints.Count >= 25)
		{
			Messages.Message("MessageCantAddWaypointBecauseLimit".Translate(25), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		RoutePlannerWaypoint routePlannerWaypoint = (RoutePlannerWaypoint)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.RoutePlannerWaypoint);
		routePlannerWaypoint.Tile = tile;
		Find.WorldObjects.Add(routePlannerWaypoint);
		waypoints.Add(routePlannerWaypoint);
		RecreatePaths();
		if (playSound)
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
		}
	}

	public void TryRemoveWaypoint(RoutePlannerWaypoint point, bool playSound = true)
	{
		if (cantRemoveFirstWaypoint && waypoints.Any() && point == waypoints[0])
		{
			Messages.Message("MessageCantRemoveWaypointBecauseFirst".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		point.Destroy();
		waypoints.Remove(point);
		for (int num = waypoints.Count - 1; num >= 1; num--)
		{
			if (waypoints[num].Tile == waypoints[num - 1].Tile)
			{
				waypoints[num].Destroy();
				waypoints.RemoveAt(num);
			}
		}
		RecreatePaths();
		if (playSound)
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}
	}

	private void ReleasePaths()
	{
		for (int i = 0; i < paths.Count; i++)
		{
			paths[i].ReleaseToPool();
		}
		paths.Clear();
	}

	private void RecreatePaths()
	{
		ReleasePaths();
		if (waypoints.NullOrEmpty())
		{
			return;
		}
		WorldPathing pather = waypoints[0].Tile.Layer.Pather;
		for (int i = 1; i < waypoints.Count; i++)
		{
			paths.Add(pather.FindPath(waypoints[i - 1].Tile, waypoints[i].Tile, null));
		}
		cachedTicksToWaypoint.Clear();
		int num = 0;
		int caravanTicksPerMove = CaravanTicksPerMove;
		for (int j = 0; j < waypoints.Count; j++)
		{
			if (j == 0)
			{
				cachedTicksToWaypoint.Add(0);
				continue;
			}
			num += CaravanArrivalTimeEstimator.EstimatedTicksToArrive(waypoints[j - 1].Tile, waypoints[j].Tile, paths[j - 1], 0f, caravanTicksPerMove, GenTicks.TicksAbs + num);
			cachedTicksToWaypoint.Add(num);
		}
	}

	private RoutePlannerWaypoint MostRecentWaypointAt(PlanetTile tile)
	{
		for (int num = waypoints.Count - 1; num >= 0; num--)
		{
			if (waypoints[num].Tile == tile)
			{
				return waypoints[num];
			}
		}
		return null;
	}
}
