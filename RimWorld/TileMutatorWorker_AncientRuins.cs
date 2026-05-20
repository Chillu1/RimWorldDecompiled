using System;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientRuins : TileMutatorWorker
{
	private static readonly FloatRange MapFillPercentRange = new FloatRange(0.3f, 0.5f);

	protected static GenStep_AncientRuins GenStep => (GenStep_AncientRuins)GenStepDefOf.AncientRuins_Special.genStep;

	public TileMutatorWorker_AncientRuins(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GenerateNonCriticalStructures(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		try
		{
			GenStep.GenerateRuins(map, default(GenStepParams), MapFillPercentRange);
		}
		catch (Exception ex)
		{
			Log.Error("Exception while generating ancient ruins: " + ex);
		}
	}
}
