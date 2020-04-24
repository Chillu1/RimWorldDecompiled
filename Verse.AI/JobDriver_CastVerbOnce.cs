using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_CastVerbOnce : JobDriver
	{
		public override string GetReport()
		{
			return TranslatorFormattedStringExtensions.Translate(arg2: (!base.TargetA.HasThing) ? ((string)"AreaLower".Translate()) : base.TargetThingA.LabelCap, key: "UsingVerb", arg1: job.verbToUse.ReportLabel);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Combat.GotoCastPosition(TargetIndex.A);
			yield return Toils_Combat.CastVerb(TargetIndex.A);
		}
	}
}
