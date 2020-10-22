using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class FactionManager : IExposable
	{
		private List<Faction> allFactions = new List<Faction>();

		private List<Faction> toRemove = new List<Faction>();

		private Faction ofPlayer;

		private Faction ofMechanoids;

		private Faction ofInsects;

		private Faction ofAncients;

		private Faction ofAncientsHostile;

		private Faction empire;

		public List<Faction> AllFactionsListForReading => allFactions;

		public IEnumerable<Faction> AllFactions => allFactions;

		public IEnumerable<Faction> AllFactionsVisible => allFactions.Where((Faction fa) => !fa.Hidden);

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
			Scribe_Collections.Look(ref toRemove, "toRemove", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.FactionManagerPostLoadInit();
				if (toRemove == null)
				{
					toRemove = new List<Faction>();
				}
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

		private void Remove(Faction faction)
		{
			if (!faction.temporary)
			{
				Log.Error("Attempting to remove " + faction.Name + " which is not a temporary faction, only temporary factions can be removed");
			}
			else
			{
				if (!allFactions.Contains(faction))
				{
					return;
				}
				List<Pawn> allMapsWorldAndTemporary_AliveOrDead = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead;
				for (int i = 0; i < allMapsWorldAndTemporary_AliveOrDead.Count; i++)
				{
					if (allMapsWorldAndTemporary_AliveOrDead[i].Faction == faction)
					{
						allMapsWorldAndTemporary_AliveOrDead[i].SetFaction(null);
					}
				}
				for (int j = 0; j < Find.Maps.Count; j++)
				{
					Find.Maps[j].pawnDestinationReservationManager.Notify_FactionRemoved(faction);
				}
				Find.LetterStack.Notify_FactionRemoved(faction);
				faction.RemoveAllRelations();
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
			for (int num = toRemove.Count - 1; num >= 0; num--)
			{
				Faction faction = toRemove[num];
				toRemove.Remove(faction);
				Remove(faction);
				Find.QuestManager.Notify_FactionRemoved(faction);
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

		public bool TryGetRandomNonColonyHumanlikeFaction_NewTemp(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false, TechLevel minTechLevel = TechLevel.Undefined, bool allowTemporary = false)
		{
			return AllFactions.Where((Faction x) => !x.IsPlayer && !x.Hidden && x.def.humanlikeFaction && (allowDefeated || !x.defeated) && (allowTemporary || !x.temporary) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel)).TryRandomElementByWeight((Faction x) => (tryMedievalOrBetter && (int)x.def.techLevel < 3) ? 0.1f : 1f, out faction);
		}

		[Obsolete]
		public bool TryGetRandomNonColonyHumanlikeFaction(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false, TechLevel minTechLevel = TechLevel.Undefined)
		{
			return TryGetRandomNonColonyHumanlikeFaction_NewTemp(out faction, tryMedievalOrBetter, allowDefeated, minTechLevel);
		}

		public IEnumerable<Faction> GetFactions_NewTemp(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined, bool allowTemporary = false)
		{
			for (int i = 0; i < allFactions.Count; i++)
			{
				Faction faction = allFactions[i];
				if (!faction.IsPlayer && (allowHidden || !faction.Hidden) && (allowTemporary || !faction.temporary) && (allowDefeated || !faction.defeated) && (allowNonHumanlike || faction.def.humanlikeFaction) && (minTechLevel == TechLevel.Undefined || (int)faction.def.techLevel >= (int)minTechLevel))
				{
					yield return faction;
				}
			}
		}

		[Obsolete]
		public IEnumerable<Faction> GetFactions(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			return GetFactions_NewTemp(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel);
		}

		public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions_NewTemp(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.HostileTo(Faction.OfPlayer)
				select x).TryRandomElement(out var result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomNonHostileFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions_NewTemp(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where !x.HostileTo(Faction.OfPlayer)
				select x).TryRandomElement(out var result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomAlliedFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions_NewTemp(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.PlayerRelationKind == FactionRelationKind.Ally
				select x).TryRandomElement(out var result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomRoyalFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in GetFactions_NewTemp(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
				where x.def.HasRoyalTitles
				select x).TryRandomElement(out var result))
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

		public void Notify_QuestCleanedUp(Quest quest)
		{
			for (int num = allFactions.Count - 1; num >= 0; num--)
			{
				Faction faction = allFactions[num];
				if (FactionCanBeRemoved(faction))
				{
					QueueForRemoval(faction);
				}
			}
		}

		public void Notify_PawnKilled(Pawn pawn)
		{
			TryQueuePawnFactionForRemoval(pawn);
		}

		public void Notify_PawnLeftMap(Pawn pawn)
		{
			TryQueuePawnFactionForRemoval(pawn);
		}

		private void TryQueuePawnFactionForRemoval(Pawn pawn)
		{
			if (pawn.Faction != null && FactionCanBeRemoved(pawn.Faction))
			{
				QueueForRemoval(pawn.Faction);
			}
			Faction extraHomeFaction = pawn.GetExtraHomeFaction();
			if (extraHomeFaction != null && FactionCanBeRemoved(extraHomeFaction))
			{
				QueueForRemoval(extraHomeFaction);
			}
			Faction extraMiniFaction = pawn.GetExtraMiniFaction();
			if (extraMiniFaction != null && FactionCanBeRemoved(extraMiniFaction))
			{
				QueueForRemoval(extraMiniFaction);
			}
		}

		private void QueueForRemoval(Faction faction)
		{
			if (!faction.temporary)
			{
				Log.Error("Cannot queue faction " + faction.Name + " for removal, only temporary factions can be removed");
			}
			else if (!toRemove.Contains(faction))
			{
				toRemove.Add(faction);
			}
		}

		private bool FactionCanBeRemoved(Faction faction)
		{
			if (!faction.temporary || toRemove.Contains(faction) || Find.QuestManager.IsReservedByAnyQuest(faction))
			{
				return false;
			}
			List<Pawn> allMaps_Spawned = PawnsFinder.AllMaps_Spawned;
			for (int i = 0; i < allMaps_Spawned.Count; i++)
			{
				Pawn pawn = allMaps_Spawned[i];
				if (!pawn.Dead && ((pawn.Faction != null && pawn.Faction == faction) || faction == pawn.GetExtraHomeFaction() || faction == pawn.GetExtraMiniFaction()))
				{
					return false;
				}
			}
			return true;
		}
	}
}
