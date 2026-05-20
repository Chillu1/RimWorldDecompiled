using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientQuarry : TileMutatorWorker
{
	private static readonly IntRange NumQuarriedAreasRange = new IntRange(10, 15);

	private static readonly IntRange QuarrySizeRange = new IntRange(100, 500);

	public TileMutatorWorker_AncientQuarry(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostFog(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		int randomInRange = NumQuarriedAreasRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (!CellFinder.TryFindRandomCell(map, delegate(IntVec3 c)
			{
				if (c.Fogged(map))
				{
					return false;
				}
				if (c.GetEdifice(map) == null)
				{
					return false;
				}
				if (!c.GetEdifice(map).def.IsNonResourceNaturalRock)
				{
					return false;
				}
				bool flag2 = false;
				IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
				foreach (IntVec3 intVec in cardinalDirections)
				{
					IntVec3 c2 = c + intVec;
					if (c2.InBounds(map) && c2.GetEdifice(map) == null)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					return false;
				}
				int size = 0;
				map.floodFiller.FloodFill(c, (Predicate<IntVec3>)((IntVec3 x) => x.GetEdifice(map)?.def.mineable ?? false), (Action<IntVec3>)delegate
				{
					size++;
				}, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
				return (size >= QuarrySizeRange.max * 2) ? true : false;
			}, out var result))
			{
				continue;
			}
			int randomInRange2 = QuarrySizeRange.RandomInRange;
			List<IntVec3> list = GridShapeMaker.IrregularLump(result, map, randomInRange2);
			bool flag = list.All((IntVec3 c) => c.Roofed(map));
			foreach (IntVec3 item in list)
			{
				Building edifice = item.GetEdifice(map);
				if (edifice != null)
				{
					if (!edifice.def.mineable)
					{
						continue;
					}
					edifice.Destroy();
					map.fogGrid.FloodUnfogAdjacent(item, sendLetters: false);
					if (Rand.Chance(0.8f))
					{
						GenSpawn.Spawn(ThingDefOf.Filth_RubbleRock, item, map);
					}
					if (Rand.Chance(0.1f) && edifice.def.building.mineableThing != null)
					{
						Thing thing = GenSpawn.Spawn(edifice.def.building.mineableThing, item, map);
						thing.stackCount = Rand.Range(1, Mathf.Max(1, edifice.def.building.EffectiveMineableYield));
						if (thing.def.EverHaulable && !thing.def.designateHaulable)
						{
							thing.SetForbidden(value: true, warnOnFail: false);
						}
					}
				}
				if (!flag)
				{
					map.roofGrid.SetRoof(item, null);
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (IntVec3 item2 in list)
			{
				if (RoofCollapseUtility.WithinRangeOfRoofHolder(item2, map))
				{
					continue;
				}
				ThingDef rock = DeepDrillUtility.RockForTerrain(item2.GetTerrain(map));
				if (rock != null)
				{
					ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault((ThingDef def) => def.IsStuff && def.stuffProps.SourceNaturalRock == rock);
					if (thingDef == null || !thingDef.stuffProps.CanMake(ThingDefOf.Column))
					{
						thingDef = GenStuff.RandomStuffByCommonalityFor(ThingDefOf.Column);
					}
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Column, thingDef), item2, map);
				}
			}
		}
		RoofCollapseCellsFinder.CheckAndRemoveCollpsingRoofs(map);
	}
}
