using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class Pawn_InfectionVectorTracker : IExposable
	{
		private readonly Pawn pawn;

		private Dictionary<InfectionPathwayDef, InfectionPathway> pathways = new Dictionary<InfectionPathwayDef, InfectionPathway>(20);

		private bool givenPrearrival;

		private const int UpdateRateTicks = 600;

		private const int MaximumPathways = 100;

		private static readonly List<InfectionPathway> TmpPathways = new List<InfectionPathway>();

		public IEnumerable<InfectionPathway> Pathways => pathways.Values;

		public int PathwaysCount => pathways.Count;

		public Pawn_InfectionVectorTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void NotifySpawned(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad && !givenPrearrival)
			{
				givenPrearrival = true;
				if (!Find.GameInfo.startingAndOptionalPawns.Contains(pawn) && !pawn.DevelopmentalStage.Baby() && !pawn.dontGivePreArrivalPathway)
				{
					AddInfectionVector(InfectionPathwayDefOf.PrearrivalGeneric);
				}
			}
		}

		public bool TryGetPathway(InfectionPathwayDef def, out InfectionPathway pathway)
		{
			return pathways.TryGetValue(def, out pathway);
		}

		public List<InfectionPathway> GetPathwaysForHediff(HediffDef hediff)
		{
			TmpPathways.Clear();
			foreach (HediffInfectionPathway possiblePathway in hediff.possiblePathways)
			{
				if (pathways.TryGetValue(possiblePathway.PathwayDef, out var value))
				{
					TmpPathways.Add(value);
				}
			}
			return TmpPathways;
		}

		public bool AnyPathwayForHediff(HediffDef hediff)
		{
			TmpPathways.Clear();
			foreach (HediffInfectionPathway possiblePathway in hediff.possiblePathways)
			{
				if (pathways.ContainsKey(possiblePathway.PathwayDef))
				{
					return true;
				}
			}
			return false;
		}

		public void AddInfectionVector(InfectionPathwayDef def, Pawn source = null)
		{
			AddInfectionVector(new InfectionPathway(def, pawn, source?.kindDef));
		}

		public void AddInfectionVector(InfectionPathway pathway)
		{
			if (pathways.ContainsKey(pathway.Def))
			{
				pathways[pathway.Def] = pathway;
				return;
			}
			if (pathways.Count >= 100)
			{
				ReducePathwaysForNewPathway();
			}
			pathways.Add(pathway.Def, pathway);
		}

		public void InfectionTickInterval(int delta)
		{
			foreach (KeyValuePair<InfectionPathwayDef, InfectionPathway> pathway in pathways)
			{
				pathway.Deconstruct(out var _, out var value);
				value.TickInterval(delta);
			}
			if (pawn.IsHashIntervalTick(600, delta))
			{
				RemoveExpiredPathways();
			}
		}

		private void RemoveExpiredPathways()
		{
			TmpPathways.Clear();
			foreach (var (_, infectionPathway2) in pathways)
			{
				if (infectionPathway2.Expired)
				{
					TmpPathways.Add(infectionPathway2);
				}
			}
			foreach (InfectionPathway tmpPathway in TmpPathways)
			{
				pathways.Remove(tmpPathway.Def);
			}
			TmpPathways.Clear();
		}

		private void ReducePathwaysForNewPathway()
		{
			if (pathways.Count <= 99)
			{
				return;
			}
			int num = pathways.Count - 100 + 1;
			TmpPathways.Clear();
			foreach (var (_, item) in pathways)
			{
				TmpPathways.Add(item);
			}
			TmpPathways.SortBy((InfectionPathway pathway) => pathway.Def.ExpiryTicks - pathway.AgeTicks);
			for (int num2 = 0; num2 < num; num2++)
			{
				pathways.Remove(TmpPathways[num2].Def);
			}
			TmpPathways.Clear();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref givenPrearrival, "givenPrearrival", defaultValue: false);
			Scribe_Collections.Look(ref pathways, "pathways", LookMode.Def, LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && pathways == null)
			{
				pathways = new Dictionary<InfectionPathwayDef, InfectionPathway>(20);
			}
		}
	}
}
