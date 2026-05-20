using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class AssignableUtility
{
	public static Pawn GetAssignedPawn(this Building building)
	{
		CompAssignableToPawn compAssignableToPawn = building.TryGetComp<CompAssignableToPawn>();
		if (compAssignableToPawn == null || !compAssignableToPawn.AssignedPawnsForReading.Any())
		{
			return null;
		}
		return compAssignableToPawn.AssignedPawnsForReading[0];
	}

	public static IEnumerable<Pawn> GetAssignedPawns(this Building building)
	{
		CompAssignableToPawn compAssignableToPawn = building.TryGetComp<CompAssignableToPawn>();
		if (compAssignableToPawn == null || !compAssignableToPawn.AssignedPawnsForReading.Any())
		{
			return null;
		}
		return compAssignableToPawn.AssignedPawns;
	}
}
