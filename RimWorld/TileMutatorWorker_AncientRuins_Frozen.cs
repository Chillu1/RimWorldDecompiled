using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientRuins_Frozen : TileMutatorWorker_AncientRuins
{
	public TileMutatorWorker_AncientRuins_Frozen(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GenerateNonCriticalStructures(Map map)
	{
		base.GenerateNonCriticalStructures(map);
		GenMorphology.GenPatchParms value = GenMorphology.GenPatchParms.For(ThingDefOf.SolidIce, TerrainDefOf.Ice);
		value.edificeThreshold = 0.75f;
		value.roofThreshold = 0.8f;
		GenStep_FrozenRuins.EncaseStructuresInIce(map, TileMutatorWorker_AncientRuins.GenStep.structureSketches, value);
	}
}
