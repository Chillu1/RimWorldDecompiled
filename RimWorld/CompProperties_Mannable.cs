using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Mannable : CompProperties
{
	public WorkTags manWorkType;

	public List<PlanetLayerDef> planetLayerWhitelist;

	public CompProperties_Mannable()
	{
		compClass = typeof(CompMannable);
	}
}
