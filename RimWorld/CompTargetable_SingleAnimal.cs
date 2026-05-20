using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompTargetable_SingleAnimal : CompTargetable
{
	protected override bool PlayerChoosesTarget => true;

	public new CompProperties_Targetable_SingleAnimal Props => (CompProperties_Targetable_SingleAnimal)props;

	protected override TargetingParameters GetTargetingParameters()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = false,
			canTargetAnimals = true,
			canTargetHumans = false,
			canTargetMechs = false,
			validator = (TargetInfo target) => Props.allowWildMan || (target.Thing is Pawn p && !p.IsWildMan())
		};
	}

	public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
	{
		yield return targetChosenByPlayer;
	}
}
