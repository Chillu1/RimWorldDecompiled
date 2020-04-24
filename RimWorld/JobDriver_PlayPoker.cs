using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlayPoker : JobDriver_SitFacingBuilding
	{
		protected override void ModifyPlayToil(Toil toil)
		{
			base.ModifyPlayToil(toil);
			toil.WithEffect(() => EffecterDefOf.PlayPoker, () => base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
		}
	}
}
