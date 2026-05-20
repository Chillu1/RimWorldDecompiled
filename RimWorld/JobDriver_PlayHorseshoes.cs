using Verse;

namespace RimWorld;

public class JobDriver_PlayHorseshoes : JobDriver_WatchBuilding
{
	private const int HorseshoeThrowInterval = 400;

	protected override void WatchTickAction(int delta)
	{
		if (pawn.IsHashIntervalTick(400, delta))
		{
			FleckMaker.ThrowHorseshoe(pawn, base.TargetA.Cell);
		}
		base.WatchTickAction(delta);
	}
}
