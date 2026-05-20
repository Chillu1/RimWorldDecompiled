using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_HarmedRecently : ThinkNode_Conditional
	{
		public int thresholdTicks = 2500;

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.lastHarmTick > 0 && Find.TickManager.TicksGame < pawn.mindState.lastHarmTick + thresholdTicks)
			{
				return true;
			}
			return false;
		}
	}
}
