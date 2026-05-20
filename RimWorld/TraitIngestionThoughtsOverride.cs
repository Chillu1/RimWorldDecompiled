using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitIngestionThoughtsOverride
	{
		public ThingDef thing;

		public MeatSourceCategory meatSource;

		public List<ThoughtDef> thoughts;

		public List<ThoughtDef> thoughtsDirect;

		public List<ThoughtDef> thoughtsAsIngredient;
	}
}
