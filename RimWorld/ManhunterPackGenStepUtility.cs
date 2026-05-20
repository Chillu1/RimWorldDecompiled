using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class ManhunterPackGenStepUtility
{
	public static bool TryGetAnimalsKind(float points, PlanetTile tile, out PawnKindDef animalKind)
	{
		if (!AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(points, tile, out animalKind))
		{
			return AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(points, PlanetTile.Invalid, out animalKind);
		}
		return true;
	}
}
