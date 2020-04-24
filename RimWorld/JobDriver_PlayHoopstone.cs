using Verse;

namespace RimWorld
{
	public class JobDriver_PlayHoopstone : JobDriver_WatchBuilding
	{
		private const int StoneThrowInterval = 400;

		protected override void WatchTickAction()
		{
			if (pawn.IsHashIntervalTick(400))
			{
				MoteMaker.ThrowStone(pawn, base.TargetA.Cell);
			}
			base.WatchTickAction();
		}
	}
}
