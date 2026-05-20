using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class PocketMapProperties
{
	public BiomeDef biome;

	public List<TileMutatorDef> tileMutators = new List<TileMutatorDef>();

	public float temperature;

	public bool destroyOnParentMapAbandoned = true;

	public bool preventPrisonerEscape = true;

	public bool canLaunchGravship;

	public bool canBeCleaned;
}
