using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalShotRecently : ThinkNode_Conditional
	{
		public int thresholdTicks = 2500;

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.lastRangedHarmTick > 0 && Find.TickManager.TicksGame < pawn.mindState.lastRangedHarmTick + thresholdTicks)
			{
				return true;
			}
			return false;
		}
	}
}
