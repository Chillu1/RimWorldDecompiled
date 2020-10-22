using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Techprints : ThingSetMaker
	{
		private float marketValueFactor = 1f;

		private static readonly SimpleCurve ResearchableProjectsCountToSelectionWeightCurve = new SimpleCurve
		{
			new CurvePoint(4f, 1f),
			new CurvePoint(0f, 5f)
		};

		private static List<ThingDef> tmpGenerated = new List<ThingDef>();

		public override float ExtraSelectionWeightFactor(ThingSetMakerParams parms)
		{
			int num = 0;
			bool flag = false;
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (!allDef.IsFinished && allDef.PrerequisitesCompleted)
				{
					if (!allDef.TechprintRequirementMet && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(allDef.Techprint, allDef.TechprintCount - allDef.TechprintsApplied))
					{
						flag = true;
					}
					else
					{
						num++;
					}
				}
			}
			if (!flag)
			{
				return 1f;
			}
			return Mathf.RoundToInt(ResearchableProjectsCountToSelectionWeightCurve.Evaluate(num));
		}

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (parms.countRange.HasValue && parms.countRange.Value.max <= 0)
			{
				return false;
			}
			ThingDef result;
			return TechprintUtility.TryGetTechprintDefToGenerate(parms.makingFaction, out result, null, (!parms.totalMarketValueRange.HasValue) ? float.MaxValue : (parms.totalMarketValueRange.Value.max * marketValueFactor));
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			tmpGenerated.Clear();
			ThingDef result3;
			if (parms.countRange.HasValue)
			{
				int num = Mathf.Max(parms.countRange.Value.RandomInRange, 1);
				for (int i = 0; i < num; i++)
				{
					if (!TechprintUtility.TryGetTechprintDefToGenerate(parms.makingFaction, out var result, tmpGenerated))
					{
						break;
					}
					tmpGenerated.Add(result);
					outThings.Add(ThingMaker.MakeThing(result));
				}
			}
			else if (parms.totalMarketValueRange.HasValue)
			{
				float num2 = parms.totalMarketValueRange.Value.RandomInRange * marketValueFactor;
				ThingDef result2;
				for (float num3 = 0f; TechprintUtility.TryGetTechprintDefToGenerate(parms.makingFaction, out result2, tmpGenerated, num2 - num3) || (!tmpGenerated.Any() && TechprintUtility.TryGetTechprintDefToGenerate(parms.makingFaction, out result2, tmpGenerated)); num3 += result2.BaseMarketValue)
				{
					tmpGenerated.Add(result2);
					outThings.Add(ThingMaker.MakeThing(result2));
				}
			}
			else if (TechprintUtility.TryGetTechprintDefToGenerate(parms.makingFaction, out result3, tmpGenerated))
			{
				tmpGenerated.Add(result3);
				outThings.Add(ThingMaker.MakeThing(result3));
			}
			tmpGenerated.Clear();
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.HasComp(typeof(CompTechprint)));
		}
	}
}
