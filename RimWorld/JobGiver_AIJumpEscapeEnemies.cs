using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIJumpEscapeEnemies : ThinkNode_JobGiver
{
	private AbilityDef ability;

	private static List<Thing> tmpHostileSpots = new List<Thing>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		Ability ability = pawn.abilities?.GetAbility(this.ability);
		if (ability == null || !ability.CanCast)
		{
			return null;
		}
		if (TryFindRelocatePosition(pawn, out var relocatePosition, ability.verb.EffectiveRange))
		{
			return ability.GetJob(relocatePosition, relocatePosition);
		}
		return null;
	}

	private bool TryFindRelocatePosition(Pawn pawn, out IntVec3 relocatePosition, float maxDistance)
	{
		tmpHostileSpots.Clear();
		tmpHostileSpots.AddRange(from a in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
			where !a.ThreatDisabled(pawn)
			select a.Thing);
		Ability jump = pawn.abilities?.GetAbility(ability);
		relocatePosition = CellFinderLoose.GetFallbackDest(pawn, tmpHostileSpots, maxDistance, 5f, 5f, 20, (IntVec3 c) => jump.verb.ValidateTarget(c, showMessages: false));
		tmpHostileSpots.Clear();
		return relocatePosition.IsValid;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIJumpEscapeEnemies obj = (JobGiver_AIJumpEscapeEnemies)base.DeepCopy(resolve);
		obj.ability = ability;
		return obj;
	}
}
