using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ActivityDormant : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return ModLister.AnomalyInstalled;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_LayDown.ActivityDormant();
	}
}
