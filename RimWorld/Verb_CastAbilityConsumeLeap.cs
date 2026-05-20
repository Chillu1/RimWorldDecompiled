using Verse;

namespace RimWorld;

public class Verb_CastAbilityConsumeLeap : Verb_CastAbilityJump
{
	public override ThingDef JumpFlyerDef => ThingDefOf.PawnFlyer_ConsumeLeap;
}
