using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class BuildingGroundSpawner : GroundSpawner
{
	protected Thing thingToSpawn;

	public IntRange? emergeDelay;

	public List<string> questTagsToForward;

	protected override IntRange ResultSpawnDelay => emergeDelay ?? def.building.groundSpawnerSpawnDelay;

	protected override SoundDef SustainerSound => def.building.groundSpawnerSustainerSound ?? SoundDefOf.Tunnel;

	protected virtual ThingDef ThingDefToSpawn => def.building.groundSpawnerThingToSpawn;

	public Thing ThingToSpawn => thingToSpawn;

	public override void PostMake()
	{
		base.PostMake();
		PostMakeInt();
	}

	protected virtual void PostMakeInt()
	{
		thingToSpawn = ThingMaker.MakeThing(ThingDefToSpawn);
	}

	protected override void Spawn(Map map, IntVec3 pos)
	{
		TerrainDef newTerr = map.Biome.TerrainForAffordance(ThingDefToSpawn.terrainAffordanceNeeded);
		foreach (IntVec3 item in GenAdj.OccupiedRect(pos, Rot4.North, ThingDefToSpawn.Size))
		{
			map.terrainGrid.RemoveTopLayer(item, doLeavings: false);
			if (!item.GetAffordances(map).Contains(ThingDefToSpawn.terrainAffordanceNeeded))
			{
				map.terrainGrid.SetTerrain(item, newTerr);
			}
		}
		GenSpawn.Spawn(thingToSpawn, pos, map, Rot4.North, WipeMode.FullRefund, respawningAfterLoad: false, forbidLeavings: true);
		thingToSpawn.questTags = questTagsToForward;
		BuildingProperties building = def.building;
		if (building != null && building.groundSpawnerDestroyAdjacent)
		{
			foreach (IntVec3 item2 in GenAdj.CellsAdjacentCardinal(thingToSpawn))
			{
				Building edifice = item2.GetEdifice(map);
				if (edifice != null && edifice.def.destroyable)
				{
					edifice.Destroy(DestroyMode.Refund);
				}
			}
		}
		Find.TickManager.slower.SignalForceNormalSpeedShort();
		if (def.building?.groundSpawnerLetterLabel != null && def.building?.groundSpawnerLetterText != null)
		{
			Find.LetterStack.ReceiveLetter(def.building.groundSpawnerLetterLabel, def.building.groundSpawnerLetterText, LetterDefOf.NegativeEvent, new TargetInfo(thingToSpawn));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref emergeDelay, "emergeDelay");
		Scribe_Deep.Look(ref thingToSpawn, "thingToSpawn");
		Scribe_Collections.Look(ref questTagsToForward, "questTagsToForward", LookMode.Value);
	}
}
