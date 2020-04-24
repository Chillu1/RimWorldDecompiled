using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FactionManager : IExposable
	{
		private List<Faction> allFactions = new List<Faction>();

		private Faction ofPlayer;

		private Faction ofMechanoids;

		private Faction ofInsects;

		private Faction ofAncients;

		private Faction ofAncientsHostile;

		private Faction empire;

		public List<Faction> AllFactionsListForReading => allFactions;

		public IEnumerable<Faction> AllFactions => allFactions;

		public IEnumerable<Faction> AllFactionsVisible => allFactions.Where((Faction fa) => !fa.def.hidden);

		public IEnumerable<Faction> AllFactionsVisibleInViewOrder => GetInViewOrder(AllFactionsVisible);

		public IEnumerable<Faction> AllFactionsInViewOrder => GetInViewOrder(AllFactions);

		public Faction OfPlayer => ofPlayer;

		public Faction OfMechanoids => ofMechanoids;

		public Faction OfInsects => ofInsects;

		public Faction OfAncients => ofAncients;

		public Faction OfAncientsHostile => ofAncientsHostile;

		public Faction Empire => empire;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allFactions, "allFactions", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.FactionManagerPostLoadInit();
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (allFactions.RemoveAll((Faction x) => x == null || x.def == null) != 0)
				{
					Log.Error("Some factions were null after loading.");
				}
				RecacheFactions();
			}
		}

		public void Add(Faction faction)
		{
			if (!allFactions.Contains(faction))
			{
				allFactions.Add(faction);
				RecacheFactions();
			}
		}

		public void Remove(Faction faction)
		{
			if (allFactions.Contains(faction))
			{
				allFactions.Remove(faction);
				RecacheFactions();
			}
		}

		public void FactionManagerTick()
		{
			SettlementProximityGoodwillUtility.CheckSettlementProximityGoodwillChange();
			for (int i = 0; i < allFactions.Count; i++)
			{
				allFactions[i].FactionTick();
			}
		}

		public Faction FirstFactionOfDef(FactionDef facDef)
		{
			for (int i = 0; i < allFactions.Count; i++)
			{
				if (allFactions[i].def == facDef)
				{
					return allFactions[i];
				}
			}
			return null;
		}

		public bool TryGetRandomNonColonyHumanlikeFaction(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false, TechLevel minTechLevel = TechLevel.Undefined)
		{
			return AllFactions.Where((Faction x) => !x.IsPlayer && !x.def.hidden && x.def.humanlikeFaction && (allowDefeated || !x.defeated) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel)).TryRandomElementByWeight((Faction x) => (tryMedievalOrBetter && (int)x.def.techLevel < 3) ? 0.1f : 1f, out faction);
		}

		public IEnumerable<Faction> GetFactions(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			for (int i = 0; i < allFactions.Count; i++)
			{
				Faction faction = allFactions[i];
				if (!faction.IsPlayer && (allowHidden || !faction.def.hidden) && (allowDefeated || !faction.defeated) && (allowNonHumanlike || faction.def.humanlikeFaction) && (minTechLevel == TechLevel.Undefined || (int)faction.def.techLevel >= (int)minTechLevel))
				{
					yield return faction;
				}
			}
		}

		public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.HostileTo(Faction.OfPlayer)
				select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomNonHostileFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where !x.HostileTo(Faction.OfPlayer)
				select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomAlliedFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.PlayerRelationKind == FactionRelationKind.Ally
				select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomRoyalFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.def.HasRoyalTitles
				select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public void LogKidnappedPawns()
		{
			Log.Message("Kidnapped pawns:");
			for (int i = 0; i < allFactions.Count; i++)
			{
				allFactions[i].kidnapped.LogKidnappedPawns();
			}
		}

		public static IEnumerable<Faction> GetInViewOrder(IEnumerable<Faction> factions)
		{
			return from x in factions
				orderby x.defeated, x.def.listOrderPriority descending
				select x;
		}

		private void RecacheFactions()
		{
			ofPlayer = null;
			for (int i = 0; i < allFactions.Count; i++)
			{
				if (allFactions[i].IsPlayer)
				{
					ofPlayer = allFactions[i];
					break;
				}
			}
			ofMechanoids = FirstFactionOfDef(FactionDefOf.Mechanoid);
			ofInsects = FirstFactionOfDef(FactionDefOf.Insect);
			ofAncients = FirstFactionOfDef(FactionDefOf.Ancients);
			ofAncientsHostile = FirstFactionOfDef(FactionDefOf.AncientsHostile);
			empire = FirstFactionOfDef(FactionDefOf.Empire);
		}
	}
}
