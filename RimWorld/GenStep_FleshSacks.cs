using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_FleshSacks : GenStep
{
	private int sackClumpSize = 50;

	private SimpleCurve numFleshSacksFromPoints;

	private bool forceAtLeastOneShard;

	private bool spawnSurroundingFleshmass = true;

	private bool trySpawnInRoom;

	public override int SeedPart => 1234731256;

	public override void Generate(Map map, GenStepParams parms)
	{
		float x = parms.sitePart?.site?.desiredThreatPoints ?? StorytellerUtility.DefaultThreatPointsNow(map);
		int num = Mathf.RoundToInt(numFleshSacksFromPoints?.Evaluate(x) ?? 0f);
		for (int i = 0; i < num; i++)
		{
			if ((!trySpawnInRoom || !CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map, mustBeInRoom: true), out var result)) && !CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map, mustBeInRoom: false), out result))
			{
				continue;
			}
			foreach (IntVec3 item in GridShapeMaker.IrregularLump(result, map, sackClumpSize))
			{
				if (spawnSurroundingFleshmass)
				{
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Fleshmass), item, map, Rot4.Random).SetFaction(Faction.OfEntities);
				}
				map.terrainGrid.SetTerrain(item, TerrainDefOf.Flesh);
			}
			List<Thing> list;
			if (forceAtLeastOneShard && i == 0)
			{
				list = Gen.YieldSingle(ThingMaker.MakeThing(ThingDefOf.Shard)).ToList();
			}
			else
			{
				ThingSetMakerParams parms2 = new ThingSetMakerParams
				{
					qualityGenerator = QualityGenerator.Reward,
					makingFaction = Faction.OfEntities
				};
				list = ThingSetMakerDefOf.MapGen_FleshSackLoot.root.Generate(parms2);
			}
			Building_Casket building_Casket = ThingMaker.MakeThing(ThingDefOf.FleshSack) as Building_Casket;
			GenSpawn.Spawn(building_Casket, result, map);
			building_Casket.SetFaction(Faction.OfEntities);
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Thing thing = list[num2];
				if (!building_Casket.TryAcceptThing(thing, allowSpecialEffects: false))
				{
					thing.Destroy();
				}
			}
		}
	}

	private bool Validator(IntVec3 c, Map map, bool mustBeInRoom)
	{
		if (!GenSpawn.CanSpawnAt(ThingDefOf.FleshSack, c, map))
		{
			return false;
		}
		if (c.DistanceToEdge(map) <= 2)
		{
			return false;
		}
		if ((mustBeInRoom && c.GetRoom(map) == null) || !c.GetRoom(map).ProperRoom)
		{
			return false;
		}
		if (!map.generatorDef.isUnderground && !map.reachability.CanReachMapEdge(c, TraverseMode.PassDoors))
		{
			return false;
		}
		return true;
	}
}
