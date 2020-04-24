using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Kidnap : JobDriver_TakeAndExitMap
	{
		protected Pawn Takee => (Pawn)base.Item;

		public override string GetReport()
		{
			if (Takee == null || pawn.HostileTo(Takee))
			{
				return base.GetReport();
			}
			return JobUtility.GetResolvedJobReport(JobDefOf.Rescue.reportString, Takee);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => Takee == null || (!Takee.Downed && Takee.Awake()));
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
		}
	}
}
