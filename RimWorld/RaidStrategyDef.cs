using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RaidStrategyDef : Def
{
	public Type workerClass;

	public SimpleCurve selectionWeightPerPointsCurve;

	public float minPawns = 1f;

	public List<FactionCurve> selectionWeightCurvesPerFaction;

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	[MustTranslate]
	public string arrivalTextFriendly;

	[MustTranslate]
	public string arrivalTextEnemy;

	[MustTranslate]
	public string letterLabelEnemy;

	[MustTranslate]
	public string letterLabelFriendly;

	public SimpleCurve pointsFactorCurve;

	public bool pawnsCanBringFood;

	public List<PawnsArrivalModeDef> arriveModes;

	public float raidLootValueFactor = 1f;

	private RaidStrategyWorker workerInt;

	public RaidStrategyWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (RaidStrategyWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}
}
