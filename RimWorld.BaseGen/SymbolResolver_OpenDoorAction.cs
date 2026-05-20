using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_OpenDoorAction : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			SignalAction_OpenDoor obj = (SignalAction_OpenDoor)ThingMaker.MakeThing(ThingDefOf.SignalAction_OpenDoor);
			obj.signalTag = rp.openDoorActionSignalTag;
			obj.door = rp.openDoorActionDoor;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
