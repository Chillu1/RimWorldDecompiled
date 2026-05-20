using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientToxVent : TileMutatorWorker_AncientVent
{
	protected override ThingDef AncientVentDef => ThingDefOf.AncientToxVent;

	public TileMutatorWorker_AncientToxVent(TileMutatorDef def)
		: base(def)
	{
	}
}
