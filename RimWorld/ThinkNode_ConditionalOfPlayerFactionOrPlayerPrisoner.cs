using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalOfPlayerFactionOrPlayerPrisoner : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer)
			{
				if (pawn.HostFaction == Faction.OfPlayer)
				{
					return pawn.guest.IsPrisoner;
				}
				return false;
			}
			return true;
		}
	}
}
