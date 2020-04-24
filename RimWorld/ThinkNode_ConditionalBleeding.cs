using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalBleeding : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.health.hediffSet.BleedRateTotal > 0.001f;
		}
	}
}
