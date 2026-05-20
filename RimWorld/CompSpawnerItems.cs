using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompSpawnerItems : ThingComp
{
	private int ticksPassed;

	public CompProperties_SpawnerItems Props => (CompProperties_SpawnerItems)props;

	public bool Active => parent.GetComp<CompCanBeDormant>()?.Awake ?? true;

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "DEV: Spawn items";
		command_Action.action = delegate
		{
			SpawnItems();
		};
		yield return command_Action;
	}

	private void SpawnItems()
	{
		if (Props.MatchingItems.TryRandomElement(out var result))
		{
			int stackCount = Mathf.CeilToInt(Props.approxMarketValuePerDay / result.BaseMarketValue);
			Thing thing = ThingMaker.MakeThing(result);
			thing.stackCount = stackCount;
			GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
		}
	}

	public override void CompTickRare()
	{
		if (Active)
		{
			ticksPassed += 250;
			if (ticksPassed >= Props.spawnInterval)
			{
				SpawnItems();
				ticksPassed -= Props.spawnInterval;
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0);
	}

	public override string CompInspectStringExtra()
	{
		if (Active)
		{
			return "NextSpawnedResourceIn".Translate() + ": " + (Props.spawnInterval - ticksPassed).ToStringTicksToPeriod();
		}
		return null;
	}
}
