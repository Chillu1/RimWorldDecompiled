using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ResurrectMech : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex CastCellInd = TargetIndex.B;

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private IntVec3 Destination => base.TargetB.Cell;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(Corpse, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(Destination, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B);
			yield return Toils_General.Do(delegate
			{
				Ability ability = pawn.abilities.GetAbility(AbilityDefOf.ResurrectionMech);
				pawn.jobs.TryTakeOrderedJob(ability.GetJob(Corpse, Destination), JobTag.Misc);
			});
		}
	}
}
