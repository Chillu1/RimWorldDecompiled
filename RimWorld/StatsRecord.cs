using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatsRecord : IExposable
	{
		public int numRaidsEnemy;

		public int numThreatBigs;

		public int colonistsKilled;

		public int colonistsLaunched;

		public int greatestPopulation;

		public void ExposeData()
		{
			Scribe_Values.Look(ref numRaidsEnemy, "numRaidsEnemy", 0);
			Scribe_Values.Look(ref numThreatBigs, "numThreatsQueued", 0);
			Scribe_Values.Look(ref colonistsKilled, "colonistsKilled", 0);
			Scribe_Values.Look(ref colonistsLaunched, "colonistsLaunched", 0);
			Scribe_Values.Look(ref greatestPopulation, "greatestPopulation", 3);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				UpdateGreatestPopulation();
			}
		}

		public void Notify_ColonistKilled()
		{
			colonistsKilled++;
		}

		public void UpdateGreatestPopulation()
		{
			int a = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count();
			greatestPopulation = Mathf.Max(a, greatestPopulation);
		}
	}
}
