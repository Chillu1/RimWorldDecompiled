using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyUndownedColonistSpawnedNearby : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Spawned)
			{
				return pawn.Map.mapPawns.FreeColonistsSpawned.Any((Pawn x) => !x.Downed);
			}
			return false;
		}
	}
}
