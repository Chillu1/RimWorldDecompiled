using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_ImmobileCaravan : Alert_Critical
{
	private List<Caravan> immobileCaravansResult = new List<Caravan>();

	private List<Caravan> ImmobileCaravans
	{
		get
		{
			immobileCaravansResult.Clear();
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (caravan.Shuttle == null && caravan.IsPlayerControlled && caravan.ImmobilizedByMass)
				{
					immobileCaravansResult.Add(caravan);
				}
			}
			return immobileCaravansResult;
		}
	}

	public Alert_ImmobileCaravan()
	{
		defaultLabel = "ImmobileCaravan".Translate();
		defaultExplanation = "ImmobileCaravanDesc".Translate();
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(ImmobileCaravans);
	}
}
