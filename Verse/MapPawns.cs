using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace Verse;

public sealed class MapPawns
{
	private class FactionDictionary
	{
		private Dictionary<Faction, List<Pawn>> pawnList = new Dictionary<Faction, List<Pawn>>(16);

		private List<Pawn> nullFactionPawns = new List<Pawn>(32);

		public List<Pawn> GetPawnList(Faction faction)
		{
			AssertMainThread();
			List<Pawn> value;
			List<Pawn> list = ((faction == null) ? nullFactionPawns : (pawnList.TryGetValue(faction, out value) ? value : (pawnList[faction] = new List<Pawn>(32))));
			if (list.Count > 0)
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (list[num] == null)
					{
						list.RemoveAt(num);
					}
				}
			}
			return list;
		}

		public IEnumerable<Faction> KnownFactions()
		{
			return pawnList.Keys.Concat(null);
		}
	}

	private Map map;

	private List<Pawn> pawnsSpawned = new List<Pawn>();

	private FactionDictionary pawnsInFactionSpawned = new FactionDictionary();

	private List<Pawn> prisonersOfColonySpawned = new List<Pawn>();

	private List<Pawn> slavesOfColonySpawned = new List<Pawn>();

	private List<Thing> tmpThings = new List<Thing>();

	private List<Pawn> allPawnsResult = new List<Pawn>();

	private List<Pawn> allPawnsUnspawnedResult = new List<Pawn>();

	private List<Pawn> prisonersOfColonyResult = new List<Pawn>();

	private List<Pawn> humanlikePawnsResult = new List<Pawn>();

	private List<Pawn> humanlikeSpawnedPawnsResult = new List<Pawn>();

	private List<Pawn> freeColonistsAndPrisonersResult = new List<Pawn>();

	private readonly List<Pawn> freeAdultColonistsSpawnedResult = new List<Pawn>();

	private List<Pawn> freeColonistsAndPrisonersSpawnedResult = new List<Pawn>();

	private List<Pawn> spawnedPawnsWithAnyHediffResult = new List<Pawn>();

	private List<Pawn> spawnedHumanlikesWithAnyHediffResult = new List<Pawn>();

	private List<Pawn> spawnedAnimalsWithAnyHediffResult = new List<Pawn>();

	private List<Pawn> spawnedHungryPawnsResult = new List<Pawn>();

	private List<Pawn> spawnedPawnsWithMiscNeedsResult = new List<Pawn>();

	private List<Pawn> colonyAnimalsResult = new List<Pawn>();

	private List<Pawn> spawnedColonyAnimalsResult = new List<Pawn>();

	private List<Pawn> spawnedColonyMechsResult = new List<Pawn>();

	private List<Pawn> colonySubhumansResult = new List<Pawn>();

	private List<Pawn> spawnedColonySubhumansResult = new List<Pawn>();

	private List<Pawn> spawnedDownedPawnsResult = new List<Pawn>();

	private List<Pawn> spawnedPawnsWhoShouldHaveSurgeryDoneNowResult = new List<Pawn>();

	private List<Pawn> spawnedPawnsWhoShouldHaveInventoryUnloadedResult = new List<Pawn>();

	private List<Pawn> slavesAndPrisonersOfColonySpawnedResult = new List<Pawn>();

	private List<Faction> tmpFactionsOnMap = new List<Faction>(16);

	private FactionDictionary pawnsInFactionResult = new FactionDictionary();

	private FactionDictionary freeHumanlikesOfFactionResult = new FactionDictionary();

	private FactionDictionary freeHumanlikesSpawnedOfFactionResult = new FactionDictionary();

	private FactionDictionary spawnedBabiesInFactionResult = new FactionDictionary();

	private List<Pawn> shamblersSpawned = new List<Pawn>();

	public List<Pawn> AllPawns
	{
		get
		{
			List<Pawn> allPawnsUnspawned = AllPawnsUnspawned;
			if (allPawnsUnspawned.Count == 0)
			{
				return pawnsSpawned;
			}
			allPawnsResult.Clear();
			allPawnsResult.AddRange(pawnsSpawned);
			allPawnsResult.AddRange(allPawnsUnspawned);
			return allPawnsResult;
		}
	}

	public List<Pawn> AllPawnsUnspawned
	{
		get
		{
			AssertMainThread();
			allPawnsUnspawnedResult.Clear();
			ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), allPawnsUnspawnedResult, allowUnreal: true, null, alsoGetSpawnedThings: false);
			for (int num = allPawnsUnspawnedResult.Count - 1; num >= 0; num--)
			{
				if (allPawnsUnspawnedResult[num].Dead)
				{
					allPawnsUnspawnedResult.RemoveAt(num);
				}
			}
			return allPawnsUnspawnedResult;
		}
	}

	public List<Pawn> FreeColonists => FreeHumanlikesOfFaction(Faction.OfPlayer);

	public List<Pawn> PrisonersOfColony
	{
		get
		{
			prisonersOfColonyResult.Clear();
			List<Pawn> allPawns = AllPawns;
			for (int i = 0; i < allPawns.Count; i++)
			{
				if (allPawns[i].IsPrisonerOfColony)
				{
					prisonersOfColonyResult.Add(allPawns[i]);
				}
			}
			return prisonersOfColonyResult;
		}
	}

	public List<Pawn> AllHumanlike
	{
		get
		{
			humanlikePawnsResult.Clear();
			List<Pawn> allPawns = AllPawns;
			for (int i = 0; i < allPawns.Count; i++)
			{
				if (allPawns[i].RaceProps.Humanlike)
				{
					humanlikePawnsResult.Add(allPawns[i]);
				}
			}
			return humanlikePawnsResult;
		}
	}

	public List<Pawn> AllHumanlikeSpawned
	{
		get
		{
			humanlikeSpawnedPawnsResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].RaceProps.Humanlike)
				{
					humanlikeSpawnedPawnsResult.Add(allPawnsSpawned[i]);
				}
			}
			return humanlikeSpawnedPawnsResult;
		}
	}

	public List<Pawn> FreeColonistsAndPrisoners
	{
		get
		{
			List<Pawn> freeColonists = FreeColonists;
			List<Pawn> prisonersOfColony = PrisonersOfColony;
			if (prisonersOfColony.Count == 0)
			{
				return freeColonists;
			}
			freeColonistsAndPrisonersResult.Clear();
			freeColonistsAndPrisonersResult.AddRange(freeColonists);
			freeColonistsAndPrisonersResult.AddRange(prisonersOfColony);
			return freeColonistsAndPrisonersResult;
		}
	}

	public int ColonistCount
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				Log.Error("ColonistCount while not playing. This should get the starting player pawn count.");
				return 3;
			}
			int num = 0;
			List<Pawn> allPawns = AllPawns;
			for (int i = 0; i < allPawns.Count; i++)
			{
				if (allPawns[i].IsColonist)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int AllPawnsCount => AllPawns.Count;

	public int AllPawnsUnspawnedCount => AllPawnsUnspawned.Count;

	public int FreeColonistsCount => FreeColonists.Count;

	public int PrisonersOfColonyCount => PrisonersOfColony.Count;

	public int FreeColonistsAndPrisonersCount => FreeColonistsCount + PrisonersOfColonyCount;

	public bool AnyPawnBlockingMapRemoval
	{
		get
		{
			Faction ofPlayer = Faction.OfPlayer;
			foreach (Pawn item in pawnsSpawned)
			{
				if (IsValidColonyPawn(item))
				{
					return true;
				}
				if (item.relations?.relativeInvolvedInRescueQuest != null)
				{
					return true;
				}
				if (item.Faction == ofPlayer || item.HostFaction == ofPlayer)
				{
					Job curJob = item.CurJob;
					if (curJob != null && curJob.exitMapOnArrival)
					{
						return true;
					}
					if (item.health.hediffSet.InLabor())
					{
						return true;
					}
				}
				if (CaravanExitMapUtility.FindCaravanToJoinFor(item) != null && !item.Downed)
				{
					return true;
				}
				if (ModsConfig.BiotechActive && item.IsColonyMechPlayerControlled && !item.Downed && item.GetOverseer() != null)
				{
					return true;
				}
			}
			foreach (Pawn item2 in AllPawnsUnspawned)
			{
				if (item2.SpawnedOrAnyParentSpawned && IsValidColonyPawn(item2))
				{
					return true;
				}
			}
			foreach (Thing item3 in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
			{
				if (item3 is Corpse corpse && IsValidColonyPawn(corpse.InnerPawn))
				{
					return true;
				}
			}
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
			for (int i = 0; i < list.Count; i++)
			{
				IThingHolder thingHolder = PlayerEjectablePodHolder(list[i], includeCryptosleepCaskets: false);
				if (thingHolder == null)
				{
					continue;
				}
				tmpThings.Clear();
				ThingOwnerUtility.GetAllThingsRecursively(thingHolder, tmpThings);
				for (int j = 0; j < tmpThings.Count; j++)
				{
					if (tmpThings[j] is Pawn { Dead: false, Downed: false } pawn && (pawn.IsColonist || pawn.IsColonyMech))
					{
						tmpThings.Clear();
						return true;
					}
				}
			}
			foreach (Map childPocketMap in map.ChildPocketMaps)
			{
				if (childPocketMap.mapPawns.AnyPawnBlockingMapRemoval)
				{
					return true;
				}
			}
			tmpThings.Clear();
			return false;
		}
	}

	public IReadOnlyList<Pawn> AllPawnsSpawned => pawnsSpawned;

	public List<Pawn> FreeColonistsSpawned => FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);

	public List<Pawn> FreeAdultColonistsSpawned
	{
		get
		{
			freeAdultColonistsSpawnedResult.Clear();
			foreach (Pawn item in FreeColonistsSpawned)
			{
				if (item.DevelopmentalStage.Adult())
				{
					freeAdultColonistsSpawnedResult.Add(item);
				}
			}
			return freeAdultColonistsSpawnedResult;
		}
	}

	public List<Pawn> PrisonersOfColonySpawned => prisonersOfColonySpawned;

	public List<Pawn> SlavesOfColonySpawned => slavesOfColonySpawned;

	public List<Pawn> FreeColonistsAndPrisonersSpawned
	{
		get
		{
			AssertMainThread();
			List<Pawn> freeColonistsSpawned = FreeColonistsSpawned;
			List<Pawn> list = PrisonersOfColonySpawned;
			if (list.Count == 0)
			{
				return freeColonistsSpawned;
			}
			freeColonistsAndPrisonersSpawnedResult.Clear();
			freeColonistsAndPrisonersSpawnedResult.AddRange(freeColonistsSpawned);
			freeColonistsAndPrisonersSpawnedResult.AddRange(list);
			return freeColonistsAndPrisonersSpawnedResult;
		}
	}

	public List<Pawn> SpawnedPawnsWithAnyHediff
	{
		get
		{
			spawnedPawnsWithAnyHediffResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].health.hediffSet.hediffs.Count != 0)
				{
					spawnedPawnsWithAnyHediffResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedPawnsWithAnyHediffResult;
		}
	}

	public List<Pawn> SpawnedHumanlikesWithAnyHediff
	{
		get
		{
			spawnedHumanlikesWithAnyHediffResult.Clear();
			List<Pawn> allHumanlikeSpawned = AllHumanlikeSpawned;
			for (int i = 0; i < allHumanlikeSpawned.Count; i++)
			{
				if (allHumanlikeSpawned[i].health.hediffSet.hediffs.Count != 0)
				{
					spawnedHumanlikesWithAnyHediffResult.Add(allHumanlikeSpawned[i]);
				}
			}
			return spawnedHumanlikesWithAnyHediffResult;
		}
	}

	public List<Pawn> SpawnedAnimalsWithAnyHediff
	{
		get
		{
			spawnedAnimalsWithAnyHediffResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].IsAnimal && allPawnsSpawned[i].health.hediffSet.hediffs.Count != 0)
				{
					spawnedAnimalsWithAnyHediffResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedAnimalsWithAnyHediffResult;
		}
	}

	public List<Pawn> SpawnedHungryPawns
	{
		get
		{
			spawnedHungryPawnsResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (FeedPatientUtility.IsHungry(allPawnsSpawned[i]))
				{
					spawnedHungryPawnsResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedHungryPawnsResult;
		}
	}

	public List<Pawn> SpawnedPawnsWithMiscNeeds
	{
		get
		{
			spawnedPawnsWithMiscNeedsResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (!allPawnsSpawned[i].needs.MiscNeeds.NullOrEmpty())
				{
					spawnedPawnsWithMiscNeedsResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedPawnsWithMiscNeedsResult;
		}
	}

	public List<Pawn> ColonyAnimals
	{
		get
		{
			colonyAnimalsResult.Clear();
			List<Pawn> list = PawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsAnimal)
				{
					colonyAnimalsResult.Add(list[i]);
				}
			}
			return colonyAnimalsResult;
		}
	}

	public List<Pawn> SpawnedColonyAnimals
	{
		get
		{
			spawnedColonyAnimalsResult.Clear();
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsAnimal)
				{
					spawnedColonyAnimalsResult.Add(list[i]);
				}
			}
			return spawnedColonyAnimalsResult;
		}
	}

	public List<Pawn> SpawnedColonyMechs
	{
		get
		{
			spawnedColonyMechsResult.Clear();
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsColonyMech)
				{
					spawnedColonyMechsResult.Add(list[i]);
				}
			}
			return spawnedColonyMechsResult;
		}
	}

	public List<Pawn> ColonySubhumansControllable
	{
		get
		{
			colonySubhumansResult.Clear();
			List<Pawn> list = PawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsColonySubhuman && list[i].mutant.Def.canBeDrafted)
				{
					colonySubhumansResult.Add(list[i]);
				}
			}
			return colonySubhumansResult;
		}
	}

	public List<Pawn> SpawnedColonySubhumansPlayerControlled
	{
		get
		{
			spawnedColonySubhumansResult.Clear();
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsColonySubhumanPlayerControlled)
				{
					spawnedColonySubhumansResult.Add(list[i]);
				}
			}
			return spawnedColonySubhumansResult;
		}
	}

	public List<Pawn> SpawnedDownedPawns
	{
		get
		{
			spawnedDownedPawnsResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].Downed)
				{
					spawnedDownedPawnsResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedDownedPawnsResult;
		}
	}

	public List<Pawn> SpawnedPawnsWhoShouldHaveSurgeryDoneNow
	{
		get
		{
			spawnedPawnsWhoShouldHaveSurgeryDoneNowResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (HealthAIUtility.ShouldHaveSurgeryDoneNow(allPawnsSpawned[i]))
				{
					spawnedPawnsWhoShouldHaveSurgeryDoneNowResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedPawnsWhoShouldHaveSurgeryDoneNowResult;
		}
	}

	public List<Pawn> SpawnedPawnsWhoShouldHaveInventoryUnloaded
	{
		get
		{
			spawnedPawnsWhoShouldHaveInventoryUnloadedResult.Clear();
			IReadOnlyList<Pawn> allPawnsSpawned = AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].inventory.UnloadEverything)
				{
					spawnedPawnsWhoShouldHaveInventoryUnloadedResult.Add(allPawnsSpawned[i]);
				}
			}
			return spawnedPawnsWhoShouldHaveInventoryUnloadedResult;
		}
	}

	public int AllPawnsSpawnedCount => pawnsSpawned.Count;

	public int FreeColonistsSpawnedCount => FreeColonistsSpawned.Count;

	public int FreeAdultColonistsSpawnedCount => FreeAdultColonistsSpawned.Count;

	public int PrisonersOfColonySpawnedCount => PrisonersOfColonySpawned.Count;

	public int FreeColonistsAndPrisonersSpawnedCount => FreeColonistsAndPrisonersSpawned.Count;

	public int ColonistsSpawnedCount
	{
		get
		{
			int num = 0;
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsColonist)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int FreeColonistsSpawnedOrInPlayerEjectablePodsCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < pawnsSpawned.Count; i++)
			{
				if (pawnsSpawned[i].IsFreeColonist)
				{
					num++;
				}
			}
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
			for (int j = 0; j < list.Count; j++)
			{
				IThingHolder thingHolder = PlayerEjectablePodHolder(list[j]);
				if (thingHolder == null)
				{
					continue;
				}
				tmpThings.Clear();
				ThingOwnerUtility.GetAllThingsRecursively(thingHolder, tmpThings);
				for (int k = 0; k < tmpThings.Count; k++)
				{
					if (tmpThings[k] is Pawn { Dead: false, IsFreeColonist: not false })
					{
						num++;
					}
				}
			}
			tmpThings.Clear();
			return num;
		}
	}

	public int SlavesAndPrisonersOfColonySpawnedCount => SlavesAndPrisonersOfColonySpawned.Count;

	public bool AnyColonistSpawned
	{
		get
		{
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsColonist)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnyFreeColonistSpawned
	{
		get
		{
			List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsFreeColonist)
				{
					return true;
				}
			}
			return false;
		}
	}

	public List<Pawn> SlavesAndPrisonersOfColonySpawned
	{
		get
		{
			slavesAndPrisonersOfColonySpawnedResult.Clear();
			slavesAndPrisonersOfColonySpawnedResult.AddRange(prisonersOfColonySpawned);
			slavesAndPrisonersOfColonySpawnedResult.AddRange(slavesOfColonySpawned);
			return slavesAndPrisonersOfColonySpawnedResult;
		}
	}

	public List<Pawn> SpawnedShamblers => shamblersSpawned;

	private static bool IsValidColonyPawn(Pawn pawn)
	{
		if ((!pawn.Dead || pawn.HasDeathRefusalOrResurrecting) && pawn.IsColonist)
		{
			return true;
		}
		if ((!pawn.Dead || pawn.HasDeathRefusalOrResurrecting) && pawn.IsColonySubhuman && pawn.mutant.Def.canTravelInCaravan)
		{
			return true;
		}
		return false;
	}

	private static void AssertMainThread()
	{
		if (!UnityData.IsInMainThread && !LongEventHandler.AnyEventNowOrWaiting)
		{
			throw new Exception("Accessing map pawns off main thread - this is never allowed due to list pooling and will result in modification exceptions elsewhere in code.");
		}
	}

	private static IThingHolder PlayerEjectablePodHolder(Thing thing, bool includeCryptosleepCaskets = true)
	{
		Building_CryptosleepCasket building_CryptosleepCasket = thing as Building_CryptosleepCasket;
		CompTransporter compTransporter = thing.TryGetComp<CompTransporter>();
		CompBiosculpterPod compBiosculpterPod = thing.TryGetComp<CompBiosculpterPod>();
		if ((includeCryptosleepCaskets && building_CryptosleepCasket != null && building_CryptosleepCasket.def.building.isPlayerEjectable) || thing is IActiveTransporter || thing is PawnFlyer || thing is Building_Enterable || compTransporter != null || compBiosculpterPod != null)
		{
			IThingHolder thingHolder = compTransporter;
			object obj = thingHolder;
			if (obj == null)
			{
				thingHolder = compBiosculpterPod;
				obj = thingHolder ?? (thing as IThingHolder);
			}
			return (IThingHolder)obj;
		}
		return null;
	}

	public MapPawns(Map map)
	{
		this.map = map;
	}

	private void EnsureFactionsListsInit()
	{
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			pawnsInFactionSpawned.GetPawnList(allFactionsListForReading[i]);
		}
	}

	public List<Faction> FactionsOnMap()
	{
		tmpFactionsOnMap.Clear();
		foreach (Faction item in pawnsInFactionResult.KnownFactions())
		{
			if (pawnsInFactionResult.GetPawnList(item).Count > 0)
			{
				tmpFactionsOnMap.Add(item);
			}
		}
		return tmpFactionsOnMap;
	}

	public List<Pawn> PawnsInFaction(Faction faction)
	{
		List<Pawn> pawnList = pawnsInFactionResult.GetPawnList(faction);
		pawnList.Clear();
		List<Pawn> allPawns = AllPawns;
		for (int i = 0; i < allPawns.Count; i++)
		{
			if (allPawns[i].Faction == faction)
			{
				pawnList.Add(allPawns[i]);
			}
		}
		return pawnList;
	}

	public List<Pawn> SpawnedPawnsInFaction(Faction faction)
	{
		EnsureFactionsListsInit();
		return pawnsInFactionSpawned.GetPawnList(faction);
	}

	public List<Pawn> FreeHumanlikesOfFaction(Faction faction)
	{
		List<Pawn> pawnList = freeHumanlikesOfFactionResult.GetPawnList(faction);
		pawnList.Clear();
		List<Pawn> allPawns = AllPawns;
		for (int i = 0; i < allPawns.Count; i++)
		{
			if ((!ModsConfig.AnomalyActive || !allPawns[i].IsSubhuman) && allPawns[i].Faction == faction && (allPawns[i].HostFaction == null || allPawns[i].IsSlave) && allPawns[i].RaceProps.Humanlike)
			{
				pawnList.Add(allPawns[i]);
			}
		}
		return pawnList;
	}

	public List<Pawn> FreeHumanlikesSpawnedOfFaction(Faction faction)
	{
		List<Pawn> pawnList = freeHumanlikesSpawnedOfFactionResult.GetPawnList(faction);
		pawnList.Clear();
		List<Pawn> list = SpawnedPawnsInFaction(faction);
		for (int i = 0; i < list.Count; i++)
		{
			if ((!ModsConfig.AnomalyActive || !list[i].IsSubhuman) && list[i].HostFaction == null && list[i].RaceProps.Humanlike)
			{
				pawnList.Add(list[i]);
			}
		}
		return pawnList;
	}

	public List<Pawn> SpawnedBabiesInFaction(Faction faction)
	{
		List<Pawn> pawnList = spawnedBabiesInFactionResult.GetPawnList(faction);
		pawnList.Clear();
		List<Pawn> list = SpawnedPawnsInFaction(faction);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].DevelopmentalStage.Baby())
			{
				pawnList.Add(list[i]);
			}
		}
		return pawnList;
	}

	public void RegisterPawn(Pawn p)
	{
		if (p.Dead)
		{
			Log.Warning("Tried to register dead pawn " + p?.ToString() + " in " + GetType()?.ToString() + ".");
		}
		else if (!p.Spawned)
		{
			Log.Warning("Tried to register despawned pawn " + p?.ToString() + " in " + GetType()?.ToString() + ".");
		}
		else if (p.Map != map)
		{
			Log.Warning("Tried to register pawn " + p?.ToString() + " but his Map is not this one.");
		}
		else
		{
			if (!p.mindState.Active)
			{
				return;
			}
			EnsureFactionsListsInit();
			if (!pawnsSpawned.Contains(p))
			{
				pawnsSpawned.Add(p);
			}
			if (p.Faction != null)
			{
				List<Pawn> pawnList = pawnsInFactionSpawned.GetPawnList(p.Faction);
				if (!pawnList.Contains(p))
				{
					pawnList.Add(p);
					if (p.Faction == Faction.OfPlayer)
					{
						pawnList.InsertionSort(delegate(Pawn a, Pawn b)
						{
							int num = a.playerSettings?.joinTick ?? 0;
							int value = b.playerSettings?.joinTick ?? 0;
							return num.CompareTo(value);
						});
					}
				}
			}
			if (p.IsPrisonerOfColony && !prisonersOfColonySpawned.Contains(p))
			{
				prisonersOfColonySpawned.Add(p);
			}
			if (p.IsSlaveOfColony && !slavesOfColonySpawned.Contains(p))
			{
				slavesOfColonySpawned.Add(p);
			}
			DoListChangedNotifications();
		}
	}

	public void DeRegisterPawn(Pawn p)
	{
		EnsureFactionsListsInit();
		pawnsSpawned.Remove(p);
		List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
		for (int i = 0; i < allFactionsListForReading.Count; i++)
		{
			Faction faction = allFactionsListForReading[i];
			pawnsInFactionSpawned.GetPawnList(faction).Remove(p);
		}
		prisonersOfColonySpawned.Remove(p);
		slavesOfColonySpawned.Remove(p);
		DoListChangedNotifications();
	}

	public void UpdateRegistryForPawn(Pawn p)
	{
		DeRegisterPawn(p);
		if (p.Spawned && p.Map == map)
		{
			RegisterPawn(p);
		}
		DoListChangedNotifications();
	}

	public void RegisterShambler(Pawn p)
	{
		if (ModLister.CheckAnomaly("Shambler registration") && !shamblersSpawned.Contains(p))
		{
			shamblersSpawned.Add(p);
		}
	}

	public void DeregisterShambler(Pawn p)
	{
		if (ModLister.CheckAnomaly("Shambler deregistration"))
		{
			shamblersSpawned.Remove(p);
		}
	}

	private void DoListChangedNotifications()
	{
		MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
		if (Find.ColonistBar != null)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
	}

	public void LogListedPawns()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MapPawns:");
		stringBuilder.AppendLine("pawnsSpawned");
		foreach (Pawn item in pawnsSpawned)
		{
			stringBuilder.AppendLine("    " + item);
		}
		stringBuilder.AppendLine("AllPawnsUnspawned");
		foreach (Pawn item2 in AllPawnsUnspawned)
		{
			stringBuilder.AppendLine("    " + item2);
		}
		foreach (Faction item3 in pawnsInFactionSpawned.KnownFactions())
		{
			stringBuilder.AppendLine("pawnsInFactionSpawned[" + item3.ToStringSafe() + "]");
			foreach (Pawn pawn in pawnsInFactionSpawned.GetPawnList(item3))
			{
				stringBuilder.AppendLine("    " + pawn);
			}
		}
		stringBuilder.AppendLine("prisonersOfColonySpawned");
		foreach (Pawn item4 in prisonersOfColonySpawned)
		{
			stringBuilder.AppendLine("    " + item4);
		}
		Log.Message(stringBuilder.ToString());
	}
}
