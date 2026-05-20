using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompSpawnImmortalSubplantsAround : ThingComp
{
	private bool disabled;

	private List<IntVec3> cells;

	private int curCheckIndex;

	public CompProperties_SpawnSubplant Props => (CompProperties_SpawnSubplant)props;

	public int CheckRespawnIntervalTicks => Props.checkRespawnIntervalHours * 2500;

	public void Disable()
	{
		disabled = true;
	}

	private void WipeAllExistingPlants(IntVec3 cell)
	{
		List<Thing> thingList = cell.GetThingList(parent.Map);
		for (int num = thingList.Count - 1; num >= 0; num--)
		{
			if (thingList[num].def.category == ThingCategory.Plant && thingList[num].def != Props.dontWipePlant)
			{
				thingList[num].Destroy();
			}
		}
	}

	private Plant SpawnPlant(IntVec3 cell)
	{
		Plant plant = (Plant)GenSpawn.Spawn(Props.subplant, cell, parent.Map);
		plant.Growth = Rand.Range(Props.minGrowthForSpawn, 1f);
		plant.Age = (plant.def.plant.LimitedLifespan ? Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 2500, 0)) : 0);
		return plant;
	}

	private void RespawnCheck()
	{
		foreach (IntVec3 item in cells.InRandomOrder())
		{
			if (item.GetPlant(parent.Map)?.def != Props.subplant && (bool)Props.subplant.CanEverPlantAt(item, parent.Map, out var _, canWipePlantsExceptTree: false, checkMapTemperature: true, writeNoReason: true))
			{
				if (!parent.Map.thingGrid.CellContains(item, ThingCategory.Item))
				{
					SpawnPlant(item);
				}
				break;
			}
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (disabled)
		{
			return;
		}
		if (respawningAfterLoad)
		{
			if (cells != null)
			{
				return;
			}
			cells = new List<IntVec3>();
			{
				foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, Props.maxRadius, useCenter: false))
				{
					if (item.InBounds(parent.Map) && item.GetPlant(parent.Map)?.def == Props.subplant)
					{
						cells.Add(item);
					}
				}
				return;
			}
		}
		if (cells != null)
		{
			return;
		}
		cells = new List<IntVec3>();
		foreach (IntVec3 item2 in GenRadial.RadialCellsAround(parent.Position, Props.maxRadius, useCenter: false).InRandomOrder())
		{
			if (!item2.InBounds(parent.Map) || !Rand.Chance(Props.chanceOverDistance.Evaluate(item2.DistanceTo(parent.Position))))
			{
				continue;
			}
			if (Props.maxPlants != -1 && cells.Count >= Props.maxPlants)
			{
				break;
			}
			Plant plant = item2.GetPlant(parent.Map);
			if (plant == null || plant.def != Props.dontWipePlant)
			{
				WipeAllExistingPlants(item2);
				if ((bool)Props.subplant.CanEverPlantAt(item2, parent.Map, out var _, canWipePlantsExceptTree: true, checkMapTemperature: true, writeNoReason: true))
				{
					cells.Add(item2);
					SpawnPlant(item2);
				}
			}
		}
	}

	public override void CompTick()
	{
		if (disabled || !parent.Spawned || cells == null)
		{
			return;
		}
		for (int i = 0; i < 100; i++)
		{
			curCheckIndex++;
			if (curCheckIndex >= cells.Count)
			{
				curCheckIndex = 0;
				break;
			}
			Plant plant = cells[curCheckIndex].GetPlant(parent.Map);
			if (plant != null && plant.Spawned && plant.def == Props.subplant)
			{
				plant.Age = 0;
			}
		}
		if (parent.IsHashIntervalTick(CheckRespawnIntervalTicks))
		{
			RespawnCheck();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Respawn " + Props.subplant.label;
			command_Action.action = RespawnCheck;
			yield return command_Action;
		}
	}

	public override void PostExposeData()
	{
		string text = (Props.saveKeysPrefix.NullOrEmpty() ? null : (Props.saveKeysPrefix + "_"));
		Scribe_Values.Look(ref disabled, text + "disabled", defaultValue: false);
		Scribe_Collections.Look(ref cells, text + "cells", LookMode.Value);
	}
}
