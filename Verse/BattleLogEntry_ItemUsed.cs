using Verse.Grammar;

namespace Verse;

public class BattleLogEntry_ItemUsed : BattleLogEntry_Event
{
	public ThingDef itemUsed;

	public BattleLogEntry_ItemUsed()
	{
	}

	public BattleLogEntry_ItemUsed(Pawn caster, Thing target, ThingDef itemUsed, RulePackDef eventDef)
		: base(target, eventDef, caster)
	{
		this.itemUsed = itemUsed;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref itemUsed, "itemUsed");
	}

	protected override GrammarRequest GenerateGrammarRequest()
	{
		GrammarRequest result = base.GenerateGrammarRequest();
		result.Rules.AddRange(GrammarUtility.RulesForDef("ITEM", itemUsed));
		return result;
	}
}
