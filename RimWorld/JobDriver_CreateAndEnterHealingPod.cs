using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CreateAndEnterHealingPod : JobDriver_CreateAndEnterDryadHolder
	{
		public override Toil EnterToil()
		{
			return Toils_General.Do(delegate
			{
				GenSpawn.Spawn(ThingDefOf.DryadHealingPod, job.targetB.Cell, pawn.Map).TryGetComp<CompDryadHealingPod>().TryAcceptPawn(pawn);
			});
		}
	}
}
