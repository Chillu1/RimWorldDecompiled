using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Infestation : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (Faction.OfInsects != null)
		{
			return !rp.infestationSignalTag.NullOrEmpty();
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		SignalAction_Infestation obj = (SignalAction_Infestation)ThingMaker.MakeThing(ThingDefOf.SignalAction_Infestation);
		obj.signalTag = rp.infestationSignalTag;
		obj.hivesCount = rp.hivesCount ?? 1;
		obj.insectsPoints = rp.insectsPoints;
		obj.spawnAnywhereIfNoGoodCell = rp.spawnAnywhereIfNoGoodCell == true;
		obj.ignoreRoofedRequirement = rp.ignoreRoofedRequirement == true;
		obj.overrideLoc = rp.overrideLoc;
		obj.sendStandardLetter = rp.sendStandardLetter ?? true;
		GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
	}
}
