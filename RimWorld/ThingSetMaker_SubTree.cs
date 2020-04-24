using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_SubTree : ThingSetMaker
	{
		public ThingSetMakerDef def;

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			return def.root.CanGenerate(parms);
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			outThings.AddRange(def.root.Generate(parms));
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return def.root.AllGeneratableThingsDebug(parms);
		}
	}
}
