using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
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

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_Result = new List<Pawn>();

		private static List<Pawn> allCaravansAndTravelingTransportPods_Alive_Result = new List<Pawn>();

		private static List<Pawn> allCaravansAndTravelingTransportPods_AliveOrDead_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_Colonists_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result = new List<Pawn>();

		private static List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result = new List<Pawn>();

		private static List<Pawn> allMaps_PrisonersOfColonySpawned_Result = new List<Pawn>();

		private static List<Pawn> allMaps_PrisonersOfColony_Result = new List<Pawn>();

		private static List<Pawn> allMaps_FreeColonists_Result = new List<Pawn>();

		private static List<Pawn> allMaps_FreeColonistsSpawned_Result = new List<Pawn>();

		private static List<Pawn> allMaps_FreeColonistsAndPrisonersSpawned_Result = new List<Pawn>();

		private static List<Pawn> allMaps_FreeColonistsAndPrisoners_Result = new List<Pawn>();

		private static Dictionary<Faction, List<Pawn>> allMaps_SpawnedPawnsInFaction_Result = new Dictionary<Faction, List<Pawn>>();

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
				return allMapsAndWorld_Alive_Result;
			}
		}

		public static List<Pawn> AllMaps
		{
			get
			{
				allMaps_Result.Clear();
				if (Current.ProgramState != 0)
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

		public static List<Pawn> AllMaps_Spawned
		{
			get
			{
				allMaps_Spawned_Result.Clear();
				if (Current.ProgramState != 0)
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
				List<Pawn> allCaravansAndTravelingTransportPods_AliveOrDead = AllCaravansAndTravelingTransportPods_AliveOrDead;
				if (allCaravansAndTravelingTransportPods_AliveOrDead.Count == 0)
				{
					return allMapsWorldAndTemporary_AliveOrDead;
				}
				all_AliveOrDead_Result.Clear();
				all_AliveOrDead_Result.AddRange(allMapsWorldAndTemporary_AliveOrDead);
				all_AliveOrDead_Result.AddRange(allCaravansAndTravelingTransportPods_AliveOrDead);
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
						Pawn pawn = list[k] as Pawn;
						if (pawn != null)
						{
							temporary_Result.Add(pawn);
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
								Pawn pawn2 = things[num] as Pawn;
								if (pawn2 != null)
								{
									temporary_Result.Add(pawn2);
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

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				List<Pawn> allMaps = AllMaps;
				List<Pawn> allCaravansAndTravelingTransportPods_Alive = AllCaravansAndTravelingTransportPods_Alive;
				if (allCaravansAndTravelingTransportPods_Alive.Count == 0)
				{
					return allMaps;
				}
				allMapsCaravansAndTravelingTransportPods_Alive_Result.Clear();
				allMapsCaravansAndTravelingTransportPods_Alive_Result.AddRange(allMaps);
				allMapsCaravansAndTravelingTransportPods_Alive_Result.AddRange(allCaravansAndTravelingTransportPods_Alive);
				return allMapsCaravansAndTravelingTransportPods_Alive_Result;
			}
		}

		public static List<Pawn> AllCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				allCaravansAndTravelingTransportPods_Alive_Result.Clear();
				List<Pawn> allCaravansAndTravelingTransportPods_AliveOrDead = AllCaravansAndTravelingTransportPods_AliveOrDead;
				for (int i = 0; i < allCaravansAndTravelingTransportPods_AliveOrDead.Count; i++)
				{
					if (!allCaravansAndTravelingTransportPods_AliveOrDead[i].Dead)
					{
						allCaravansAndTravelingTransportPods_Alive_Result.Add(allCaravansAndTravelingTransportPods_AliveOrDead[i]);
					}
				}
				return allCaravansAndTravelingTransportPods_Alive_Result;
			}
		}

		public static List<Pawn> AllCaravansAndTravelingTransportPods_AliveOrDead
		{
			get
			{
				allCaravansAndTravelingTransportPods_AliveOrDead_Result.Clear();
				if (Find.World != null)
				{
					List<Caravan> caravans = Find.WorldObjects.Caravans;
					for (int i = 0; i < caravans.Count; i++)
					{
						allCaravansAndTravelingTransportPods_AliveOrDead_Result.AddRange(caravans[i].PawnsListForReading);
					}
					List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
					for (int j = 0; j < travelingTransportPods.Count; j++)
					{
						allCaravansAndTravelingTransportPods_AliveOrDead_Result.AddRange(travelingTransportPods[j].Pawns);
					}
				}
				return allCaravansAndTravelingTransportPods_AliveOrDead_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_Colonists_Result.Clear();
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].IsColonist)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_Colonists_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_Colonists_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_Result.Clear();
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].IsFreeColonist)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Result.Clear();
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].IsFreeColonist && !allMapsCaravansAndTravelingTransportPods_Alive[i].Suspended)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_Result.Clear();
				Faction ofPlayer = Faction.OfPlayer;
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].Faction == ofPlayer)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep_Result.Clear();
				Faction ofPlayer = Faction.OfPlayer;
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].Faction == ofPlayer && !allMapsCaravansAndTravelingTransportPods_Alive[i].Suspended)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony_Result.Clear();
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive = AllMapsCaravansAndTravelingTransportPods_Alive;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive.Count; i++)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive[i].IsPrisonerOfColony)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners
		{
			get
			{
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists = AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists;
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony = AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony;
				if (allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony.Count == 0)
				{
					return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists;
				}
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result.Clear();
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists);
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result.AddRange(allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony);
				return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result;
			}
		}

		public static List<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
		{
			get
			{
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Clear();
				List<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners = AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners;
				for (int i = 0; i < allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners.Count; i++)
				{
					if (!allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners[i].Suspended)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Add(allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners[i]);
					}
				}
				return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result;
			}
		}

		public static List<Pawn> AllMaps_PrisonersOfColonySpawned
		{
			get
			{
				allMaps_PrisonersOfColonySpawned_Result.Clear();
				if (Current.ProgramState != 0)
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
				if (Current.ProgramState != 0)
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
				if (Current.ProgramState != 0)
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
				if (Current.ProgramState != 0)
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
				if (Current.ProgramState != 0)
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
				if (Current.ProgramState != 0)
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

		public static List<Pawn> AllMaps_SpawnedPawnsInFaction(Faction faction)
		{
			if (!allMaps_SpawnedPawnsInFaction_Result.TryGetValue(faction, out List<Pawn> value))
			{
				value = new List<Pawn>();
				allMaps_SpawnedPawnsInFaction_Result.Add(faction, value);
			}
			value.Clear();
			if (Current.ProgramState != 0)
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
			allMapsCaravansAndTravelingTransportPods_Alive_Result.Clear();
			allCaravansAndTravelingTransportPods_Alive_Result.Clear();
			allCaravansAndTravelingTransportPods_AliveOrDead_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_Colonists_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_Result.Clear();
			allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep_Result.Clear();
			allMaps_PrisonersOfColonySpawned_Result.Clear();
			allMaps_PrisonersOfColony_Result.Clear();
			allMaps_FreeColonists_Result.Clear();
			allMaps_FreeColonistsSpawned_Result.Clear();
			allMaps_FreeColonistsAndPrisonersSpawned_Result.Clear();
			allMaps_FreeColonistsAndPrisoners_Result.Clear();
			allMaps_SpawnedPawnsInFaction_Result.Clear();
		}
	}
}
