using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalNonPlayerNonHostileFaction : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
			{
				return !pawn.Faction.HostileTo(Faction.OfPlayer);
			}
			return false;
		}
	}
}
