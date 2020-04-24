using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_NeedsTracker : IExposable
	{
		private Pawn pawn;

		private List<Need> needs = new List<Need>();

		public Need_Mood mood;

		public Need_Food food;

		public Need_Rest rest;

		public Need_Joy joy;

		public Need_Beauty beauty;

		public Need_RoomSize roomsize;

		public Need_Outdoors outdoors;

		public Need_Chemical_Any drugsDesire;

		public Need_Comfort comfort;

		public Need_Authority authority;

		public List<Need> AllNeeds => needs;

		public Pawn_NeedsTracker()
		{
		}

		public Pawn_NeedsTracker(Pawn newPawn)
		{
			pawn = newPawn;
			AddOrRemoveNeedsAsAppropriate();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref needs, "needs", LookMode.Deep, pawn);
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (needs.RemoveAll((Need x) => x == null) != 0)
				{
					Log.Error("Pawn " + pawn.ToStringSafe() + " had some null needs after loading.");
				}
				BindDirectNeedFields();
			}
		}

		private void BindDirectNeedFields()
		{
			authority = TryGetNeed<Need_Authority>();
			mood = TryGetNeed<Need_Mood>();
			food = TryGetNeed<Need_Food>();
			rest = TryGetNeed<Need_Rest>();
			joy = TryGetNeed<Need_Joy>();
			beauty = TryGetNeed<Need_Beauty>();
			comfort = TryGetNeed<Need_Comfort>();
			roomsize = TryGetNeed<Need_RoomSize>();
			outdoors = TryGetNeed<Need_Outdoors>();
			drugsDesire = TryGetNeed<Need_Chemical_Any>();
		}

		public void NeedsTrackerTick()
		{
			if (pawn.IsHashIntervalTick(150))
			{
				for (int i = 0; i < needs.Count; i++)
				{
					needs[i].NeedInterval();
				}
			}
		}

		public T TryGetNeed<T>() where T : Need
		{
			for (int i = 0; i < needs.Count; i++)
			{
				if (needs[i].GetType() == typeof(T))
				{
					return (T)needs[i];
				}
			}
			return null;
		}

		public Need TryGetNeed(NeedDef def)
		{
			for (int i = 0; i < needs.Count; i++)
			{
				if (needs[i].def == def)
				{
					return needs[i];
				}
			}
			return null;
		}

		public void SetInitialLevels()
		{
			for (int i = 0; i < needs.Count; i++)
			{
				needs[i].SetInitialLevel();
			}
		}

		public void AddOrRemoveNeedsAsAppropriate()
		{
			List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				NeedDef needDef = allDefsListForReading[i];
				if (ShouldHaveNeed(needDef))
				{
					if (TryGetNeed(needDef) == null)
					{
						AddNeed(needDef);
					}
				}
				else if (TryGetNeed(needDef) != null)
				{
					RemoveNeed(needDef);
				}
			}
		}

		private bool ShouldHaveNeed(NeedDef nd)
		{
			if ((int)pawn.RaceProps.intelligence < (int)nd.minIntelligence)
			{
				return false;
			}
			if (nd.colonistsOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer))
			{
				return false;
			}
			if (nd.colonistAndPrisonersOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || pawn.HostFaction != Faction.OfPlayer))
			{
				return false;
			}
			if (pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.disablesNeed == nd))
			{
				return false;
			}
			if (nd.onlyIfCausedByHediff && !pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.causesNeed == nd))
			{
				return false;
			}
			if (nd.neverOnPrisoner && pawn.IsPrisoner)
			{
				return false;
			}
			if (nd.titleRequiredAny != null)
			{
				if (pawn.royalty == null)
				{
					return false;
				}
				bool flag = false;
				foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
				{
					if (nd.titleRequiredAny.Contains(item.def))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (nd == NeedDefOf.Food)
			{
				return pawn.RaceProps.EatsFood;
			}
			if (nd == NeedDefOf.Rest)
			{
				return pawn.RaceProps.needsRest;
			}
			return true;
		}

		private void AddNeed(NeedDef nd)
		{
			Need need = (Need)Activator.CreateInstance(nd.needClass, pawn);
			need.def = nd;
			needs.Add(need);
			need.SetInitialLevel();
			BindDirectNeedFields();
		}

		private void RemoveNeed(NeedDef nd)
		{
			Need item = TryGetNeed(nd);
			needs.Remove(item);
			BindDirectNeedFields();
		}
	}
}
