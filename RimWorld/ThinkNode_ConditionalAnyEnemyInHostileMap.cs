using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyEnemyInHostileMap : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			Map map = pawn.Map;
			if (!map.IsPlayerHome && map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer))
			{
				return GenHostility.AnyHostileActiveThreatToPlayer(map, countDormantPawnsAsHostile: true);
			}
			return false;
		}
	}
}
