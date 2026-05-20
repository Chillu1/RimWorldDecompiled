using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Message : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			SignalAction_Message obj = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
			obj.signalTag = rp.messageSignalTag;
			obj.message = rp.message;
			obj.messageType = rp.messageType;
			obj.lookTargets = rp.lookTargets;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
