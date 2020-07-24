using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentDef : Def
	{
		public Type workerClass;

		public IncidentCategoryDef category;

		public List<IncidentTargetTagDef> targetTags;

		public float baseChance;

		public float baseChanceWithRoyalty = -1f;

		public IncidentPopulationEffect populationEffect;

		public int earliestDay;

		public int minPopulation;

		public bool requireColonistsPresent;

		public float minRefireDays;

		public int minDifficulty;

		public bool pointsScaleable;

		public float minThreatPoints = float.MinValue;

		public List<BiomeDef> allowedBiomes;

		[NoTranslate]
		public List<string> tags;

		[NoTranslate]
		public List<string> refireCheckTags;

		public SimpleCurve chanceFactorByPopulationCurve;

		public TaleDef tale;

		public int minGreatestPopulation = -1;

		[MustTranslate]
		public string letterText;

		[MustTranslate]
		public string letterLabel;

		public LetterDef letterDef;

		public List<HediffDef> letterHyperlinkHediffDefs;

		public PawnKindDef pawnKind;

		public bool pawnMustBeCapableOfViolence;

		public Gender pawnFixedGender;

		public GameConditionDef gameCondition;

		public FloatRange durationDays;

		public HediffDef diseaseIncident;

		public FloatRange diseaseVictimFractionRange = new FloatRange(0f, 0.49f);

		public int diseaseMaxVictims = 99999;

		public List<BiomeDiseaseRecord> diseaseBiomeRecords;

		public List<BodyPartDef> diseasePartsToAffect;

		public ThingDef mechClusterBuilding;

		public List<MTBByBiome> mtbDaysByBiome;

		public QuestScriptDef questScriptDef;

		[Unsaved(false)]
		private IncidentWorker workerInt;

		[Unsaved(false)]
		private List<IncidentDef> cachedRefireCheckIncidents;

		public bool NeedsParmsPoints => category.needsParmsPoints;

		public IncidentWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (IncidentWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public List<IncidentDef> RefireCheckIncidents
		{
			get
			{
				if (refireCheckTags == null)
				{
					return null;
				}
				if (cachedRefireCheckIncidents == null)
				{
					cachedRefireCheckIncidents = new List<IncidentDef>();
					List<IncidentDef> allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (ShouldDoRefireCheckWith(allDefsListForReading[i]))
						{
							cachedRefireCheckIncidents.Add(allDefsListForReading[i]);
						}
					}
				}
				return cachedRefireCheckIncidents;
			}
		}

		public static IncidentDef Named(string defName)
		{
			return DefDatabase<IncidentDef>.GetNamed(defName);
		}

		private bool ShouldDoRefireCheckWith(IncidentDef other)
		{
			if (other.tags == null)
			{
				return false;
			}
			if (other == this)
			{
				return false;
			}
			for (int i = 0; i < other.tags.Count; i++)
			{
				for (int j = 0; j < refireCheckTags.Count; j++)
				{
					if (other.tags[i] == refireCheckTags[j])
					{
						return true;
					}
				}
			}
			return false;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (category == null)
			{
				yield return "category is undefined.";
			}
			if (targetTags == null || targetTags.Count == 0)
			{
				yield return "no target type";
			}
			if (TargetTagAllowed(IncidentTargetTagDefOf.World) && targetTags.Any((IncidentTargetTagDef tt) => tt != IncidentTargetTagDefOf.World))
			{
				yield return "allows world target type along with other targets. World targeting incidents should only target the world.";
			}
			if (TargetTagAllowed(IncidentTargetTagDefOf.World) && allowedBiomes != null)
			{
				yield return "world-targeting incident has a biome restriction list";
			}
		}

		public bool TargetTagAllowed(IncidentTargetTagDef target)
		{
			return targetTags.Contains(target);
		}

		public bool TargetAllowed(IIncidentTarget target)
		{
			return targetTags.Intersect(target.IncidentTargetTags()).Any();
		}
	}
}
