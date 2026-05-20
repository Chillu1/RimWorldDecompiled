using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CreateAndEnterCocoon : JobDriver_CreateAndEnterDryadHolder
	{
		public override Toil EnterToil()
		{
			return Toils_General.Do(delegate
			{
				GenSpawn.Spawn(ThingDefOf.DryadCocoon, job.targetB.Cell, pawn.Map).TryGetComp<CompDryadCocoon>().TryAcceptPawn(pawn);
			});
		}
	}
}
