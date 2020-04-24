using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class GameCondition_PsychicSuppression : GameCondition
	{
		public Gender gender;

		public override string LetterText => base.LetterText.Formatted(gender.GetLabel().ToLower());

		public override string Description => base.Description.Formatted(gender.GetLabel().ToLower());

		public override void Init()
		{
			base.Init();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref gender, "gender", Gender.None);
		}

		public static void CheckPawn(Pawn pawn, Gender targetGender)
		{
			if (pawn.RaceProps.Humanlike && pawn.gender == targetGender && !pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicSuppression))
			{
				pawn.health.AddHediff(HediffDefOf.PsychicSuppression);
			}
		}

		public override void GameConditionTick()
		{
			foreach (Map affectedMap in base.AffectedMaps)
			{
				foreach (Pawn allPawn in affectedMap.mapPawns.AllPawns)
				{
					CheckPawn(allPawn, gender);
				}
			}
		}

		public override void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.RandomizeSettings(points, map, outExtraDescriptionRules, outExtraDescriptionConstants);
			if (map.mapPawns.FreeColonistsCount > 0)
			{
				gender = map.mapPawns.FreeColonists.RandomElement().gender;
			}
			else
			{
				gender = Rand.Element(Gender.Male, Gender.Female);
			}
			outExtraDescriptionRules.Add(new Rule_String("psychicSuppressorGender", gender.GetLabel()));
		}
	}
}
