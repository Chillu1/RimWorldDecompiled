using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeDef : Def
{
	public Type workerClass = typeof(PawnsArrivalModeWorker);

	public SimpleCurve selectionWeightCurve;

	public SimpleCurve pointsFactorCurve;

	public TechLevel minTechLevel;

	public bool forQuickMilitaryAid;

	public bool walkIn;

	public bool canBeBackup = true;

	public float minSpaceSelectionWeight = -1f;

	public List<BiomeDef> biomeWhitelist;

	public List<BiomeDef> biomeBlacklist;

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	[MustTranslate]
	public string textEnemy;

	[MustTranslate]
	public string textFriendly;

	[MustTranslate]
	public string textWillArrive;

	public List<FactionCurve> selectionWeightCurvesPerFaction;

	[Unsaved(false)]
	private PawnsArrivalModeWorker workerInt;

	public PawnsArrivalModeWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (PawnsArrivalModeWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}
}
