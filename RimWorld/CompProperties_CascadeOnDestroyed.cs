using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_CascadeOnDestroyed : CompProperties
{
	public IntRange cascadeCountRange = new IntRange(4, 8);

	public List<ThingDef> cascadeThingDefs;

	public CompProperties_CascadeOnDestroyed()
	{
		compClass = typeof(CompCascadeOnDestroyed);
	}
}
