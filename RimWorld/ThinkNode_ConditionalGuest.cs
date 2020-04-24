using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalGuest : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.HostFaction != null)
			{
				return !pawn.IsPrisoner;
			}
			return false;
		}
	}
}
