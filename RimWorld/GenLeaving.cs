using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLeaving
	{
		private const float LeaveFraction_Kill = 0.5f;

		private const float LeaveFraction_Cancel = 1f;

		public const float LeaveFraction_DeconstructDefault = 0.75f;

		private const float LeaveFraction_FailConstruction = 0.5f;

		private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, List<Thing> listOfLeavingsOut = null)
		{
			DoLeavingsFor(diedThing, map, mode, diedThing.OccupiedRect(), null, listOfLeavingsOut);
		}

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, CellRect leavingsRect, Predicate<IntVec3> nearPlaceValidator = null, List<Thing> listOfLeavingsOut = null)
		{
			if (Current.ProgramState != ProgramState.Playing && mode != DestroyMode.Refund)
			{
				return;
			}
			switch (mode)
			{
			case DestroyMode.Vanish:
			case DestroyMode.QuestLogic:
				return;
			case DestroyMode.KillFinalize:
			{
				if (diedThing.def.filthLeaving == null)
				{
					break;
				}
				for (int i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
				{
					for (int j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
					{
						FilthMaker.TryMakeFilth(new IntVec3(j, 0, i), map, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
					}
				}
				break;
			}
			}
			ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
			if (mode == DestroyMode.KillFinalize && diedThing.def.killedLeavings != null)
			{
				for (int k = 0; k < diedThing.def.killedLeavings.Count; k++)
				{
					Thing thing = ThingMaker.MakeThing(diedThing.def.killedLeavings[k].thingDef);
					thing.stackCount = diedThing.def.killedLeavings[k].count;
					thingOwner.TryAdd(thing);
				}
			}
			if (CanBuildingLeaveResources(diedThing, mode))
			{
				Frame frame = diedThing as Frame;
				if (frame != null)
				{
					for (int num = frame.resourceContainer.Count - 1; num >= 0; num--)
					{
						int num2 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(frame.resourceContainer[num].stackCount);
						if (num2 > 0)
						{
							frame.resourceContainer.TryTransferToContainer(frame.resourceContainer[num], thingOwner, num2);
						}
					}
					frame.resourceContainer.ClearAndDestroyContents();
				}
				else
				{
					List<ThingDefCountClass> list = diedThing.CostListAdjusted();
					for (int l = 0; l < list.Count; l++)
					{
						ThingDefCountClass thingDefCountClass = list[l];
						int num3 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(thingDefCountClass.count);
						if (num3 > 0 && mode == DestroyMode.KillFinalize && thingDefCountClass.thingDef.slagDef != null)
						{
							int count = thingDefCountClass.thingDef.slagDef.smeltProducts.First((ThingDefCountClass pro) => pro.thingDef == ThingDefOf.Steel).count;
							int a = num3 / count;
							a = Mathf.Min(a, diedThing.def.Size.Area / 2);
							for (int m = 0; m < a; m++)
							{
								thingOwner.TryAdd(ThingMaker.MakeThing(thingDefCountClass.thingDef.slagDef));
							}
							num3 -= a * count;
						}
						if (num3 > 0)
						{
							Thing thing2 = ThingMaker.MakeThing(thingDefCountClass.thingDef);
							thing2.stackCount = num3;
							thingOwner.TryAdd(thing2);
						}
					}
				}
			}
			List<IntVec3> list2 = leavingsRect.Cells.InRandomOrder().ToList();
			int num4 = 0;
			while (thingOwner.Count > 0)
			{
				if (mode == DestroyMode.KillFinalize && !map.areaManager.Home[list2[num4]])
				{
					thingOwner[0].SetForbidden(value: true, warnOnFail: false);
				}
				if (!thingOwner.TryDrop(thingOwner[0], list2[num4], map, ThingPlaceMode.Near, out var lastResultingThing, null, nearPlaceValidator))
				{
					Log.Warning(string.Concat("Failed to place all leavings for destroyed thing ", diedThing, " at ", leavingsRect.CenterCell));
					break;
				}
				listOfLeavingsOut?.Add(lastResultingThing);
				num4++;
				if (num4 >= list2.Count)
				{
					num4 = 0;
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
					Log.Warning(string.Concat("Failed to place all leavings for removed terrain ", terrain, " at ", cell));
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
			if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
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
				DestroyMode.KillFinalize => (int count) => GenMath.RoundRandom((float)count * 0.5f), 
				DestroyMode.Deconstruct => (int count) => Mathf.Min(GenMath.RoundRandom((float)count * destroyedThing.def.resourcesFractionWhenDeconstructed), count), 
				DestroyMode.Cancel => (int count) => GenMath.RoundRandom((float)count * 1f), 
				DestroyMode.FailConstruction => (int count) => GenMath.RoundRandom((float)count * 0.5f), 
				DestroyMode.Refund => (int count) => count, 
				DestroyMode.QuestLogic => (int count) => 0, 
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
				int num = GenMath.RoundRandom(damageDealt * Mathf.Min(0.0166666675f, 1f / ((float)t.MaxHitPoints / 10f)));
				for (int i = 0; i < num; i++)
				{
					FilthMaker.TryMakeFilth(tmpCellsCandidates.RandomElement(), t.Map, t.def.filthLeaving);
				}
				tmpCellsCandidates.Clear();
			}
		}
	}
}
