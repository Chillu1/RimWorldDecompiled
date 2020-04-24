using RimWorld;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_AbilityUsed : BattleLogEntry_Event
	{
		public AbilityDef abilityUsed;

		public BattleLogEntry_AbilityUsed()
		{
		}

		public BattleLogEntry_AbilityUsed(Pawn caster, Thing target, AbilityDef ability, RulePackDef eventDef)
			: base(target, eventDef, caster)
		{
			abilityUsed = ability;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref abilityUsed, "abilityUsed");
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			result.Rules.AddRange(GrammarUtility.RulesForDef("ABILITY", abilityUsed));
			if (subjectPawn == null && subjectThing == null)
			{
				result.Rules.Add(new Rule_String("SUBJECT_definite", "AreaLower".Translate()));
			}
			return result;
		}
	}
}
