using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalOfPlayerFaction : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Faction == Faction.OfPlayer;
		}
	}
}
