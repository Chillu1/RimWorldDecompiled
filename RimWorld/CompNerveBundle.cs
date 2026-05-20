using Verse;

namespace RimWorld;

public class CompNerveBundle : CompFleshmassHeartChild
{
	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		Thing thing = ThingMaker.MakeThing(ThingDefOf.FleshmassNeuralLump);
		thing.TryGetComp<CompAnalyzableBiosignature>().biosignature = heart.Biosignature;
		GenSpawn.Spawn(thing, parent.Position, prevMap);
		Messages.Message("NeuralLumpDroppedMessage".Translate(), thing, MessageTypeDefOf.NeutralEvent);
		base.Notify_Killed(prevMap, dinfo);
	}
}
