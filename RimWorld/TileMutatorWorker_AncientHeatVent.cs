using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientHeatVent : TileMutatorWorker_AncientVent
{
	protected override ThingDef AncientVentDef => ThingDefOf.AncientHeatVent;

	public TileMutatorWorker_AncientHeatVent(TileMutatorDef def)
		: base(def)
	{
	}
}
