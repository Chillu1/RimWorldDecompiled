using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class FactionManager : IExposable
{
	private List<Faction> allFactions = new List<Faction>();

	private List<Faction> toRemove = new List<Faction>();

	public GoodwillSituationManager goodwillSituationManager = new GoodwillSituationManager();

	private Faction ofPlayer;

	private Faction ofMechanoids;

	private Faction ofInsects;

	private Faction ofAncients;

	private Faction ofAncientsHostile;

	private Faction empire;

	private Faction ofPirates;

	private Faction ofHoraxCult;

	private Faction ofEntities;

	private Faction ofTradersGuild;

	private Faction ofSalvagers;

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

	public Faction OfEmpire => empire;

	public Faction OfPirates => ofPirates;

	public Faction OfHoraxCult => ofHoraxCult;

	public Faction OfEntities => ofEntities;

	public Faction OfTradersGuild => ofTradersGuild;

	public Faction OfSalvagers => ofSalvagers;

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
		if (allFactions.Contains(faction))
		{
			return;
		}
		allFactions.Add(faction);
		RecacheFactions();
		foreach (Map map in Find.Maps)
		{
			map.events.Notify_FactionAdded(faction);
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
			faction.RemoveAllRelations();
			allFactions.Remove(faction);
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
				Find.Maps[j].listerBuildings.Notify_FactionRemoved(faction);
			}
			Find.LetterStack.Notify_FactionRemoved(faction);
			Find.PlayLog.Notify_FactionRemoved(faction);
			RecacheFactions();
			Find.QuestManager.Notify_FactionRemoved(faction);
			Find.IdeoManager.Notify_FactionRemoved(faction);
			Find.TaleManager.Notify_FactionRemoved(faction);
			foreach (Map map in Find.Maps)
			{
				map.events.Notify_FactionRemoved(faction);
			}
		}
	}

	public void FactionManagerTick()
	{
		goodwillSituationManager.GoodwillManagerTick();
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

	public bool TryGetRandomNonColonyHumanlikeFaction(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false, TechLevel minTechLevel = TechLevel.Undefined, TechLevel maxTechLevel = TechLevel.Undefined, bool allowTemporary = false, bool requireHostile = false)
	{
		return AllFactions.Where((Faction x) => !x.IsPlayer && !x.Hidden && x.def.humanlikeFaction && (allowDefeated || !x.defeated) && (allowTemporary || !x.temporary) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel) && (maxTechLevel == TechLevel.Undefined || (int)x.def.techLevel <= (int)maxTechLevel) && (!requireHostile || x.HostileTo(Faction.OfPlayer))).TryRandomElementByWeight((Faction x) => (tryMedievalOrBetter && (int)x.def.techLevel < 3) ? 0.1f : 1f, out faction);
	}

	public IEnumerable<Faction> GetFactions(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined, bool allowTemporary = false)
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

	public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
	{
		if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
			where x.HostileTo(Faction.OfPlayer)
			select x).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	public Faction RandomRaidableEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
	{
		if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
			where x.HostileTo(Faction.OfPlayer) && !x.def.pawnGroupMakers.NullOrEmpty()
			select x).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	public Faction RandomNonHostileFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
	{
		if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
			where !x.HostileTo(Faction.OfPlayer)
			select x).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	public Faction RandomAlliedFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
	{
		if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
			where x.PlayerRelationKind == FactionRelationKind.Ally
			select x).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	public Faction RandomRoyalFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
	{
		if ((from x in GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel)
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

	public void LogAllFactions()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Faction allFaction in allFactions)
		{
			stringBuilder.AppendLine($"name: {allFaction.Name}, temporary: {allFaction.temporary}, can be deleted?: {FactionCanBeRemoved(allFaction)}");
		}
		stringBuilder.AppendLine($"{allFactions.Count} factions found.");
		Log.Message(stringBuilder.ToString());
	}

	public void LogFactionsToRemove()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Faction item in toRemove)
		{
			stringBuilder.AppendLine($"name: {item.Name}, temporary: {item.temporary}, can be deleted?: {FactionCanBeRemoved(item)}");
		}
		stringBuilder.AppendLine($"{toRemove.Count} factions found.");
		Log.Message(stringBuilder.ToString());
	}

	public void LogFactionsOnPawns()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IGrouping<Faction, Pawn> item in from p in Find.WorldPawns.AllPawnsAliveOrDead
			group p by p.Faction)
		{
			if (item.Key == null)
			{
				stringBuilder.AppendLine($"no faction: {item.Count()} pawns found.");
			}
			else
			{
				stringBuilder.AppendLine($"{item.Key}: {item.Count()} pawns found.");
			}
		}
		Log.Message(stringBuilder.ToString());
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
		ofPirates = FirstFactionOfDef(FactionDefOf.Pirate);
		ofHoraxCult = FirstFactionOfDef(FactionDefOf.HoraxCult);
		ofEntities = FirstFactionOfDef(FactionDefOf.Entities);
		ofTradersGuild = FirstFactionOfDef(FactionDefOf.TradersGuild);
		ofSalvagers = FirstFactionOfDef(FactionDefOf.Salvagers);
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

	public void Notify_PawnLeftFaction(Faction oldFaction)
	{
		if (FactionCanBeRemoved(oldFaction))
		{
			QueueForRemoval(oldFaction);
		}
	}

	public void Notify_WorldObjectDestroyed(WorldObject worldObject)
	{
		if (worldObject.Faction != null && FactionCanBeRemoved(worldObject.Faction))
		{
			QueueForRemoval(worldObject.Faction);
		}
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
		if (pawn.SlaveFaction != null && FactionCanBeRemoved(pawn.SlaveFaction))
		{
			QueueForRemoval(pawn.SlaveFaction);
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
		if (!CheckPawns(PawnsFinder.AllMaps_Spawned))
		{
			return false;
		}
		if (!CheckPawns(PawnsFinder.AllCaravansAndTravellingTransporters_Alive))
		{
			return false;
		}
		foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
		{
			if (allWorldObject.Faction == faction)
			{
				return false;
			}
		}
		return true;
		bool CheckPawns(IReadOnlyList<Pawn> pawns)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (!pawn.Dead && ((pawn.Faction != null && pawn.Faction == faction) || faction == pawn.GetExtraHomeFaction() || faction == pawn.GetExtraMiniFaction() || faction == pawn.SlaveFaction || (faction.leader == pawn && pawn.Faction != Faction.OfPlayer)))
				{
					return false;
				}
			}
			return true;
		}
	}
}
