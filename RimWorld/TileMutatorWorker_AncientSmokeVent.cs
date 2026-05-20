using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientSmokeVent : TileMutatorWorker_AncientVent
{
	protected override ThingDef AncientVentDef => ThingDefOf.AncientSmokeVent;

	public TileMutatorWorker_AncientSmokeVent(TileMutatorDef def)
		: base(def)
	{
	}
}
