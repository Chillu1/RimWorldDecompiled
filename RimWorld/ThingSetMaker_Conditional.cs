using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class ThingSetMaker_Conditional : ThingSetMaker
	{
		public ThingSetMaker thingSetMaker;

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (Condition(parms))
			{
				return thingSetMaker.CanGenerate(parms);
			}
			return false;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			outThings.AddRange(thingSetMaker.Generate(parms));
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return thingSetMaker.AllGeneratableThingsDebug(parms);
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			thingSetMaker.ResolveReferences();
		}

		protected abstract bool Condition(ThingSetMakerParams parms);
	}
}
