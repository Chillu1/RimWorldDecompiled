using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse;

public class MentalBreakDef : Def
{
	public Type workerClass = typeof(MentalBreakWorker);

	public MentalStateDef mentalState;

	public float baseCommonality;

	public SimpleCurve commonalityFactorPerPopulationCurve;

	public MentalBreakIntensity intensity;

	public TraitDef requiredTrait;

	public GeneDef requiredGene;

	public bool anomalousBreak;

	public bool questLodgersCanDo = true;

	public PreceptDef requiredPrecept;

	public List<PlanetLayerDef> layerWhitelist = new List<PlanetLayerDef>();

	private MentalBreakWorker workerInt;

	public MentalBreakWorker Worker
	{
		get
		{
			if (workerInt == null && workerClass != null)
			{
				workerInt = (MentalBreakWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (intensity == MentalBreakIntensity.None)
		{
			yield return "intensity not set";
		}
	}
}
