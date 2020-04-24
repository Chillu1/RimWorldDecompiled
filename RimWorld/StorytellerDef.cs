using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StorytellerDef : Def
	{
		public int listOrder = 9999;

		public bool listVisible = true;

		public bool tutorialMode;

		public bool disableAdaptiveTraining;

		public bool disableAlerts;

		public bool disablePermadeath;

		public DifficultyDef forcedDifficulty;

		[NoTranslate]
		private string portraitLarge;

		[NoTranslate]
		private string portraitTiny;

		public List<StorytellerCompProperties> comps = new List<StorytellerCompProperties>();

		public SimpleCurve populationIntentFactorFromPopCurve;

		public SimpleCurve populationIntentFactorFromPopAdaptDaysCurve;

		public SimpleCurve pointsFactorFromDaysPassed;

		public float adaptDaysMin;

		public float adaptDaysMax = 100f;

		public float adaptDaysGameStartGraceDays;

		public SimpleCurve pointsFactorFromAdaptDays;

		public SimpleCurve adaptDaysLossFromColonistLostByPostPopulation;

		public SimpleCurve adaptDaysLossFromColonistViolentlyDownedByPopulation;

		public SimpleCurve adaptDaysGrowthRateCurve;

		[Unsaved(false)]
		public Texture2D portraitLargeTex;

		[Unsaved(false)]
		public Texture2D portraitTinyTex;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!portraitTiny.NullOrEmpty())
				{
					portraitTinyTex = ContentFinder<Texture2D>.Get(portraitTiny);
					portraitLargeTex = ContentFinder<Texture2D>.Get(portraitLarge);
				}
			});
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].ResolveReferences(this);
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (pointsFactorFromAdaptDays == null)
			{
				yield return "pointsFactorFromAdaptDays is null";
			}
			if (adaptDaysLossFromColonistLostByPostPopulation == null)
			{
				yield return "adaptDaysLossFromColonistLostByPostPopulation is null";
			}
			if (adaptDaysLossFromColonistViolentlyDownedByPopulation == null)
			{
				yield return "adaptDaysLossFromColonistViolentlyDownedByPopulation is null";
			}
			if (adaptDaysGrowthRateCurve == null)
			{
				yield return "adaptDaysGrowthRateCurve is null";
			}
			if (pointsFactorFromDaysPassed == null)
			{
				yield return "pointsFactorFromDaysPassed is null";
			}
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (string item2 in comps[i].ConfigErrors(this))
				{
					yield return item2;
				}
			}
		}
	}
}
