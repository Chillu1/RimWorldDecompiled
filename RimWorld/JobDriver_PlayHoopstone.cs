using Verse;

namespace RimWorld;

public class JobDriver_PlayHoopstone : JobDriver_WatchBuilding
{
	private const int StoneThrowInterval = 400;

	protected override void WatchTickAction(int delta)
	{
		if (pawn.IsHashIntervalTick(400, delta))
		{
			FleckMaker.ThrowStone(pawn, base.TargetA.Cell);
		}
		base.WatchTickAction(delta);
	}
}
