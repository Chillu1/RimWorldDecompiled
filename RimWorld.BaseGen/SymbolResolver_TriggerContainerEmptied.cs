using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_TriggerContainerEmptied : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			return rp.triggerContainerEmptiedThing != null;
		}

		public override void Resolve(ResolveParams rp)
		{
			TriggerContainerEmptied obj = (TriggerContainerEmptied)ThingMaker.MakeThing(ThingDefOf.TriggerContainerEmptied);
			obj.signalTag = rp.triggerContainerEmptiedSignalTag;
			obj.container = rp.triggerContainerEmptiedThing;
			GenSpawn.Spawn(obj, rp.triggerContainerEmptiedThing.Position, BaseGen.globalSettings.map);
		}
	}
}
