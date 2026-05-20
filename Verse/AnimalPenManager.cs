using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class AnimalPenManager
{
	private readonly Map map;

	private readonly Dictionary<CompAnimalPenMarker, PenMarkerState> penMarkers = new Dictionary<CompAnimalPenMarker, PenMarkerState>();

	private bool dirty = true;

	private ThingFilter cachedDefaultAutoCutFilter;

	private ThingFilter cachedFixedAutoCutFilter;

	private HashSet<string> existingPenNames = new HashSet<string>();

	private readonly PenFoodCalculator cached_placingPenFoodCalculator = new PenFoodCalculator();

	private IntVec3? cached_placingPenFoodCalculator_forPosition;

	public AnimalPenManager(Map map)
	{
		this.map = map;
		map.events.BuildingSpawned += Notify_BuildingSpawned;
		map.events.BuildingDespawned += Notify_BuildingDespawned;
	}

	public void RebuildAllPens()
	{
		ForceRebuildPens();
	}

	public PenMarkerState GetPenMarkerState(CompAnimalPenMarker marker)
	{
		RebuildIfDirty();
		return penMarkers[marker];
	}

	public CompAnimalPenMarker GetPenNamed(string name)
	{
		return penMarkers.Keys.FirstOrDefault((CompAnimalPenMarker marker) => marker.label == name);
	}

	public ThingFilter GetFixedAutoCutFilter()
	{
		if (cachedFixedAutoCutFilter == null)
		{
			cachedFixedAutoCutFilter = new ThingFilter();
			foreach (ThingDef allWildPlant in map.wildPlantSpawner.AllWildPlants)
			{
				if (allWildPlant.plant.allowAutoCut)
				{
					cachedFixedAutoCutFilter.SetAllow(allWildPlant, allow: true);
				}
			}
			cachedFixedAutoCutFilter.SetAllow(ThingCategoryDefOf.Stumps, allow: true);
		}
		return cachedFixedAutoCutFilter;
	}

	public ThingFilter GetDefaultAutoCutFilter()
	{
		if (cachedDefaultAutoCutFilter == null)
		{
			cachedDefaultAutoCutFilter = new ThingFilter();
			ThingDef plant_Grass = ThingDefOf.Plant_Grass;
			float num = plant_Grass.ingestible.CachedNutrition / plant_Grass.plant.growDays * 0.5f - 0.0001f;
			foreach (ThingDef allowedThingDef in GetFixedAutoCutFilter().AllowedThingDefs)
			{
				if (!MapPlantGrowthRateCalculator.IsEdibleByPastureAnimals(allowedThingDef) || (!(allowedThingDef.ingestible.CachedNutrition / allowedThingDef.plant.growDays >= num) && (allowedThingDef.plant.harvestedThingDef == null || !allowedThingDef.plant.harvestedThingDef.IsIngestible)))
				{
					cachedDefaultAutoCutFilter.SetAllow(allowedThingDef, allow: true);
				}
			}
		}
		return cachedDefaultAutoCutFilter;
	}

	private void Notify_BuildingSpawned(Building building)
	{
		dirty = true;
	}

	private void Notify_BuildingDespawned(Building building)
	{
		dirty = true;
	}

	private void RebuildIfDirty()
	{
		if (dirty)
		{
			ForceRebuildPens();
		}
	}

	private void ForceRebuildPens()
	{
		dirty = false;
		penMarkers.Clear();
		foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
		{
			CompAnimalPenMarker compAnimalPenMarker = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
			penMarkers.Add(compAnimalPenMarker, new PenMarkerState(compAnimalPenMarker));
		}
	}

	public string MakeNewAnimalPenName()
	{
		existingPenNames.Clear();
		existingPenNames.AddRange(penMarkers.Keys.Select((CompAnimalPenMarker marker) => marker.label));
		int num = 1;
		string text;
		while (true)
		{
			text = "AnimalPenMarkerDefaultLabel".Translate(num);
			if (!existingPenNames.Contains(text))
			{
				break;
			}
			num++;
		}
		existingPenNames.Clear();
		return text;
	}

	public void DrawPlacingMouseAttachments(BuildableDef def)
	{
		if ((def as ThingDef)?.CompDefFor<CompAnimalPenMarker>() == null)
		{
			return;
		}
		IntVec3 intVec = UI.MouseCell();
		if (!intVec.InBounds(map))
		{
			return;
		}
		if (cached_placingPenFoodCalculator_forPosition.HasValue)
		{
			IntVec3 value = intVec;
			IntVec3? intVec2 = cached_placingPenFoodCalculator_forPosition;
			if (!(value != intVec2))
			{
				goto IL_007a;
			}
		}
		cached_placingPenFoodCalculator.ResetAndProcessPen(intVec, map, considerBlueprints: true);
		cached_placingPenFoodCalculator_forPosition = intVec;
		goto IL_007a;
		IL_007a:
		AnimalPenGUI.DrawPlacingMouseAttachments(intVec, map, cached_placingPenFoodCalculator);
	}
}
