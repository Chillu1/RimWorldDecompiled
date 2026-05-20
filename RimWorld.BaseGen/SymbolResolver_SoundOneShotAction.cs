using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_SoundOneShotAction : SymbolResolver
	{
		private const string SoundSignalPrefix = "SoundTrigger";

		public override void Resolve(ResolveParams rp)
		{
			SignalAction_SoundOneShot obj = (SignalAction_SoundOneShot)ThingMaker.MakeThing(ThingDefOf.SignalAction_SoundOneShot);
			obj.signalTag = rp.soundOneShotActionSignalTag;
			obj.sound = rp.sound;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
