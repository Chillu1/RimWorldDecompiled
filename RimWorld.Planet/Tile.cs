using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class Tile : IExposable
{
	private BiomeDef biome;

	public float elevation = 100f;

	public Hilliness hilliness;

	public float temperature = 20f;

	public float rainfall;

	public float swampiness;

	public WorldFeature feature;

	public float pollution;

	public PlanetTile tile;

	public List<TileMutatorDef> mutatorsNullable;

	[Unsaved(false)]
	private bool? tmpHasSecondaryBiome;

	[Unsaved(false)]
	private BiomeDef tmpSecondaryBiome;

	private Hilliness? hillinessLabelCached;

	private float? cachedMaxTemp;

	private float? cachedMinTemp;

	public PlanetLayer Layer => tile.Layer;

	public BiomeDef PrimaryBiome
	{
		get
		{
			return biome;
		}
		set
		{
			biome = value;
		}
	}

	public IEnumerable<BiomeDef> Biomes
	{
		get
		{
			yield return biome;
			if (!tmpHasSecondaryBiome.HasValue)
			{
				TileMutatorDef tileMutatorDef = Mutators.FirstOrDefault((TileMutatorDef m) => m.Worker is TileMutatorWorker_MixedBiome);
				if (tileMutatorDef != null)
				{
					tmpHasSecondaryBiome = true;
					tmpSecondaryBiome = (tileMutatorDef.Worker as TileMutatorWorker_MixedBiome)?.SecondaryBiome(tile, Layer);
				}
				else
				{
					tmpHasSecondaryBiome = false;
					tmpSecondaryBiome = null;
				}
			}
			if (tmpHasSecondaryBiome.Value)
			{
				yield return tmpSecondaryBiome;
			}
		}
	}

	public Landmark Landmark
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return null;
			}
			return Find.World.landmarks[tile];
		}
	}

	public bool OnSurface => tile.Layer.IsRootSurface;

	public virtual bool WaterCovered => false;

	public bool IsCoastal => Find.World.CoastDirectionAt(tile) != Rot4.Invalid;

	public IList<TileMutatorDef> Mutators
	{
		get
		{
			IList<TileMutatorDef> list = mutatorsNullable;
			return list ?? Array.Empty<TileMutatorDef>();
		}
	}

	public Hilliness HillinessLabel
	{
		get
		{
			if (hillinessLabelCached.HasValue)
			{
				return hillinessLabelCached.Value;
			}
			hillinessLabelCached = hilliness;
			foreach (TileMutatorDef mutator in Mutators)
			{
				if (mutator.hillinessLabel != Hilliness.Undefined)
				{
					hillinessLabelCached = mutator.hillinessLabel;
				}
			}
			return hillinessLabelCached.Value;
		}
	}

	public Hilliness HillinessForElevationGen
	{
		get
		{
			foreach (TileMutatorDef mutator in Mutators)
			{
				if (mutator.hillinessForElevationGen != Hilliness.Undefined)
				{
					return mutator.hillinessForElevationGen;
				}
			}
			return hilliness;
		}
	}

	public Hilliness HillinessForOreGeneration
	{
		get
		{
			foreach (TileMutatorDef mutator in Mutators)
			{
				if (mutator.hillinessForOreGeneration != Hilliness.Undefined)
				{
					return mutator.hillinessForOreGeneration;
				}
			}
			return HillinessForElevationGen;
		}
	}

	public float AnimalDensity
	{
		get
		{
			float num = biome.animalDensity;
			foreach (TileMutatorDef mutator in Mutators)
			{
				num *= mutator.animalDensityFactor;
			}
			return num;
		}
	}

	public float PlantDensityFactor
	{
		get
		{
			float num = 1f;
			foreach (TileMutatorDef mutator in Mutators)
			{
				num *= mutator.plantDensityFactor;
			}
			return num;
		}
	}

	public float FishPopulationFactor
	{
		get
		{
			float num = 1f;
			foreach (TileMutatorDef mutator in Mutators)
			{
				num *= mutator.fishPopulationFactor;
			}
			return num;
		}
	}

	public bool AllowRoofedEdgeWalkIn
	{
		get
		{
			foreach (TileMutatorDef mutator in Mutators)
			{
				if (mutator.allowRoofedEdgeWalkIn)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float MaxTemperature
	{
		get
		{
			if (!cachedMaxTemp.HasValue)
			{
				cachedMaxTemp = GenTemperature.MaxTemperatureAtTile(tile);
			}
			return cachedMaxTemp.Value;
		}
	}

	public float MinTemperature
	{
		get
		{
			if (!cachedMinTemp.HasValue)
			{
				cachedMinTemp = GenTemperature.MinTemperatureAtTile(tile);
			}
			return cachedMinTemp.Value;
		}
	}

	public float MaxFishPopulation => PrimaryBiome.maxFishPopulation * FishPopulationFactor;

	public Tile()
	{
	}

	public Tile(PlanetTile tile)
	{
		this.tile = tile;
	}

	public void AddMutator(TileMutatorDef mutator)
	{
		if (mutatorsNullable == null)
		{
			mutatorsNullable = new List<TileMutatorDef>();
		}
		for (int num = mutatorsNullable.Count - 1; num >= 0; num--)
		{
			TileMutatorDef tileMutatorDef = mutatorsNullable[num];
			foreach (string category in mutator.categories)
			{
				if (tileMutatorDef.categories.Contains(category))
				{
					if (mutator.priority >= tileMutatorDef.priority)
					{
						mutatorsNullable.Remove(tileMutatorDef);
					}
					else
					{
						Log.Error("Detected mutator conflict: " + tileMutatorDef?.ToString() + " and " + mutator);
					}
				}
			}
			foreach (string overrideCategory in mutator.overrideCategories)
			{
				if (tileMutatorDef.categories.Contains(overrideCategory))
				{
					mutatorsNullable.Remove(tileMutatorDef);
				}
			}
		}
		mutatorsNullable.Add(mutator);
		mutatorsNullable.SortBy((TileMutatorDef m) => m.genOrder);
		mutator.Worker?.OnAddedToTile(tile);
	}

	public void RemoveMutator(TileMutatorDef mutator)
	{
		mutatorsNullable?.Remove(mutator);
	}

	public override string ToString()
	{
		return $"({biome} elev={elevation}m hill={hilliness} temp={temperature}°C rain={rainfall}mm" + $" swampiness={swampiness.ToStringPercent()} (allowed={biome.allowRoads}) (allowed={biome.allowRivers}))";
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Generate settlement",
			action = delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(Find.WorldSelector.SelectedTile.LayerDef.SettlementWorldObjectDef);
					mapParent.Tile = Find.WorldSelector.SelectedTile;
					mapParent.SetFaction(Faction.OfPlayer);
					Find.WorldObjects.Add(mapParent);
					GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, Find.World.info.initialMapSize, null);
				}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap, showExtraUIInfo: true, forceHideUI: false, delegate
				{
					MapParent mapParent = Find.WorldObjects.MapParentAt(Find.WorldSelector.SelectedTile);
					if (mapParent != null)
					{
						Current.Game.CurrentMap = mapParent.Map;
						CameraJumper.TryJump(mapParent.Map.Center, mapParent.Map);
					}
				});
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Generate map",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (WorldObjectDef item in DefDatabase<WorldObjectDef>.AllDefsListForReading)
				{
					WorldObjectDef local = item;
					list.Add(new FloatMenuOption(item.label, delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(local);
							mapParent.Tile = tile;
							mapParent.SetFaction(Faction.OfPlayer);
							Find.WorldObjects.Add(mapParent);
							Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, new IntVec3(100, 1, 100), null);
							Current.Game.CurrentMap = orGenerateMap;
							CameraJumper.TryJump(orGenerateMap.Center, orGenerateMap);
						}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap, showExtraUIInfo: true, forceHideUI: false, delegate
						{
							MapParent mapParent = Find.WorldObjects.MapParentAt(Find.WorldSelector.SelectedTile);
							if (mapParent != null)
							{
								Current.Game.CurrentMap = mapParent.Map;
								CameraJumper.TryJump(mapParent.Map.Center, mapParent.Map);
							}
						});
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list, "Select type"));
			}
		};
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref biome, "biome");
		Scribe_Values.Look(ref tile, "tile");
		Scribe_Values.Look(ref elevation, "elevation", 100f);
		Scribe_Values.Look(ref hilliness, "hilliness", Hilliness.Undefined);
		Scribe_Values.Look(ref temperature, "temperature", 20f);
		Scribe_Values.Look(ref rainfall, "rainfall", 0f);
		Scribe_Values.Look(ref swampiness, "swampiness", 0f);
		Scribe_Values.Look(ref pollution, "pollution", 0f);
		Scribe_Deep.Look(ref feature, "feature");
		Scribe_Collections.Look(ref mutatorsNullable, "mutatorDefs", LookMode.Def);
	}
}
