using Verse;

namespace RimWorld;

public class TileMutatorWorker_ObsidianDeposits : TileMutatorWorker
{
	private const float BlotchFactor = 0.1f;

	public TileMutatorWorker_ObsidianDeposits(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostTerrain(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			float num = GenStep_RocksFromGrid.GetResourceBlotchesPer10KCellsForMap(map) * 0.1f;
			GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
			genStep_ScatterLumpsMineable.maxValue = float.MaxValue;
			genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num, num);
			genStep_ScatterLumpsMineable.forcedDefToScatter = ThingDefOf.MineableObsidian;
			genStep_ScatterLumpsMineable.Generate(map, default(GenStepParams));
		}
	}
}
