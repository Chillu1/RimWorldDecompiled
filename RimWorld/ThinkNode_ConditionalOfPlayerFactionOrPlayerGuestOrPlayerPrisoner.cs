using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalOfPlayerFactionOrPlayerGuestOrPlayerPrisoner : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer)
			{
				return pawn.HostFaction == Faction.OfPlayer;
			}
			return true;
		}
	}
}
