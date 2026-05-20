using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;

namespace RimWorld;

public class Page_CreateWorldParams : Page
{
	private bool initialized;

	private string seedString;

	private float planetCoverage;

	private OverallRainfall rainfall;

	private OverallTemperature temperature;

	private OverallPopulation population;

	private LandmarkDensity landmarkDensity;

	public float pollution;

	private List<FactionDef> factions;

	private List<FactionDef> initialFactions;

	private static readonly float[] PlanetCoverages = new float[3] { 0.3f, 0.5f, 1f };

	private static readonly float[] PlanetCoveragesDev = new float[4] { 0.3f, 0.5f, 1f, 0.05f };

	private const float LabelWidth = 200f;

	public override string PageTitle => "CreateWorld".Translate();

	public override void PreOpen()
	{
		base.PreOpen();
		if (!initialized)
		{
			Reset();
			initialized = true;
		}
	}

	public override void PostOpen()
	{
		base.PostOpen();
		TutorSystem.Notify_Event("PageStart-CreateWorldParams");
	}

	public void Reset()
	{
		seedString = GenText.RandomSeedString();
		planetCoverage = ((Prefs.DevMode && UnityData.isEditor) ? 0.05f : (ModsConfig.OdysseyActive ? 0.5f : 0.3f));
		rainfall = OverallRainfall.Normal;
		temperature = OverallTemperature.Normal;
		population = OverallPopulation.Normal;
		landmarkDensity = LandmarkDensity.Normal;
		pollution = (ModsConfig.BiotechActive ? 0.05f : 0f);
		ResetFactionCounts();
	}

	private void ResetFactionCounts()
	{
		factions = new List<FactionDef>();
		foreach (FactionDef configurableFaction in FactionGenerator.ConfigurableFactions)
		{
			if (configurableFaction.startingCountAtWorldCreation > 0)
			{
				for (int i = 0; i < configurableFaction.startingCountAtWorldCreation; i++)
				{
					factions.Add(configurableFaction);
				}
			}
		}
		foreach (FactionDef faction in FactionGenerator.ConfigurableFactions)
		{
			if (faction.replacesFaction != null)
			{
				factions.RemoveAll((FactionDef x) => x == faction.replacesFaction);
			}
		}
		initialFactions = new List<FactionDef>();
		initialFactions.AddRange(factions);
	}

