using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class CellInspectorDrawer
{
	private static int numLines;

	private const float DistFromMouse = 26f;

	private const float LabelColumnWidth = 130f;

	private const float InfoColumnWidth = 170f;

	private const float WindowPadding = 12f;

	private const float ColumnPadding = 12f;

	private const float LineHeight = 24f;

	private const float ThingIconSize = 22f;

	private const float WindowWidth = 336f;

	public static bool active;

	public static void Update()
	{
		if (!KeyBindingDefOf.ShowCellInspector.IsDown)
		{
			active = false;
			return;
		}
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TileInspector, KnowledgeAmount.TinyInteraction);
		active = true;
		if (ShouldShow() && !WorldRendererUtility.WorldSelected)
		{
			GenUI.RenderMouseoverBracket();
		}
	}

	public static void OnGUI()
	{
		if (ShouldShow() && !Mouse.IsInputBlockedNow)
		{
			Rect rect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 336f, (float)numLines * 24f + 24f);
			numLines = 0;
			rect.x += 26f;
			rect.y += 26f;
			if (rect.xMax > (float)UI.screenWidth)
			{
				rect.x -= rect.width + 52f;
			}
			if (rect.yMax > (float)UI.screenHeight)
			{
				rect.y -= rect.height + 52f;
			}
			Find.WindowStack.ImmediateWindow(62348, rect, WindowLayer.Super, FillWindow);
		}
	}

	private static void FillWindow()
	{
		if (ShouldShow())
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			if (WorldRendererUtility.WorldSelected)
			{
				DrawWorldInspector();
			}
			else
			{
				DrawMapInspector();
			}
			Text.WordWrap = true;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	private static void DrawMapInspector()
	{
		IntVec3 intVec = UI.MouseCell();
		List<Thing> list = (from thing in intVec.GetThingList(Find.CurrentMap)
			where thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Filth && thing.def.category != ThingCategory.Ethereal && (!(thing is Pawn pawn) || !pawn.IsHiddenFromPlayer())
			select thing).ToList();
		if (list.Any())
		{
			foreach (Thing item in list)
			{
				DrawThingRow(item);
			}
		}
		IEnumerable<string> enumerable = from filth in intVec.GetThingList(Find.CurrentMap)
			where filth.def.category == ThingCategory.Filth
			select filth.def.label;
		if (enumerable.Any())
		{
			DrawRow("Filth_Label".Translate(), enumerable.ToCommaList().CapitalizeFirst().Truncate(170f));
		}
		if (list.Any() || enumerable.Any())
		{
			DrawDivider();
		}
		Room room = intVec.GetRoom(Find.CurrentMap);
		if (room != null && room.Role != RoomRoleDefOf.None)
		{
			DrawHeader(room.GetRoomRoleLabel().CapitalizeFirst());
			foreach (RoomStatDef item2 in DefDatabase<RoomStatDef>.AllDefsListForReading)
			{
				if (!item2.isHidden || DebugViewSettings.showAllRoomStats)
				{
					float stat = room.GetStat(item2);
					RoomStatScoreStage scoreStage = item2.GetScoreStage(stat);
					DrawRow(item2.LabelCap, (scoreStage == null) ? "" : (scoreStage.label.CapitalizeFirst() + " (" + item2.ScoreToString(stat) + ")"));
				}
			}
			DrawDivider();
		}
		TerrainDef terrain = intVec.GetTerrain(Find.CurrentMap);
		bool flag = intVec.IsPolluted(Find.CurrentMap);
		float fertility = intVec.GetFertility(Find.CurrentMap);
		float temperature = intVec.GetTemperature(Find.CurrentMap);
		float value = Find.CurrentMap.glowGrid.GroundGlowAt(intVec);
		Zone zone = intVec.GetZone(Find.CurrentMap);
		float depth = Find.CurrentMap.snowGrid.GetDepth(intVec);
		float num = (ModsConfig.OdysseyActive ? Find.CurrentMap.sandGrid.GetDepth(intVec) : 0f);
		WeatherBuildupCategory buildupCategory = WeatherBuildupUtility.GetBuildupCategory(depth);
		WeatherBuildupCategory buildupCategory2 = WeatherBuildupUtility.GetBuildupCategory(num);
		RoofDef roof = intVec.GetRoof(Find.CurrentMap);
		byte b = Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.BlindSmoke);
		byte b2 = Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.ToxGas);
		byte b3 = Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.RotStink);
		byte b4 = Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.DeadlifeDust);
		float num2 = BeautyUtility.AverageBeautyPerceptible(intVec, Find.CurrentMap);
		if (ModsConfig.OdysseyActive)
		{
			WaterBody waterBody = Find.CurrentMap.waterBodyTracker.WaterBodyAt(intVec);
			if (waterBody != null && waterBody.HasFish)
			{
				IEnumerable<ThingDef> commonFishIncludingExtras = waterBody.CommonFishIncludingExtras;
				IEnumerable<ThingDef> uncommonFish = waterBody.UncommonFish;
				DrawRow(info: (from x in commonFishIncludingExtras.Concat(uncommonFish)
					select x.label).ToCommaList().CapitalizeFirst(), label: "Fish".Translate());
			}
		}
		DrawRow("Beauty_Label".Translate(), num2.ToString("F1"));
		if (zone != null)
		{
			DrawRow("Zone_Label".Translate(), zone.label);
		}
		if (roof != null)
		{
			DrawRow("Roof_Label".Translate(), roof.LabelCap);
		}
		DrawRow("Terrain_Label".Translate(), flag ? "PollutedTerrain".Translate(terrain.label).CapitalizeFirst() : terrain.LabelCap);
		if (depth > 0.03f)
		{
			DrawRow("Snow_Label".Translate(), WeatherBuildupUtility.GetSnowDescription(buildupCategory).CapitalizeFirst());
		}
		if (num > 0.03f)
		{
			DrawRow("Sand_Label".Translate(), WeatherBuildupUtility.GetSandDescription(buildupCategory2).CapitalizeFirst());
		}
		DrawRow("WalkSpeed_Label".Translate(), GenPath.SpeedPercentString(Mathf.Max(terrain.pathCost, WeatherBuildupUtility.MovementTicksAddOn(buildupCategory))));
		if ((double)fertility > 0.0001)
		{
			DrawRow("Fertility_Label".Translate(), fertility.ToStringPercent());
		}
		DrawRow("Temperature_Label".Translate(), temperature.ToStringTemperature("F0"));
		DrawRow("LightLevel_Label".Translate(), MouseoverUtility.GetGlowLabelByValue(value));
		if (b > 0)
		{
			DrawRow(GasType.BlindSmoke.GetLabel().CapitalizeFirst(), ((float)(int)b / 255f).ToStringPercent("F0"));
		}
		if (b2 > 0)
		{
			DrawRow(GasType.ToxGas.GetLabel().CapitalizeFirst(), ((float)(int)b2 / 255f).ToStringPercent("F0"));
		}
		if (b3 > 0)
		{
			DrawRow(GasType.RotStink.GetLabel().CapitalizeFirst(), ((float)(int)b3 / 255f).ToStringPercent("F0"));
		}
		if (b4 > 0)
		{
			DrawRow(GasType.DeadlifeDust.GetLabel().CapitalizeFirst(), ((float)(int)b4 / 255f).ToStringPercent("F0"));
		}
	}

	private static void DrawWorldInspector()
	{
		List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
		PlanetTile mouseTileIndex = GenWorld.MouseTile();
		Tile tile = Find.WorldGrid[mouseTileIndex];
		foreach (WorldObject item in list)
		{
			DrawHeader(item.LabelCap);
			WorldObject worldObject = item;
			if (!(worldObject is Settlement settlement))
			{
				if (worldObject is Caravan caravan)
				{
					DrawRow("CaravanColonists_Label".Translate(), caravan.pawns.Count((Pawn pawn) => pawn.IsColonist).ToString());
					if (caravan.pather.Moving)
					{
						if (!caravan.pather.MovingNow)
						{
							DrawRow("CaravanStatus_Label".Translate(), CaravanBedUtility.AppendUsingBedsLabel("CaravanResting".Translate(), caravan.beds.GetUsedBedCount()));
						}
						else if (caravan.pather.ArrivalAction != null)
						{
							DrawRow("CaravanStatus_Label".Translate(), caravan.pather.ArrivalAction.ReportString);
						}
						else
						{
							DrawRow("CaravanStatus_Label".Translate(), "CaravanTraveling".Translate());
						}
						float num = (float)CaravanArrivalTimeEstimator.EstimatedTicksToArrive(caravan, allowCaching: true) / 60000f;
						DrawRow("CaravanTTD_Label".Translate(), num.ToString("0.#"));
					}
					else
					{
						Settlement settlement2 = CaravanVisitUtility.SettlementVisitedNow(caravan);
						if (settlement2 != null)
						{
							DrawRow("CaravanStatus_Label".Translate(), "CaravanVisiting".Translate(settlement2.Label));
						}
						else
						{
							DrawRow("CaravanStatus_Label".Translate(), "CaravanWaiting".Translate());
						}
					}
				}
			}
			else if (settlement.Faction != null)
			{
				DrawRow("Faction_Label".Translate(), settlement.Faction.Name);
				if (settlement.Faction != Faction.OfPlayer)
				{
					if (settlement.Faction.Hidden)
					{
						DrawRow("Relationship_Label".Translate(), settlement.Faction.PlayerRelationKind.GetLabelCap());
					}
					else
					{
						DrawRow("Relationship_Label".Translate(), settlement.Faction.PlayerRelationKind.GetLabelCap() + " (" + settlement.Faction.PlayerGoodwill.ToStringWithSign() + ")");
					}
				}
			}
			DrawDivider();
		}
		DrawRow("Biome_Label".Translate(), tile.PrimaryBiome.LabelCap);
		if (tile.Mutators.Any())
		{
			DrawRow("Features_Label".Translate(), tile.Mutators.Select((TileMutatorDef mutator) => mutator.Label(mouseTileIndex)).ToCommaList().CapitalizeFirst()
				.Truncate(170f));
		}
		if (!tile.PrimaryBiome.impassable)
		{
			DrawRow("Hilliness_Label".Translate(), tile.hilliness.GetLabelCap());
		}
		if (tile is SurfaceTile surfaceTile)
		{
			if (surfaceTile.Roads != null)
			{
				DrawRow("Road_Label".Translate(), surfaceTile.Roads.Select((SurfaceTile.RoadLink rl) => rl.road).MaxBy((RoadDef road) => road.priority).LabelCap);
			}
			if (surfaceTile.Rivers != null)
			{
				DrawRow("River_Label".Translate(), surfaceTile.Rivers[0].river.LabelCap);
			}
		}
		if (!Find.World.Impassable(mouseTileIndex))
		{
			string info = (WorldPathGrid.CalculatedMovementDifficultyAt(mouseTileIndex, perceivedStatic: false) * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(mouseTileIndex, PlanetTile.Invalid)).ToString("0.#");
			DrawRow("MovementDifficulty_Label".Translate(), info);
		}
		if (ModsConfig.BiotechActive && tile.pollution > 0f)
		{
			DrawRow("Pollution_Label".Translate(), GenWorld.GetPollutionDescription(tile.pollution) + " (" + tile.pollution.ToStringPercent() + ")");
		}
	}

	private static bool ShouldShow()
	{
		if (Current.ProgramState != ProgramState.Playing)
		{
			return false;
		}
		if (WorldRendererUtility.WorldSelected)
		{
			if (!GenWorld.MouseTile().Valid)
			{
				return false;
			}
		}
		else if (Find.CurrentMap != null && (!UI.MouseCell().InBounds(Find.CurrentMap) || UI.MouseCell().Fogged(Find.CurrentMap)))
		{
			return false;
		}
		return active;
	}

	private static void DrawThingRow(Thing thing)
	{
		float num = (float)numLines * 24f;
		CompSelectProxy compSelectProxy = thing.TryGetComp<CompSelectProxy>();
		if (compSelectProxy != null && compSelectProxy.thingToSelect != null)
		{
			thing = compSelectProxy.thingToSelect;
		}
		List<object> selectedObjects = Find.Selector.SelectedObjects;
		Rect rect = new Rect(12f, num + 12f, 312f, 24f);
		if (selectedObjects.Contains(thing))
		{
			Widgets.DrawHighlight(rect);
		}
		else if (numLines % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		rect = new Rect(24f, num + 12f + 1f, 22f, 22f);
		float scale;
		float angle;
		Vector2 iconProportions;
		Color color;
		Material material;
		if (thing is Blueprint || thing is Frame)
		{
			Widgets.DefIcon(rect, thing.def);
		}
		else if (thing is Pawn || thing is Corpse)
		{
			Widgets.ThingIcon(rect.ExpandedBy(5f), thing);
		}
		else if (Widgets.GetIconFor(thing, new Vector2(rect.width, rect.height), null, stackOfOne: false, out scale, out angle, out iconProportions, out color, out material) != BaseContent.BadTex)
		{
			Widgets.ThingIcon(rect, thing);
		}
		rect = new Rect(58f, num + 12f, 370f, 24f);
		Widgets.LabelEllipses(rect, thing.LabelMouseover);
		numLines++;
	}

	private static void DrawRow(string label, string info)
	{
		float num = (float)numLines * 24f;
		Rect rect = new Rect(12f, num + 12f, 312f, 24f);
		if (numLines % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		GUI.color = Color.gray;
		rect = new Rect(24f, num + 12f, 130f, 24f);
		Widgets.Label(rect, label);
		GUI.color = Color.white;
		rect = new Rect(154f, num + 12f, 170f, 24f);
		Widgets.LabelEllipses(rect, info);
		numLines++;
	}

	private static void DrawHeader(string text)
	{
		float num = (float)numLines * 24f;
		Rect rect = new Rect(12f, num + 12f - 8f, 312f, 28f);
		Text.Anchor = TextAnchor.UpperCenter;
		Text.Font = GameFont.Medium;
		Widgets.Label(rect, text);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		numLines++;
	}

	private static void DrawDivider()
	{
		float num = (float)numLines * 24f;
		GUI.color = Color.gray;
		Widgets.DrawLineHorizontal(0f, num + 12f + 12f, 336f);
		GUI.color = Color.white;
		numLines++;
	}
}
