using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class PawnsFinder
{
	private static List<Pawn> allMapsWorldAndTemporary_AliveOrDead_Result = new List<Pawn>();

	private static List<Pawn> allMapsWorldAndTemporary_Alive_Result = new List<Pawn>();

	private static List<Pawn> allMapsAndWorld_Alive_Result = new List<Pawn>();

	private static List<Pawn> allMaps_Result = new List<Pawn>();

	private static List<Pawn> allMaps_Spawned_Result = new List<Pawn>();

	private static List<Pawn> all_AliveOrDead_Result = new List<Pawn>();

	private static List<Pawn> temporary_Result = new List<Pawn>();

	private static List<Pawn> temporary_Alive_Result = new List<Pawn>();

	private static List<Pawn> temporary_Dead_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_Result = new List<Pawn>();

	private static List<Pawn> allCaravansAndTravellingTransporters_Alive_Result = new List<Pawn>();

	private static List<Pawn> allCaravansAndTravellingTransporters_AliveOrDead_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_Colonists_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep_Result = new List<Pawn>();

	private static List<Pawn> allMaps_PrisonersOfColonySpawned_Result = new List<Pawn>();

	private static List<Pawn> allMaps_PrisonersOfColony_Result = new List<Pawn>();

	private static List<Pawn> allMaps_FreeColonists_Result = new List<Pawn>();

	private static List<Pawn> allMaps_FreeColonistsSpawned_Result = new List<Pawn>();

	private static List<Pawn> allMaps_FreeColonistsAndPrisonersSpawned_Result = new List<Pawn>();

	private static List<Pawn> allMaps_FreeColonistsAndPrisoners_Result = new List<Pawn>();

	private static List<Pawn> allMaps_ColonySubhumansSpawned_Result = new List<Pawn>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended_Result = new List<Pawn>();

	private static Dictionary<Faction, List<Pawn>> allMaps_SpawnedPawnsInFaction_Result = new Dictionary<Faction, List<Pawn>>();

	private static List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result = new List<Pawn>();

	private static List<Pawn> homeMaps_FreeColonistsSpawned_Result = new List<Pawn>();

	public static List<Pawn> AllMapsWorldAndTemporary_AliveOrDead
	{
		get
		{
			allMapsWorldAndTemporary_AliveOrDead_Result.Clear();
			allMapsWorldAndTemporary_AliveOrDead_Result.AddRange(AllMapsWorldAndTemporary_Alive);
			if (Find.World != null)
			{
				allMapsWorldAndTemporary_AliveOrDead_Result.AddRange(Find.WorldPawns.AllPawnsDead);
			}
			allMapsWorldAndTemporary_AliveOrDead_Result.AddRange(Temporary_Dead);
			if (Find.CurrentGravship != null)
			{
				allMapsWorldAndTemporary_AliveOrDead_Result.AddRange(Find.CurrentGravship.Pawns);
			}
			return allMapsWorldAndTemporary_AliveOrDead_Result;
		}
	}

	public static List<Pawn> AllMapsWorldAndTemporary_Alive
	{
		get
		{
			allMapsWorldAndTemporary_Alive_Result.Clear();
			allMapsWorldAndTemporary_Alive_Result.AddRange(AllMaps);
			if (Find.World != null)
			{
				allMapsWorldAndTemporary_Alive_Result.AddRange(Find.WorldPawns.AllPawnsAlive);
			}
			allMapsWorldAndTemporary_Alive_Result.AddRange(Temporary_Alive);
			if (Find.CurrentGravship != null)
			{
				allMapsWorldAndTemporary_Alive_Result.AddRange(Find.CurrentGravship.Pawns);
			}
			return allMapsWorldAndTemporary_Alive_Result;
		}
	}

	public static List<Pawn> AllMapsAndWorld_Alive
	{
		get
		{
			allMapsAndWorld_Alive_Result.Clear();
			allMapsAndWorld_Alive_Result.AddRange(AllMaps);
			if (Find.World != null)
			{
				allMapsAndWorld_Alive_Result.AddRange(Find.WorldPawns.AllPawnsAlive);
			}
			if (Find.CurrentGravship != null)
			{
				allMapsAndWorld_Alive_Result.AddRange(Find.CurrentGravship.Pawns);
			}
			return allMapsAndWorld_Alive_Result;
		}
	}

	public static List<Pawn> AllMaps
	{
		get
		{
			allMaps_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.AllPawns;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_Result.AddRange(maps[i].mapPawns.AllPawns);
				}
			}
			return allMaps_Result;
		}
	}

	public static IReadOnlyList<Pawn> AllMaps_Spawned
	{
		get
		{
			allMaps_Spawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.AllPawnsSpawned;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_Spawned_Result.AddRange(maps[i].mapPawns.AllPawnsSpawned);
				}
			}
			return allMaps_Spawned_Result;
		}
	}

	public static List<Pawn> All_AliveOrDead
	{
		get
		{
			List<Pawn> allMapsWorldAndTemporary_AliveOrDead = AllMapsWorldAndTemporary_AliveOrDead;
			List<Pawn> allCaravansAndTravellingTransporters_AliveOrDead = AllCaravansAndTravellingTransporters_AliveOrDead;
			if (allCaravansAndTravellingTransporters_AliveOrDead.Count == 0)
			{
				return allMapsWorldAndTemporary_AliveOrDead;
			}
			all_AliveOrDead_Result.Clear();
			all_AliveOrDead_Result.AddRange(allMapsWorldAndTemporary_AliveOrDead);
			all_AliveOrDead_Result.AddRange(allCaravansAndTravellingTransporters_AliveOrDead);
			return all_AliveOrDead_Result;
		}
	}

	public static List<Pawn> Temporary
	{
		get
		{
			temporary_Result.Clear();
			List<List<Pawn>> pawnsBeingGeneratedNow = PawnGroupKindWorker.pawnsBeingGeneratedNow;
			for (int i = 0; i < pawnsBeingGeneratedNow.Count; i++)
			{
				temporary_Result.AddRange(pawnsBeingGeneratedNow[i]);
			}
			List<List<Thing>> thingsBeingGeneratedNow = ThingSetMaker.thingsBeingGeneratedNow;
			for (int j = 0; j < thingsBeingGeneratedNow.Count; j++)
			{
				List<Thing> list = thingsBeingGeneratedNow[j];
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] is Pawn item)
					{
						temporary_Result.Add(item);
					}
				}
			}
			if (Current.ProgramState != ProgramState.Playing && Find.GameInitData != null)
			{
				List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
				for (int l = 0; l < startingAndOptionalPawns.Count; l++)
				{
					if (startingAndOptionalPawns[l] != null)
					{
						temporary_Result.Add(startingAndOptionalPawns[l]);
					}
				}
			}
			if (Find.World != null)
			{
				List<Site> sites = Find.WorldObjects.Sites;
				for (int m = 0; m < sites.Count; m++)
				{
					for (int n = 0; n < sites[m].parts.Count; n++)
					{
						if (sites[m].parts[n].things == null || sites[m].parts[n].things.contentsLookMode != LookMode.Deep)
						{
							continue;
						}
						ThingOwner things = sites[m].parts[n].things;
						for (int num = 0; num < things.Count; num++)
						{
							if (things[num] is Pawn item2)
							{
								temporary_Result.Add(item2);
							}
						}
					}
				}
			}
			if (Find.World != null)
			{
				List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
				for (int num2 = 0; num2 < allWorldObjects.Count; num2++)
				{
					DownedRefugeeComp component = allWorldObjects[num2].GetComponent<DownedRefugeeComp>();
					if (component != null && component.pawn != null && component.pawn.Any)
					{
						temporary_Result.Add(component.pawn[0]);
					}
					PrisonerWillingToJoinComp component2 = allWorldObjects[num2].GetComponent<PrisonerWillingToJoinComp>();
					if (component2 != null && component2.pawn != null && component2.pawn.Any)
					{
						temporary_Result.Add(component2.pawn[0]);
					}
				}
			}
			return temporary_Result;
		}
	}

	public static List<Pawn> Temporary_Alive
	{
		get
		{
			temporary_Alive_Result.Clear();
			List<Pawn> temporary = Temporary;
			for (int i = 0; i < temporary.Count; i++)
			{
				if (!temporary[i].Dead)
				{
					temporary_Alive_Result.Add(temporary[i]);
				}
			}
			return temporary_Alive_Result;
		}
	}

	public static List<Pawn> Temporary_Dead
	{
		get
		{
			temporary_Dead_Result.Clear();
			List<Pawn> temporary = Temporary;
			for (int i = 0; i < temporary.Count; i++)
			{
				if (temporary[i].Dead)
				{
					temporary_Dead_Result.Add(temporary[i]);
				}
			}
			return temporary_Dead_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive
	{
		get
		{
			List<Pawn> allMaps = AllMaps;
			List<Pawn> allCaravansAndTravellingTransporters_Alive = AllCaravansAndTravellingTransporters_Alive;
			if (allCaravansAndTravellingTransporters_Alive.Count == 0)
			{
				return allMaps;
			}
			allMapsCaravansAndTravellingTransporters_Alive_Result.Clear();
			allMapsCaravansAndTravellingTransporters_Alive_Result.AddRange(allMaps);
			allMapsCaravansAndTravellingTransporters_Alive_Result.AddRange(allCaravansAndTravellingTransporters_Alive);
			return allMapsCaravansAndTravellingTransporters_Alive_Result;
		}
	}

	public static IReadOnlyList<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned
	{
		get
		{
			IReadOnlyList<Pawn> allMaps_Spawned = AllMaps_Spawned;
			List<Pawn> allCaravansAndTravellingTransporters_Alive = AllCaravansAndTravellingTransporters_Alive;
			if (allCaravansAndTravellingTransporters_Alive.Count == 0)
			{
				return allMaps_Spawned;
			}
			allMapsCaravansAndTravellingTransporters_AliveSpawned_Result.Clear();
			allMapsCaravansAndTravellingTransporters_AliveSpawned_Result.AddRange(allMaps_Spawned);
			allMapsCaravansAndTravellingTransporters_AliveSpawned_Result.AddRange(allCaravansAndTravellingTransporters_Alive);
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_Result;
		}
	}

	public static List<Pawn> AllCaravansAndTravellingTransporters_Alive
	{
		get
		{
			allCaravansAndTravellingTransporters_Alive_Result.Clear();
			List<Pawn> allCaravansAndTravellingTransporters_AliveOrDead = AllCaravansAndTravellingTransporters_AliveOrDead;
			for (int i = 0; i < allCaravansAndTravellingTransporters_AliveOrDead.Count; i++)
			{
				if (!allCaravansAndTravellingTransporters_AliveOrDead[i].Dead)
				{
					allCaravansAndTravellingTransporters_Alive_Result.Add(allCaravansAndTravellingTransporters_AliveOrDead[i]);
				}
			}
			return allCaravansAndTravellingTransporters_Alive_Result;
		}
	}

	public static List<Pawn> AllCaravansAndTravellingTransporters_AliveOrDead
	{
		get
		{
			allCaravansAndTravellingTransporters_AliveOrDead_Result.Clear();
			if (Find.World != null)
			{
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int i = 0; i < caravans.Count; i++)
				{
					allCaravansAndTravellingTransporters_AliveOrDead_Result.AddRange(caravans[i].PawnsListForReading);
				}
				List<TravellingTransporters> travellingTransporters = Find.WorldObjects.TravellingTransporters;
				for (int j = 0; j < travellingTransporters.Count; j++)
				{
					allCaravansAndTravellingTransporters_AliveOrDead_Result.AddRange(travellingTransporters[j].Pawns);
				}
				if (Find.CurrentGravship != null)
				{
					foreach (Pawn pawn in Find.CurrentGravship.Pawns)
					{
						if (!pawn.SpawnedOrAnyParentSpawned)
						{
							allCaravansAndTravellingTransporters_AliveOrDead_Result.Add(pawn);
						}
					}
				}
			}
			return allCaravansAndTravellingTransporters_AliveOrDead_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_Colonists
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_Colonists_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsColonist)
				{
					allMapsCaravansAndTravellingTransporters_Alive_Colonists_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_Colonists_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsColonist && !allMapsCaravansAndTravellingTransporters_Alive[i].IsSlave)
				{
					allMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsFreeColonist)
				{
					allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_Result.Clear();
			IReadOnlyList<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned = AllMapsCaravansAndTravellingTransporters_AliveSpawned;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_AliveSpawned.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_AliveSpawned[i].IsFreeColonist)
				{
					allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_Result.Add(allMapsCaravansAndTravellingTransporters_AliveSpawned[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsFreeColonist && !allMapsCaravansAndTravellingTransporters_Alive[i].IsQuestLodger())
				{
					allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsFreeColonist && !allMapsCaravansAndTravellingTransporters_Alive[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended_Result.Clear();
			IReadOnlyList<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned = AllMapsCaravansAndTravellingTransporters_AliveSpawned;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_AliveSpawned.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_AliveSpawned[i].IsFreeColonist && !allMapsCaravansAndTravellingTransporters_AliveSpawned[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended_Result.Add(allMapsCaravansAndTravellingTransporters_AliveSpawned[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsFreeColonist && !allMapsCaravansAndTravellingTransporters_Alive[i].InCryptosleep)
				{
					allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_Result.Clear();
			Faction ofPlayer = Faction.OfPlayer;
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].Faction == ofPlayer)
				{
					allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep_Result.Clear();
			Faction ofPlayer = Faction.OfPlayer;
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].Faction == ofPlayer && !allMapsCaravansAndTravellingTransporters_Alive[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsPrisonerOfColony)
				{
					allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony_Result.Clear();
			IReadOnlyList<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned = AllMapsCaravansAndTravellingTransporters_AliveSpawned;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_AliveSpawned.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_AliveSpawned[i].IsPrisonerOfColony)
				{
					allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony_Result.Add(allMapsCaravansAndTravellingTransporters_AliveSpawned[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive = AllMapsCaravansAndTravellingTransporters_Alive;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive[i].IsSlaveOfColony)
				{
					allMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony_Result.Add(allMapsCaravansAndTravellingTransporters_Alive[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners
	{
		get
		{
			List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists = AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists;
			List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony = AllMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony;
			if (allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony.Count == 0)
			{
				return allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists;
			}
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_Result.Clear();
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists);
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravellingTransporters_AliveSpawned_PrisonersOfColony);
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners
	{
		get
		{
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists = AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony = AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony;
			if (allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony.Count == 0)
			{
				return allMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
			}
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result.Clear();
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravellingTransporters_Alive_FreeColonists);
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony);
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners = AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners.Count; i++)
			{
				if (!allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Add(allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners = AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners.Count; i++)
			{
				if (!allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep_Result.Add(allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep_Result;
		}
	}

	public static List<Pawn> AllMaps_PrisonersOfColonySpawned
	{
		get
		{
			allMaps_PrisonersOfColonySpawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.PrisonersOfColonySpawned;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_PrisonersOfColonySpawned_Result.AddRange(maps[i].mapPawns.PrisonersOfColonySpawned);
				}
			}
			return allMaps_PrisonersOfColonySpawned_Result;
		}
	}

	public static List<Pawn> AllMaps_PrisonersOfColony
	{
		get
		{
			allMaps_PrisonersOfColony_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.PrisonersOfColony;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_PrisonersOfColony_Result.AddRange(maps[i].mapPawns.PrisonersOfColony);
				}
			}
			return allMaps_PrisonersOfColony_Result;
		}
	}

	public static List<Pawn> AllMaps_FreeColonists
	{
		get
		{
			allMaps_FreeColonists_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.FreeColonists;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_FreeColonists_Result.AddRange(maps[i].mapPawns.FreeColonists);
				}
			}
			return allMaps_FreeColonists_Result;
		}
	}

	public static List<Pawn> AllMaps_FreeColonistsSpawned
	{
		get
		{
			allMaps_FreeColonistsSpawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.FreeColonistsSpawned;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_FreeColonistsSpawned_Result.AddRange(maps[i].mapPawns.FreeColonistsSpawned);
				}
			}
			return allMaps_FreeColonistsSpawned_Result;
		}
	}

	public static List<Pawn> AllMaps_FreeColonistsAndPrisonersSpawned
	{
		get
		{
			allMaps_FreeColonistsAndPrisonersSpawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.FreeColonistsAndPrisonersSpawned;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_FreeColonistsAndPrisonersSpawned_Result.AddRange(maps[i].mapPawns.FreeColonistsAndPrisonersSpawned);
				}
			}
			return allMaps_FreeColonistsAndPrisonersSpawned_Result;
		}
	}

	public static List<Pawn> AllMaps_FreeColonistsAndPrisoners
	{
		get
		{
			allMaps_FreeColonistsAndPrisoners_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.FreeColonistsAndPrisoners;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_FreeColonistsAndPrisoners_Result.AddRange(maps[i].mapPawns.FreeColonistsAndPrisoners);
				}
			}
			return allMaps_FreeColonistsAndPrisoners_Result;
		}
	}

	public static List<Pawn> AllMaps_ColonySubhumansSpawnedPlayerControlled
	{
		get
		{
			allMaps_ColonySubhumansSpawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					return maps[0].mapPawns.SpawnedColonySubhumansPlayerControlled;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					allMaps_ColonySubhumansSpawned_Result.AddRange(maps[i].mapPawns.SpawnedColonySubhumansPlayerControlled);
				}
			}
			return allMaps_ColonySubhumansSpawned_Result;
		}
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended
	{
		get
		{
			allMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended_Result.Clear();
			List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction = AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;
			for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction.Count; i++)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction[i].IsColonySubhuman && !allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction[i].Suspended)
				{
					allMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended_Result.Add(allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction[i]);
				}
			}
			return allMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended_Result;
		}
	}

	public static List<Pawn> HomeMaps_FreeColonistsSpawned
	{
		get
		{
			homeMaps_FreeColonistsSpawned_Result.Clear();
			if (Current.ProgramState != ProgramState.Entry)
			{
				List<Map> maps = Find.Maps;
				if (maps.Count == 1)
				{
					if (!maps[0].IsPlayerHome)
					{
						return homeMaps_FreeColonistsSpawned_Result;
					}
					return maps[0].mapPawns.FreeColonistsSpawned;
				}
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome)
					{
						homeMaps_FreeColonistsSpawned_Result.AddRange(maps[i].mapPawns.FreeColonistsSpawned);
					}
				}
			}
			return homeMaps_FreeColonistsSpawned_Result;
		}
	}

	public static List<Pawn> AllMaps_SpawnedPawnsInFaction(Faction faction)
	{
		if (!allMaps_SpawnedPawnsInFaction_Result.TryGetValue(faction, out var value))
		{
			value = new List<Pawn>();
			allMaps_SpawnedPawnsInFaction_Result.Add(faction, value);
		}
		value.Clear();
		if (Current.ProgramState != ProgramState.Entry)
		{
			List<Map> maps = Find.Maps;
			if (maps.Count == 1)
			{
				return maps[0].mapPawns.SpawnedPawnsInFaction(faction);
			}
			for (int i = 0; i < maps.Count; i++)
			{
				value.AddRange(maps[i].mapPawns.SpawnedPawnsInFaction(faction));
			}
		}
		return value;
	}

	public static List<Pawn> AllMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype(XenotypeDef def)
	{
		allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result.Clear();
		if (!ModsConfig.BiotechActive)
		{
			return allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result;
		}
		List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_Colonists = AllMapsCaravansAndTravellingTransporters_Alive_Colonists;
		for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive_Colonists.Count; i++)
		{
			if (allMapsCaravansAndTravellingTransporters_Alive_Colonists[i].genes != null && allMapsCaravansAndTravellingTransporters_Alive_Colonists[i].genes.Xenotype == def)
			{
				allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result.Add(allMapsCaravansAndTravellingTransporters_Alive_Colonists[i]);
			}
		}
		return allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result;
	}

	public static void Clear()
	{
		allMapsWorldAndTemporary_AliveOrDead_Result.Clear();
		allMapsWorldAndTemporary_Alive_Result.Clear();
		allMapsAndWorld_Alive_Result.Clear();
		allMaps_Result.Clear();
		allMaps_Spawned_Result.Clear();
		all_AliveOrDead_Result.Clear();
		temporary_Result.Clear();
		temporary_Alive_Result.Clear();
		temporary_Dead_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_Result.Clear();
		allCaravansAndTravellingTransporters_Alive_Result.Clear();
		allCaravansAndTravellingTransporters_AliveOrDead_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_Colonists_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Clear();
		allMaps_PrisonersOfColonySpawned_Result.Clear();
		allMaps_PrisonersOfColony_Result.Clear();
		allMaps_FreeColonists_Result.Clear();
		allMaps_FreeColonistsSpawned_Result.Clear();
		allMaps_FreeColonistsAndPrisonersSpawned_Result.Clear();
		allMaps_FreeColonistsAndPrisoners_Result.Clear();
		allMaps_SpawnedPawnsInFaction_Result.Clear();
		homeMaps_FreeColonistsSpawned_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_Colonists_NoSlaves_Result.Clear();
		allMapsCaravansAndTravellingTransporters_Alive_Colonists_OfXenotype_Result.Clear();
	}
}
