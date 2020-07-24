using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public sealed class World : IThingHolder, IExposable, IIncidentTarget, ILoadReferenceable
	{
		public WorldInfo info = new WorldInfo();

		public List<WorldComponent> components = new List<WorldComponent>();

		public FactionManager factionManager;

		public WorldPawns worldPawns;

		public WorldObjectsHolder worldObjects;

		public GameConditionManager gameConditionManager;

		public StoryState storyState;

		public WorldFeatures features;

		public WorldGrid grid;

		public WorldPathGrid pathGrid;

		public WorldRenderer renderer;

		public WorldInterface UI;

		public WorldDebugDrawer debugDrawer;

		public WorldDynamicDrawManager dynamicDrawManager;

		public WorldPathFinder pathFinder;

		public WorldPathPool pathPool;

		public WorldReachability reachability;

		public WorldFloodFiller floodFiller;

		public ConfiguredTicksAbsAtGameStartCache ticksAbsCache;

		public TileTemperaturesComp tileTemperatures;

		public WorldGenData genData;

		private List<ThingDef> allNaturalRockDefs;

		private static List<ThingDef> tmpNaturalRockDefs = new List<ThingDef>();

		private static List<int> tmpNeighbors = new List<int>();

		private static List<Rot4> tmpOceanDirs = new List<Rot4>();

		public float PlanetCoverage => info.planetCoverage;

		public IThingHolder ParentHolder => null;

		public int Tile => -1;

		public StoryState StoryState => storyState;

		public GameConditionManager GameConditionManager => gameConditionManager;

		public float PlayerWealthForStoryteller
		{
			get
			{
				float num = 0f;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					num += maps[i].PlayerWealthForStoryteller;
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					num += caravans[j].PlayerWealthForStoryteller;
				}
				return num;
			}
		}

		public IEnumerable<Pawn> PlayerPawnsForStoryteller => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;

		public FloatRange IncidentPointsRandomFactorRange => FloatRange.One;

		public int ConstantRandSeed => info.persistentRandomValue;

		public IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			yield return IncidentTargetTagDefOf.World;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref info, "info");
			Scribe_Deep.Look(ref grid, "grid");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (grid == null || !grid.HasWorldData)
				{
					WorldGenerator.GenerateWithoutWorldData(info.seedString);
				}
				else
				{
					WorldGenerator.GenerateFromScribe(info.seedString);
				}
			}
			else
			{
				ExposeComponents();
			}
		}

		public void ExposeComponents()
		{
			Scribe_Deep.Look(ref factionManager, "factionManager");
			Scribe_Deep.Look(ref worldPawns, "worldPawns");
			Scribe_Deep.Look(ref worldObjects, "worldObjects");
			Scribe_Deep.Look(ref gameConditionManager, "gameConditionManager", this);
			Scribe_Deep.Look(ref storyState, "storyState", this);
			Scribe_Deep.Look(ref features, "features");
			Scribe_Collections.Look(ref components, "components", LookMode.Deep, this);
			FillComponents();
			BackCompatibility.PostExposeData(this);
		}

		public void ConstructComponents()
		{
			worldObjects = new WorldObjectsHolder();
			factionManager = new FactionManager();
			worldPawns = new WorldPawns();
			gameConditionManager = new GameConditionManager(this);
			storyState = new StoryState(this);
			renderer = new WorldRenderer();
			UI = new WorldInterface();
			debugDrawer = new WorldDebugDrawer();
			dynamicDrawManager = new WorldDynamicDrawManager();
			pathFinder = new WorldPathFinder();
			pathPool = new WorldPathPool();
			reachability = new WorldReachability();
			floodFiller = new WorldFloodFiller();
			ticksAbsCache = new ConfiguredTicksAbsAtGameStartCache();
			components.Clear();
			FillComponents();
		}

		private void FillComponents()
		{
			components.RemoveAll((WorldComponent component) => component == null);
			foreach (Type item2 in typeof(WorldComponent).AllSubclassesNonAbstract())
			{
				if (GetComponent(item2) == null)
				{
					try
					{
						WorldComponent item = (WorldComponent)Activator.CreateInstance(item2, this);
						components.Add(item);
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat("Could not instantiate a WorldComponent of type ", item2, ": ", ex));
					}
				}
			}
			tileTemperatures = GetComponent<TileTemperaturesComp>();
			genData = GetComponent<WorldGenData>();
		}

		public void FinalizeInit()
		{
			pathGrid.RecalculateAllPerceivedPathCosts();
			AmbientSoundManager.EnsureWorldAmbientSoundCreated();
			WorldComponentUtility.FinalizeInit(this);
		}

		public void WorldTick()
		{
			worldPawns.WorldPawnsTick();
			factionManager.FactionManagerTick();
			worldObjects.WorldObjectsHolderTick();
			debugDrawer.WorldDebugDrawerTick();
			pathGrid.WorldPathGridTick();
			WorldComponentUtility.WorldComponentTick(this);
		}

		public void WorldPostTick()
		{
			try
			{
				gameConditionManager.GameConditionManagerTick();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public void WorldUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			renderer.CheckActivateWorldCamera();
			if (worldRenderedNow)
			{
				ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
				renderer.DrawWorldLayers();
				dynamicDrawManager.DrawDynamicWorldObjects();
				features.UpdateFeatures();
				NoiseDebugUI.RenderPlanetNoise();
			}
			WorldComponentUtility.WorldComponentUpdate(this);
		}

		public T GetComponent<T>() where T : WorldComponent
		{
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		public WorldComponent GetComponent(Type type)
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (type.IsAssignableFrom(components[i].GetType()))
				{
					return components[i];
				}
			}
			return null;
		}

		public Rot4 CoastDirectionAt(int tileID)
		{
			if (!grid[tileID].biome.canBuildBase)
			{
				return Rot4.Invalid;
			}
			tmpOceanDirs.Clear();
			grid.GetTileNeighbors(tileID, tmpNeighbors);
			int i = 0;
			for (int count = tmpNeighbors.Count; i < count; i++)
			{
				if (grid[tmpNeighbors[i]].biome == BiomeDefOf.Ocean)
				{
					Rot4 rotFromTo = grid.GetRotFromTo(tileID, tmpNeighbors[i]);
					if (!tmpOceanDirs.Contains(rotFromTo))
					{
						tmpOceanDirs.Add(rotFromTo);
					}
				}
			}
			if (tmpOceanDirs.Count == 0)
			{
				return Rot4.Invalid;
			}
			Rand.PushState();
			Rand.Seed = tileID;
			int index = Rand.Range(0, tmpOceanDirs.Count);
			Rand.PopState();
			return tmpOceanDirs[index];
		}

		public bool HasCaves(int tile)
		{
			Tile tile2 = grid[tile];
			float chance;
			if ((int)tile2.hilliness >= 4)
			{
				chance = 0.5f;
			}
			else
			{
				if ((int)tile2.hilliness < 3)
				{
					return false;
				}
				chance = 0.25f;
			}
			return Rand.ChanceSeeded(chance, Gen.HashCombineInt(Find.World.info.Seed, tile));
		}

		public IEnumerable<ThingDef> NaturalRockTypesIn(int tile)
		{
			Rand.PushState();
			Rand.Seed = tile;
			if (allNaturalRockDefs == null)
			{
				allNaturalRockDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsNonResourceNaturalRock).ToList();
			}
			int num = Rand.RangeInclusive(2, 3);
			if (num > allNaturalRockDefs.Count)
			{
				num = allNaturalRockDefs.Count;
			}
			tmpNaturalRockDefs.Clear();
			tmpNaturalRockDefs.AddRange(allNaturalRockDefs);
			List<ThingDef> list = new List<ThingDef>();
			for (int i = 0; i < num; i++)
			{
				ThingDef item = tmpNaturalRockDefs.RandomElement();
				tmpNaturalRockDefs.Remove(item);
				list.Add(item);
			}
			Rand.PopState();
			return list;
		}

		public bool Impassable(int tileID)
		{
			return !pathGrid.Passable(tileID);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			List<WorldObject> allWorldObjects = worldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				IThingHolder thingHolder = allWorldObjects[i] as IThingHolder;
				if (thingHolder != null)
				{
					outChildren.Add(thingHolder);
				}
				List<WorldObjectComp> allComps = allWorldObjects[i].AllComps;
				for (int j = 0; j < allComps.Count; j++)
				{
					IThingHolder thingHolder2 = allComps[j] as IThingHolder;
					if (thingHolder2 != null)
					{
						outChildren.Add(thingHolder2);
					}
				}
			}
			for (int k = 0; k < components.Count; k++)
			{
				IThingHolder thingHolder3 = components[k] as IThingHolder;
				if (thingHolder3 != null)
				{
					outChildren.Add(thingHolder3);
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return "World";
		}

		public override string ToString()
		{
			return "World-" + info.name;
		}
	}
}
