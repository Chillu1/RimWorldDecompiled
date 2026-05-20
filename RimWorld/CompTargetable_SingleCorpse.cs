using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompTargetable_SingleCorpse : CompTargetable
{
	protected override bool PlayerChoosesTarget => true;

	protected override TargetingParameters GetTargetingParameters()
	{
		return new TargetingParameters
		{
			canTargetPawns = false,
			canTargetBuildings = false,
			canTargetItems = false,
			canTargetCorpses = true,
			mapObjectTargetsMustBeAutoAttackable = false
		};
	}

	public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
	{
		yield return targetChosenByPlayer;
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (target.Thing is Corpse)
		{
			return base.ValidateTarget(target.Thing, showMessages);
		}
		return false;
	}
}
