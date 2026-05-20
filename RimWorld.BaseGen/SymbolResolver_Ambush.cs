using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Ambush : SymbolResolver
{
	private float DefaultAmbushPoints = 200f;

	public override bool CanResolve(ResolveParams rp)
	{
		return !rp.ambushSignalTag.NullOrEmpty();
	}

	public override void Resolve(ResolveParams rp)
	{
		SignalAction_Ambush obj = (SignalAction_Ambush)ThingMaker.MakeThing(ThingDefOf.SignalAction_Ambush);
		obj.signalTag = rp.ambushSignalTag;
		obj.points = rp.ambushPoints ?? DefaultAmbushPoints;
		obj.ambushType = rp.ambushType.GetValueOrDefault();
		obj.spawnNear = rp.spawnNear ?? IntVec3.Invalid;
		obj.spawnAround = rp.spawnAround.GetValueOrDefault();
		obj.spawnPawnsOnEdge = rp.spawnPawnsOnEdge == true;
		GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
	}
}
