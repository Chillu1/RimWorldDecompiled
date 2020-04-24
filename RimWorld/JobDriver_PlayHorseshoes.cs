using Verse;

namespace RimWorld
{
	public class JobDriver_PlayHorseshoes : JobDriver_WatchBuilding
	{
		private const int HorseshoeThrowInterval = 400;

		protected override void WatchTickAction()
		{
			if (pawn.IsHashIntervalTick(400))
			{
				MoteMaker.ThrowHorseshoe(pawn, base.TargetA.Cell);
			}
			base.WatchTickAction();
		}
	}
}
