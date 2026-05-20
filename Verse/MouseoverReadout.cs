using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class MouseoverReadout
{
	private TerrainDef cachedTerrain;

	private bool cachedPolluted;

	private float cachedVacuum;

	private string cachedTerrainString;

	private string cachedVacuumString;

	private const float YInterval = 19f;

	private static readonly Vector2 BotLeft = new Vector2(15f, 65f);

	private bool ShouldShow
	{
		get
		{
			if (Find.MainTabsRoot.OpenTab != null)
			{
				return false;
			}
			return true;
		}
	}

	public void MouseoverReadoutOnGUI()
	{
		if (Event.current.type != EventType.Repaint || !ShouldShow)
		{
			return;
		}
		GenUI.DrawTextWinterShadow(new Rect(256f, UI.screenHeight - 256, -256f, 256f));
		Text.Font = GameFont.Small;
		GUI.color = new Color(1f, 1f, 1f, 0.8f);
		IntVec3 intVec = UI.MouseCell();
		if (!intVec.InBounds(Find.CurrentMap))
		{
			return;
		}
		float num = 0f;
		if (intVec.Fogged(Find.CurrentMap))
		{
			Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), "Undiscovered".Translate());
			GUI.color = Color.white;
			return;
		}
		Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), MouseoverUtility.GetGlowLabelByValue(Find.CurrentMap.glowGrid.GroundGlowAt(intVec)));
		num += 19f;
		if (Find.CurrentMap.Biome.inVacuum)
		{
			using (ProfilerBlock.Scope("vacuum"))
			{
				Rect rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				float vacuum = intVec.GetVacuum(Find.CurrentMap);
				if (!Mathf.Approximately(vacuum, cachedVacuum))
				{
					cachedVacuumString = string.Format("{0} ({1})", "Vacuum".Translate().CapitalizeFirst(), vacuum.ToStringPercent("0"));
					cachedVacuum = vacuum;
				}
				Widgets.Label(rect, cachedVacuumString);
				num += 19f;
			}
		}
		Rect rect2 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
		TerrainDef terrain = intVec.GetTerrain(Find.CurrentMap);
		bool flag = intVec.IsPolluted(Find.CurrentMap);
		if (terrain != cachedTerrain || flag != cachedPolluted)
		{
			float fertility = intVec.GetFertility(Find.CurrentMap);
			string text = (((double)fertility > 0.0001) ? (" " + "FertShort".TranslateSimple() + " " + fertility.ToStringPercent()) : "");
			TaggedString taggedString = (flag ? "PollutedTerrain".Translate(terrain.label).CapitalizeFirst() : terrain.LabelCap);
			cachedTerrainString = taggedString + ((terrain.passability != Traversability.Impassable) ? string.Format(" ({0})", "WalkSpeed".Translate(GenPath.SpeedPercentString(terrain.pathCost)) + text) : null);
			cachedTerrain = terrain;
			cachedPolluted = flag;
		}
		Widgets.Label(rect2, cachedTerrainString);
		num += 19f;
		Zone zone = intVec.GetZone(Find.CurrentMap);
		if (zone != null)
		{
			Rect rect3 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
			string label = zone.label;
			Widgets.Label(rect3, label);
			num += 19f;
		}
		float depth = Find.CurrentMap.snowGrid.GetDepth(intVec);
		if (depth > 0.03f)
		{
			Rect rect4 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
			WeatherBuildupCategory buildupCategory = WeatherBuildupUtility.GetBuildupCategory(depth);
			string label2 = "Snow".Translate() + " (" + WeatherBuildupUtility.GetSnowDescription(buildupCategory) + ")" + " (" + "WalkSpeed".Translate(GenPath.SpeedPercentString(WeatherBuildupUtility.MovementTicksAddOn(buildupCategory))) + ")";
			Widgets.Label(rect4, label2);
			num += 19f;
		}
		if (ModsConfig.OdysseyActive)
		{
			float depth2 = Find.CurrentMap.sandGrid.GetDepth(intVec);
			if (depth2 > 0.03f)
			{
				Rect rect5 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				WeatherBuildupCategory buildupCategory2 = WeatherBuildupUtility.GetBuildupCategory(depth2);
				string label3 = "Sand".Translate() + " (" + WeatherBuildupUtility.GetSandDescription(buildupCategory2) + ")" + " (" + "WalkSpeed".Translate(GenPath.SpeedPercentString(WeatherBuildupUtility.MovementTicksAddOn(buildupCategory2))) + ")";
				Widgets.Label(rect5, label3);
				num += 19f;
			}
		}
		List<Thing> thingList = intVec.GetThingList(Find.CurrentMap);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			CompSelectProxy compSelectProxy = thing.TryGetComp<CompSelectProxy>();
			if (compSelectProxy != null && compSelectProxy.thingToSelect != null)
			{
				thing = compSelectProxy.thingToSelect;
			}
			if (thing.def.category != ThingCategory.Mote && (!(thing is Pawn pawn) || !pawn.IsHiddenFromPlayer()))
			{
				Rect rect6 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string labelMouseover = thing.LabelMouseover;
				Widgets.Label(rect6, labelMouseover);
				num += 19f;
			}
		}
		RoofDef roof = intVec.GetRoof(Find.CurrentMap);
		if (roof != null)
		{
			Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), roof.LabelCap);
			num += 19f;
		}
		if (Find.CurrentMap.gasGrid.AnyGasAt(intVec))
		{
			DrawGas(GasType.BlindSmoke, Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.BlindSmoke), ref num);
			DrawGas(GasType.ToxGas, Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.ToxGas), ref num);
			DrawGas(GasType.RotStink, Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.RotStink), ref num);
			DrawGas(GasType.DeadlifeDust, Find.CurrentMap.gasGrid.DensityAt(intVec, GasType.DeadlifeDust), ref num);
		}
		if (ModsConfig.OdysseyActive && Find.CurrentMap.waterBodyTracker.TryGetWaterBodyAt(intVec, out var body) && body.HasFish)
		{
			float population = body.Population;
			float maxPopulation = body.MaxPopulation;
			IEnumerable<ThingDef> commonFishIncludingExtras = body.CommonFishIncludingExtras;
			IEnumerable<ThingDef> uncommonFish = body.UncommonFish;
			IEnumerable<ThingDef> source = commonFishIncludingExtras.Concat(uncommonFish);
			TaggedString taggedString2 = "Fish".Translate().CapitalizeFirst();
			string text2 = source.Select((ThingDef x) => x.label).ToCommaList().CapitalizeFirst();
			string text3 = string.Empty;
			GameCondition_GillRot activeCondition = Find.CurrentMap.gameConditionManager.GetActiveCondition<GameCondition_GillRot>();
			if (activeCondition != null && !activeCondition.HiddenByOtherCondition(Find.CurrentMap))
			{
				text3 = " (" + activeCondition.LabelCap + ")";
			}
			Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), $"{taggedString2}: {text2} ({population:F0}/{maxPopulation:F0}){text3}");
			num += 19f;
		}
		GUI.color = Color.white;
	}

	private void DrawGas(GasType gasType, byte density, ref float curYOffset)
	{
		if (density > 0)
		{
			Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - curYOffset, 999f, 999f), gasType.GetLabel().CapitalizeFirst() + " " + ((float)(int)density / 255f).ToStringPercent("F0"));
			curYOffset += 19f;
		}
	}
}
