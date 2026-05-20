using System.Collections.Generic;

namespace Verse.AI;

public class JobDriver_CastVerbOnce : JobDriver
{
	public override string GetReport()
	{
		string text = ((!base.TargetA.HasThing) ? ((string)"AreaLower".Translate()) : base.TargetThingA.LabelCap);
		if (job.verbToUse == null)
		{
			return null;
		}
		return "UsingVerb".Translate(job.verbToUse.ReportLabel, text);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.B);
		yield return Toils_Combat.CastVerb(TargetIndex.A);
	}
}
