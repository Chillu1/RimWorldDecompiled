using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalNotFormingCaravan : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.IsFormingCaravan();
		}
	}
}
