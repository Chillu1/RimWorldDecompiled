using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_TriggerUnfogged : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			TriggerUnfogged obj = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged);
			obj.signalTag = rp.unfoggedSignalTag;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
