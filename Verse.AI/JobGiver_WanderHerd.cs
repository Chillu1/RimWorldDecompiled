using System.Collections.Generic;

namespace Verse.AI;

public class JobGiver_WanderHerd : JobGiver_Wander
{
	private const int MinDistToHumanlike = 15;

	public JobGiver_WanderHerd()
	{
		wanderRadius = 5f;
		ticksBetweenWandersRange = new IntRange(125, 200);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		return WanderUtility.GetHerdWanderRoot(isHerdValidator: delegate(Thing t)
		{
			if (((Pawn)t).RaceProps != pawn.RaceProps || t == pawn)
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn2 = allPawnsSpawned[i];
				if (pawn2.RaceProps.Humanlike && (pawn2.Position - t.Position).LengthHorizontalSquared < 225)
				{
					return false;
				}
			}
			return true;
		}, pawn: pawn);
	}
}
