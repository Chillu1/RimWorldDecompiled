using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompTargetable_AllPawnsOnTheMap : CompTargetable
{
	protected override bool PlayerChoosesTarget => false;

	protected override TargetingParameters GetTargetingParameters()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = false
		};
	}

	public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
	{
		if (parent.MapHeld == null)
		{
			yield break;
		}
		TargetingParameters tp = GetTargetingParameters();
		foreach (Pawn item in parent.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (tp.CanTarget(item) && ValidateTarget(item))
			{
				yield return item;
			}
		}
	}
}
