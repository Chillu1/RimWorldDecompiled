using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalNonPlayerNonHostileFactionOrFactionless : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Faction != null)
			{
				if (pawn.Faction != Faction.OfPlayer)
				{
					return !pawn.Faction.HostileTo(Faction.OfPlayer);
				}
				return false;
			}
			return true;
		}
	}
}
