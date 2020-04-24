using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_ImmobileCaravan : Alert_Critical
	{
		private List<Caravan> immobileCaravansResult = new List<Caravan>();

		private List<Caravan> ImmobileCaravans
		{
			get
			{
				immobileCaravansResult.Clear();
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int i = 0; i < caravans.Count; i++)
				{
					if (caravans[i].IsPlayerControlled && caravans[i].ImmobilizedByMass)
					{
						immobileCaravansResult.Add(caravans[i]);
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
}
