using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlantProperties
	{
		public List<PlantBiomeRecord> wildBiomes;

		public int wildClusterRadius = -1;

		public float wildClusterWeight = 15f;

		public float wildOrder = 2f;

		public bool wildEqualLocalDistribution = true;

		public bool cavePlant;

		public float cavePlantWeight = 1f;

		[NoTranslate]
		public List<string> sowTags = new List<string>();

		public float sowWork = 10f;

		public int sowMinSkill;

		public bool blockAdjacentSow;

		public List<ResearchProjectDef> sowResearchPrerequisites;

		public bool mustBeWildToSow;

		public float harvestWork = 10f;

		public float harvestYield;

		public ThingDef harvestedThingDef;

		[NoTranslate]
		public string harvestTag;

		public float harvestMinGrowth = 0.65f;

		public float harvestAfterGrowth;

		public bool harvestFailable = true;

		public SoundDef soundHarvesting;

		public SoundDef soundHarvestFinish;

		public float growDays = 2f;

		public float lifespanDaysPerGrowDays = 8f;

		public float growMinGlow = 0.51f;

		public float growOptimalGlow = 1f;

		public float fertilityMin = 0.9f;

		public float fertilitySensitivity = 0.5f;

		public bool dieIfLeafless;

		public bool neverBlightable;

		public bool interferesWithRoof;

		public bool dieIfNoSunlight = true;

		public bool dieFromToxicFallout = true;

		public PlantPurpose purpose = PlantPurpose.Misc;

		public float topWindExposure = 0.25f;

		public int maxMeshCount = 1;

		public FloatRange visualSizeRange = new FloatRange(0.9f, 1.1f);

		[NoTranslate]
		private string leaflessGraphicPath;

		[Unsaved(false)]
		public Graphic leaflessGraphic;

		[NoTranslate]
		private string immatureGraphicPath;

		[Unsaved(false)]
		public Graphic immatureGraphic;

		public bool dropLeaves;

		public const int MaxMaxMeshCount = 25;

		public bool Sowable => !sowTags.NullOrEmpty();

		public bool Harvestable => harvestYield > 0.001f;

		public bool HarvestDestroys => harvestAfterGrowth <= 0f;

		public bool IsTree => harvestTag == "Wood";

		public float LifespanDays => growDays * lifespanDaysPerGrowDays;

		public int LifespanTicks => (int)(LifespanDays * 60000f);

		public bool LimitedLifespan => lifespanDaysPerGrowDays > 0f;

		public bool Blightable
		{
			get
			{
				if (Sowable && Harvestable)
				{
					return !neverBlightable;
				}
				return false;
			}
		}

		public bool GrowsInClusters => wildClusterRadius > 0;

		public void PostLoadSpecial(ThingDef parentDef)
		{
			if (!leaflessGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					leaflessGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, leaflessGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
			if (!immatureGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					immatureGraphic = GraphicDatabase.Get(parentDef.graphicData.graphicClass, immatureGraphicPath, parentDef.graphic.Shader, parentDef.graphicData.drawSize, parentDef.graphicData.color, parentDef.graphicData.colorTwo);
				});
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (maxMeshCount > 25)
			{
				yield return "maxMeshCount > MaxMaxMeshCount";
			}
		}

		private IEnumerable<Dialog_InfoCard.Hyperlink> GetHarvestYieldHyperlinks()
		{
			yield return new Dialog_InfoCard.Hyperlink(harvestedThingDef);
		}

		internal IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (sowMinSkill > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowingSkillToSow".Translate(), sowMinSkill.ToString(), "Stat_Thing_Plant_MinGrowingSkillToSow_Desc".Translate(), 4151);
			}
			string attributes = "";
			if (Harvestable)
			{
				string text = "Harvestable".Translate();
				if (!attributes.NullOrEmpty())
				{
					attributes += ", ";
					text = text.UncapitalizeFirst();
				}
				attributes += text;
			}
			if (LimitedLifespan)
			{
				string text2 = "LimitedLifespan".Translate();
				if (!attributes.NullOrEmpty())
				{
					attributes += ", ";
					text2 = text2.UncapitalizeFirst();
				}
				attributes += text2;
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "GrowingTime".Translate(), growDays.ToString("0.##") + " " + "Days".Translate(), "GrowingTimeDesc".Translate(), 4158);
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "FertilityRequirement".Translate(), fertilityMin.ToStringPercent(), "Stat_Thing_Plant_FertilityRequirement_Desc".Translate(), 4156);
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "FertilitySensitivity".Translate(), fertilitySensitivity.ToStringPercent(), "Stat_Thing_Plant_FertilitySensitivity_Desc".Translate(), 4155);
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "LightRequirement".Translate(), growMinGlow.ToStringPercent(), "Stat_Thing_Plant_LightRequirement_Desc".Translate(), 4154);
			if (!attributes.NullOrEmpty())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Attributes".Translate(), attributes, "Stat_Thing_Plant_Attributes_Desc".Translate(), 4157);
			}
			if (LimitedLifespan)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "LifeSpan".Translate(), LifespanDays.ToString("0.##") + " " + "Days".Translate(), "Stat_Thing_Plant_LifeSpan_Desc".Translate(), 4150);
			}
			if (harvestYield > 0f)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Stat_Thing_Plant_HarvestYield_Desc".Translate());
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficulty.label) + ": " + Find.Storyteller.difficultyValues.cropYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "HarvestYield".Translate(), Mathf.CeilToInt(harvestYield * Find.Storyteller.difficultyValues.cropYieldFactor).ToString("F0"), stringBuilder.ToString(), 4150, null, GetHarvestYieldHyperlinks());
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowthTemperature".Translate(), 0f.ToStringTemperature(), "Stat_Thing_Plant_MinGrowthTemperature_Desc".Translate(), 4152);
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MaxGrowthTemperature".Translate(), 58f.ToStringTemperature(), "Stat_Thing_Plant_MaxGrowthTemperature_Desc".Translate(), 4153);
		}
	}
}