	public override void DoWindowContents(Rect rect)
	{
		DrawPageTitle(rect);
		Rect mainRect = GetMainRect(rect);
		float num = (mainRect.width - Margin) * 0.5f;
		Rect rect2 = new Rect(mainRect.x, mainRect.y, num, mainRect.height);
		Widgets.BeginGroup(rect2);
		Text.Font = GameFont.Small;
		float num2 = 0f;
		float width = rect2.width - 200f;
		Widgets.Label(new Rect(0f, num2, 200f, 30f), "WorldSeed".Translate());
		Rect rect3 = new Rect(200f, num2, width, 30f);
		seedString = Widgets.TextField(rect3, seedString);
		num2 += 40f;
		if (Widgets.ButtonText(new Rect(200f, num2, width, 30f), "RandomizeSeed".Translate()))
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			seedString = GenText.RandomSeedString();
		}
		num2 += 40f;
		Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetCoverage".Translate());
		Rect rect4 = new Rect(200f, num2, width, 30f);
		if (Widgets.ButtonText(rect4, planetCoverage.ToStringPercent()))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			float[] array = (Prefs.DevMode ? PlanetCoveragesDev : PlanetCoverages);
			foreach (float coverage in array)
			{
				string text = coverage.ToStringPercent();
				if (coverage <= 0.1f)
				{
					text += " (dev)";
				}
				FloatMenuOption item = new FloatMenuOption(text, delegate
				{
					if (planetCoverage != coverage)
					{
						planetCoverage = coverage;
						if (planetCoverage == 1f)
						{
							Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
						}
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		TooltipHandler.TipRegionByKey(new Rect(0f, num2, rect4.xMax, rect4.height), "PlanetCoverageTip");
		num2 += 40f;
		Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetRainfall".Translate());
		Rect rect5 = new Rect(200f, num2, width, 30f);
		rainfall = (OverallRainfall)Mathf.RoundToInt(Widgets.HorizontalSlider(rect5, (float)rainfall, 0f, OverallRainfallUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
		num2 += 40f;
		Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetTemperature".Translate());
		Rect rect6 = new Rect(200f, num2, width, 30f);
		temperature = (OverallTemperature)Mathf.RoundToInt(Widgets.HorizontalSlider(rect6, (float)temperature, 0f, OverallTemperatureUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
		num2 += 40f;
		Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetPopulation".Translate());
		Rect rect7 = new Rect(200f, num2, width, 30f);
		population = (OverallPopulation)Mathf.RoundToInt(Widgets.HorizontalSlider(rect7, (float)population, 0f, OverallPopulationUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
		if (ModsConfig.OdysseyActive)
		{
			num2 += 40f;
			Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetLandmarkDensity".Translate());
			Rect rect8 = new Rect(200f, num2, width, 30f);
			landmarkDensity = (LandmarkDensity)Mathf.RoundToInt(Widgets.HorizontalSlider(rect8, (float)landmarkDensity, 0f, LandmarkDensityUtility.EnumValuesCount - 1, middleAlignment: true, "PlanetLandmarkDensity_Normal".Translate(), "PlanetLandmarkDensity_Low".Translate(), "PlanetLandmarkDensity_High".Translate(), 1f));
		}
		if (ModsConfig.BiotechActive)
		{
			num2 += 40f;
			Widgets.Label(new Rect(0f, num2, 200f, 30f), "PlanetPollution".Translate());
			Rect rect9 = new Rect(200f, num2, width, 30f);
			pollution = Widgets.HorizontalSlider(rect9, pollution, 0f, 1f, middleAlignment: true, pollution.ToStringPercent(), null, null, 0.05f);
		}
		if (!TutorSystem.TutorialMode)
		{
			num2 += 40f;
			Widgets.Label(new Rect(0f, num2, 200f, 30f), "AdvancedSettings".Translate());
			if (Widgets.ButtonText(new Rect(200f, num2, width, 30f), "Edit".Translate() + "..."))
			{
				Find.WindowStack.Add(new Dialog_AdvancedGameConfig());
			}
		}
		Widgets.EndGroup();
		WorldFactionsUIUtility.DoWindowContents(new Rect(mainRect.x + mainRect.xMax - num, mainRect.y, num, mainRect.height), isDefaultFactionCounts: factions.SetsEqual(initialFactions), factions: factions);
		float y = rect.yMax - 38f;
		float x = mainRect.center.x;
		Rect rect10 = new Rect(x - Page.BottomButSize.x - 8.5f, y, Page.BottomButSize.x, Page.BottomButSize.y);
		if (Widgets.ButtonText(rect10, "ResetAll".Translate()))
		{
			Reset();
		}
		rect10.x = x + 8.5f;
		if (Widgets.ButtonText(rect10, "ResetFactions".Translate()))
		{
			ResetFactionCounts();
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		}
		DoBottomButtons(rect, "WorldGenerate".Translate());
	}

	protected override bool CanDoNext()
	{
		if (!base.CanDoNext())
		{
			return false;
		}
		LongEventHandler.QueueLongEvent(delegate
		{
			Find.GameInitData.ResetWorldRelatedMapInitData();
			Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, rainfall, temperature, population, landmarkDensity, factions, pollution);
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (next != null)
				{
					Find.WindowStack.Add(next);
				}
				MemoryUtility.UnloadUnusedUnityAssets();
				Find.World.renderer.RegenerateAllLayersNow();
				Close();
			});
		}, "GeneratingWorld", doAsynchronously: true, null);
		return false;
	}
}
