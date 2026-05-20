namespace Verse;

public class HediffComp_GiveNeurocharge : HediffComp
{
	public override void CompPostTick(ref float severityAdjustment)
	{
		parent.pawn.health.lastReceivedNeuralSuperchargeTick = Find.TickManager.TicksGame;
	}
}
