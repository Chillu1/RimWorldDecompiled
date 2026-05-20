using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GenLeaving
{
	private const float LeaveFraction_Kill = 0.25f;

	private const float LeaveFraction_Cancel = 1f;

	public const float LeaveFraction_DeconstructDefault = 0.5f;

	private const float LeaveFraction_FailConstruction = 0.5f;

	private static List<Thing> tmpKilledLeavings = new List<Thing>();

	private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

	public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, List<Thing> listOfLeavingsOut = null)
	{
		DoLeavingsFor(diedThing, map, mode, diedThing.OccupiedRect().ExpandedBy(diedThing.def.killedLeavingsExpandRect), null, listOfLeavingsOut);
	}

	public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, CellRect leavingsRect, Predicate<IntVec3> nearPlaceValidator = null, List<Thing> listOfLeavingsOut = null)
	{
		if (Current.ProgramState != ProgramState.Playing && mode != DestroyMode.Refund)
		{
			return;
		}
		int num;
		switch (mode)
		{
		case DestroyMode.Vanish:
		case DestroyMode.QuestLogic:
			return;
		default:
			num = ((mode == DestroyMode.KillFinalizeLeavingsOnly) ? 1 : 0);
			break;
		case DestroyMode.KillFinalize:
			num = 1;
			break;
		}
		bool flag = (byte)num != 0;
		if (flag && diedThing.def.filthLeaving != null)
		{
			for (int i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
			{
				for (int j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
				{
					FilthMaker.TryMakeFilth(new IntVec3(j, 0, i), map, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
				}
			}
		}
		if (flag && diedThing.def.race != null && !diedThing.def.race.detritusLeavings.NullOrEmpty())
		{
			DetritusLeavingType detritusLeavingType = diedThing.def.race.detritusLeavings.RandomElement();
			if (Rand.Chance(detritusLeavingType.spawnChance))
			{
				CellFinder.TryFindRandomCellNear(diedThing.Position, map, 1, (IntVec3 c) => c.Standable(map), out var result);
				GenSpawn.Spawn(ThingMaker.MakeThing(detritusLeavingType.def), result, map).overrideGraphicIndex = detritusLeavingType.texOverrideIndex;
			}
		}
		ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
		if (flag)
		{
			List<ThingDefCountClass> list = new List<ThingDefCountClass>();
			Rand.PushState(diedThing.thingIDNumber);
			if (!(diedThing is Pawn { IsShambler: not false }) && Rand.Chance(diedThing.def.killedLeavingsChance))
			{
				if (diedThing.def.killedLeavings != null)
				{
					list.AddRange(diedThing.def.killedLeavings);
				}
				if (diedThing.HostileTo(Faction.OfPlayer) && !diedThing.def.killedLeavingsPlayerHostile.NullOrEmpty())
				{
					list.AddRange(diedThing.def.killedLeavingsPlayerHostile);
				}
				if (diedThing.def.killedLeavingsRanges != null)
				{
					foreach (ThingDefCountRangeClass killedLeavingsRange in diedThing.def.killedLeavingsRanges)
					{
						int num2 = Mathf.RoundToInt(killedLeavingsRange.countRange.RandomInRange);
						if (num2 > 0)
						{
							list.Add(new ThingDefCountClass(killedLeavingsRange.thingDef, num2));
						}
					}
				}
			}
			if (diedThing is ThingWithComps thingWithComps)
			{
				list.AddRange(thingWithComps.GetAdditionalLeavings(mode));
			}
			if (ModsConfig.AnomalyActive && diedThing is Pawn { IsMutant: not false } pawn2 && !pawn2.mutant.Def.killedLeavings.NullOrEmpty())
			{
				list.AddRange(pawn2.mutant.Def.killedLeavings);
			}
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				ThingDefCountClass thingDefCountClass = list[num3];
				if (!thingDefCountClass.IsChanceBased || Rand.Chance(thingDefCountClass.DropChance))
				{
					Thing thing = ThingMaker.MakeThing(list[num3].thingDef);
					thing.stackCount = list[num3].count;
					thingOwner.TryAdd(thing);
				}
			}
			Rand.PopState();
		}
		if (CanBuildingLeaveResources(diedThing, mode) && mode != DestroyMode.KillFinalizeLeavingsOnly)
		{
			if (diedThing is Frame frame)
			{
				for (int num4 = frame.resourceContainer.Count - 1; num4 >= 0; num4--)
				{
					int num5 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(frame.resourceContainer[num4].stackCount);
					if (num5 > 0)
					{
						frame.resourceContainer.TryTransferToContainer(frame.resourceContainer[num4], thingOwner, num5);
					}
				}
				frame.resourceContainer.ClearAndDestroyContents();
			}
			else
			{
				List<ThingDefCountClass> list2 = diedThing.CostListAdjusted();
				for (int num6 = 0; num6 < list2.Count; num6++)
				{
					ThingDefCountClass thingDefCountClass2 = list2[num6];
					List<ThingDef> forcedCostLeavings = diedThing.def.building.forcedCostLeavings;
					if (forcedCostLeavings != null && forcedCostLeavings.Contains(thingDefCountClass2.thingDef))
					{
						Thing thing2 = ThingMaker.MakeThing(thingDefCountClass2.thingDef);
						thing2.stackCount = thingDefCountClass2.count;
						thingOwner.TryAdd(thing2);
						continue;
					}
					if (thingDefCountClass2.thingDef == ThingDefOf.ReinforcedBarrel && !Find.Storyteller.difficulty.classicMortars)
					{
						CompRefuelable compRefuelable = diedThing.TryGetComp<CompRefuelable>();
						if (compRefuelable != null && compRefuelable.Props.fuelIsMortarBarrel && compRefuelable.FuelPercentOfMax < 0.5f)
						{
							continue;
						}
					}
					if (diedThing.def.building?.leavingsBlacklist != null && diedThing.def.building.leavingsBlacklist.Contains(thingDefCountClass2.thingDef))
					{
						continue;
					}
					int num7 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(thingDefCountClass2.count);
					if (num7 > 0 && mode == DestroyMode.KillFinalize && thingDefCountClass2.thingDef.slagDef != null)
					{
						int count = thingDefCountClass2.thingDef.slagDef.smeltProducts.First((ThingDefCountClass pro) => pro.thingDef == ThingDefOf.Steel).count;
						int a = num7 / count;
						a = Mathf.Min(a, diedThing.def.Size.Area / 2);
						for (int num8 = 0; num8 < a; num8++)
						{
							thingOwner.TryAdd(ThingMaker.MakeThing(thingDefCountClass2.thingDef.slagDef));
						}
						num7 -= a * count;
					}
					if (num7 > 0)
					{
						Thing thing3 = ThingMaker.MakeThing(thingDefCountClass2.thingDef);
						thing3.stackCount = num7;
						thingOwner.TryAdd(thing3);
					}
				}
			}
		}
		tmpKilledLeavings.Clear();
		List<IntVec3> list3 = leavingsRect.Cells.InRandomOrder().ToList();
		int num9 = 0;
		while (thingOwner.Count > 0)
		{
			if (mode == DestroyMode.KillFinalize && !map.areaManager.Home[list3[num9]] && !diedThing.def.forceLeavingsAllowed)
			{
				thingOwner[0].SetForbidden(value: true, warnOnFail: false);
			}
			if (!thingOwner.TryDrop(thingOwner[0], list3[num9], map, ThingPlaceMode.Near, out var lastResultingThing, null, nearPlaceValidator))
			{
				Log.Warning("Failed to place all leavings for destroyed thing " + diedThing?.ToString() + " at " + leavingsRect.CenterCell.ToString());
				break;
			}
			tmpKilledLeavings.Add(lastResultingThing);
			num9++;
			if (num9 >= list3.Count)
			{
				num9 = 0;
			}
		}
		listOfLeavingsOut?.AddRange(tmpKilledLeavings);
		if (mode == DestroyMode.KillFinalize && tmpKilledLeavings.Count > 0)
		{
			QuestUtility.SendQuestTargetSignals(diedThing.questTags, "KilledLeavingsLeft", diedThing.Named("DROPPER"), tmpKilledLeavings.Named("SUBJECT"));
		}
		tmpKilledLeavings.Clear();
	}

	public static void DropThingsNear(Thing thing, List<ThingDefCountClass> things, List<Thing> spawnedThings = null, bool forbid = true, Predicate<IntVec3> nearPlaceValidator = null)
	{
		if (things.NullOrEmpty())
		{
			return;
		}
		Rand.PushState(thing.thingIDNumber);
		ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
		foreach (ThingDefCountClass thing3 in things)
		{
			if (!thing3.IsChanceBased || Rand.Chance(thing3.DropChance))
			{
				Thing thing2 = ThingMaker.MakeThing(thing3.thingDef);
				thing2.stackCount = thing3.count;
				thingOwner.TryAdd(thing2);
			}
		}
		Rand.PopState();
		DropThingOwnerContents(thingOwner, thing.Map, thing.OccupiedRect().ExpandedBy(1), spawnedThings, forbid, nearPlaceValidator);
	}

	public static void DropThingOwnerContents(ThingOwner owner, Map map, CellRect leavingsRect, List<Thing> spawnedThings = null, bool forbid = true, Predicate<IntVec3> nearPlaceValidator = null)
	{
		List<IntVec3> list = leavingsRect.Cells.InRandomOrder().ToList();
		int num = 0;
		while (owner.Count > 0)
		{
			if (forbid)
			{
				owner[0].SetForbidden(value: true, warnOnFail: false);
			}
			if (!owner.TryDrop(owner[0], list[num], map, ThingPlaceMode.Near, out var lastResultingThing, null, nearPlaceValidator))
			{
				Log.Warning($"Failed to place all leavings for destroyed thing {owner} at {leavingsRect.CenterCell}");
				break;
			}
			spawnedThings?.Add(lastResultingThing);
			num++;
			if (num >= list.Count)
			{
				num = 0;
			}
		}
	}

	public static void DoLeavingsFor(TerrainDef terrain, IntVec3 cell, Map map)
	{
		if (Current.ProgramState != ProgramState.Playing)
		{
			return;
		}
		ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
		List<ThingDefCountClass> list = terrain.CostListAdjusted(null);
		for (int i = 0; i < list.Count; i++)
		{
			ThingDefCountClass thingDefCountClass = list[i];
			int num = GenMath.RoundRandom((float)thingDefCountClass.count * terrain.resourcesFractionWhenDeconstructed);
			if (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
				thing.stackCount = num;
				thingOwner.TryAdd(thing);
			}
		}
		while (thingOwner.Count > 0)
		{
			if (!thingOwner.TryDrop(thingOwner[0], cell, map, ThingPlaceMode.Near, out var _))
			{
				string obj = terrain?.ToString();
				IntVec3 intVec = cell;
				Log.Warning("Failed to place all leavings for removed terrain " + obj + " at " + intVec.ToString());
				break;
			}
		}
	}

	public static bool CanBuildingLeaveResources(Thing destroyedThing, DestroyMode mode)
	{
		if (!(destroyedThing is Building))
		{
			return false;
		}
		if (mode == DestroyMode.Deconstruct && destroyedThing is Frame)
		{
			mode = DestroyMode.Cancel;
		}
		return mode switch
		{
			DestroyMode.Vanish => false, 
			DestroyMode.WillReplace => false, 
			DestroyMode.KillFinalize => destroyedThing.def.leaveResourcesWhenKilled, 
			DestroyMode.Deconstruct => destroyedThing.def.resourcesFractionWhenDeconstructed != 0f, 
			DestroyMode.Cancel => true, 
			DestroyMode.FailConstruction => true, 
			DestroyMode.Refund => true, 
			DestroyMode.QuestLogic => false, 
			DestroyMode.KillFinalizeLeavingsOnly => false, 
			_ => throw new ArgumentException("Unknown destroy mode " + mode), 
		};
	}

	private static Func<int, int> GetBuildingResourcesLeaveCalculator(Thing destroyedThing, DestroyMode mode)
	{
		if (!CanBuildingLeaveResources(destroyedThing, mode))
		{
			return (int count) => 0;
		}
		if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
		{
			mode = DestroyMode.Cancel;
		}
		return mode switch
		{
			DestroyMode.Vanish => (int count) => 0, 
			DestroyMode.WillReplace => (int count) => 0, 
			DestroyMode.KillFinalize => (int count) => GenMath.RoundRandom((float)count * 0.25f), 
			DestroyMode.Deconstruct => (int count) => Mathf.Min(GenMath.RoundRandom((float)count * destroyedThing.def.resourcesFractionWhenDeconstructed), count), 
			DestroyMode.Cancel => (int count) => GenMath.RoundRandom((float)count * 1f), 
			DestroyMode.FailConstruction => (int count) => Mathf.Max(GenMath.RoundRandom((float)count * 0.5f), 1), 
			DestroyMode.Refund => (int count) => count, 
			DestroyMode.QuestLogic => (int count) => 0, 
			DestroyMode.KillFinalizeLeavingsOnly => (int count) => 0, 
			_ => throw new ArgumentException("Unknown destroy mode " + mode), 
		};
	}

	public static void DropFilthDueToDamage(Thing t, float damageDealt)
	{
		if (!t.def.useHitPoints || !t.Spawned || t.def.filthLeaving == null)
		{
			return;
		}
		CellRect cellRect = t.OccupiedRect().ExpandedBy(1);
		tmpCellsCandidates.Clear();
		foreach (IntVec3 item in cellRect)
		{
			if (item.InBounds(t.Map) && item.Walkable(t.Map))
			{
				tmpCellsCandidates.Add(item);
			}
		}
		if (tmpCellsCandidates.Any())
		{
			int num = GenMath.RoundRandom(damageDealt * Mathf.Min(1f / 60f, 1f / ((float)t.MaxHitPoints / 10f)));
			for (int i = 0; i < num; i++)
			{
				FilthMaker.TryMakeFilth(tmpCellsCandidates.RandomElement(), t.Map, t.def.filthLeaving);
			}
			tmpCellsCandidates.Clear();
		}
	}
}
