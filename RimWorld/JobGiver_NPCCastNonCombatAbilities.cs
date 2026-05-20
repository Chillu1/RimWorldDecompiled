using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_NPCCastNonCombatAbilities : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.abilities == null)
		{
			return null;
		}
		List<Ability> list = pawn.abilities.AICastableAbilities(pawn, offensive: false);
		if (list.NullOrEmpty())
		{
			return null;
		}
		Ability ability = list.RandomElement();
		TargetingParameters targetingParameters = ability.def.verbProperties?.targetParams;
		if (!ability.def.targetRequired || (targetingParameters != null && targetingParameters.canTargetSelf))
		{
			return ability.GetJob(pawn, pawn);
		}
		return null;
	}
}
