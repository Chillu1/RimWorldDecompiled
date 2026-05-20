using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_TriggerRect : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
		obj.signalTag = rp.rectTriggerSignalTag;
		obj.Rect = rp.rect;
		obj.destroyIfUnfogged = rp.destroyIfUnfogged == true;
		GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
	}
}
