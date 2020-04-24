using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class CollectionsMassCalculator
	{
		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<Thing> thingsInReverse = new List<Thing>();

		public static float Capacity(List<ThingCount> thingCounts, StringBuilder explanation = null)
		{
			float num = 0f;
			for (int i = 0; i < thingCounts.Count; i++)
			{
				if (thingCounts[i].Count > 0)
				{
					Pawn pawn = thingCounts[i].Thing as Pawn;
					if (pawn != null)
					{
						num += MassUtility.Capacity(pawn, explanation) * (float)thingCounts[i].Count;
					}
				}
			}
			return Mathf.Max(num, 0f);
		}

		public static float MassUsage(List<ThingCount> thingCounts, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreSpawnedCorpsesGearAndInventory = false)
		{
			float num = 0f;
			for (int i = 0; i < thingCounts.Count; i++)
			{
				int count = thingCounts[i].Count;
				if (count <= 0)
				{
					continue;
				}
				Thing thing = thingCounts[i].Thing;
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					num = ((!includePawnsMass) ? (num + MassUtility.GearAndInventoryMass(pawn) * (float)count) : (num + pawn.GetStatValue(StatDefOf.Mass) * (float)count));
					if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
					{
						num -= MassUtility.InventoryMass(pawn) * (float)count;
					}
					continue;
				}
				num += thing.GetStatValue(StatDefOf.Mass) * (float)count;
				if (ignoreSpawnedCorpsesGearAndInventory)
				{
					Corpse corpse = thing as Corpse;
					if (corpse != null && corpse.Spawned)
					{
						num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn) * (float)count;
					}
				}
			}
			return Mathf.Max(num, 0f);
		}

		public static float CapacityTransferables(List<TransferableOneWay> transferables, StringBuilder explanation = null)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].HasAnyThing && transferables[i].AnyThing is Pawn)
				{
					TransferableUtility.TransferNoSplit(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
					{
						tmpThingCounts.Add(new ThingCount(originalThing, toTake));
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
			}
			float result = Capacity(tmpThingCounts, explanation);
			tmpThingCounts.Clear();
			return result;
		}

		public static float CapacityLeftAfterTransfer(List<TransferableOneWay> transferables, StringBuilder explanation = null)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].HasAnyThing && transferables[i].AnyThing is Pawn)
				{
					thingsInReverse.Clear();
					thingsInReverse.AddRange(transferables[i].things);
					thingsInReverse.Reverse();
					TransferableUtility.TransferNoSplit(thingsInReverse, transferables[i].MaxCount - transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
					{
						tmpThingCounts.Add(new ThingCount(originalThing, toTake));
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
			}
			thingsInReverse.Clear();
			float result = Capacity(tmpThingCounts, explanation);
			tmpThingCounts.Clear();
			return result;
		}

		public static float CapacityLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, StringBuilder explanation = null)
		{
			tmpThingCounts.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
			float result = Capacity(tmpThingCounts, explanation);
			tmpThingCounts.Clear();
			return result;
		}

		public static float Capacity<T>(List<T> things, StringBuilder explanation = null) where T : Thing
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < things.Count; i++)
			{
				tmpThingCounts.Add(new ThingCount(things[i], things[i].stackCount));
			}
			float result = Capacity(tmpThingCounts, explanation);
			tmpThingCounts.Clear();
			return result;
		}

		public static float MassUsageTransferables(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreSpawnedCorpsesGearAndInventory = false)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableUtility.TransferNoSplit(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
				{
					tmpThingCounts.Add(new ThingCount(originalThing, toTake));
				}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
			}
			float result = MassUsage(tmpThingCounts, ignoreInventory, includePawnsMass, ignoreSpawnedCorpsesGearAndInventory);
			tmpThingCounts.Clear();
			return result;
		}

		public static float MassUsageLeftAfterTransfer(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreSpawnedCorpsesGearAndInventory = false)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				thingsInReverse.Clear();
				thingsInReverse.AddRange(transferables[i].things);
				thingsInReverse.Reverse();
				TransferableUtility.TransferNoSplit(thingsInReverse, transferables[i].MaxCount - transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
				{
					tmpThingCounts.Add(new ThingCount(originalThing, toTake));
				}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
			}
			float result = MassUsage(tmpThingCounts, ignoreInventory, includePawnsMass, ignoreSpawnedCorpsesGearAndInventory);
			tmpThingCounts.Clear();
			return result;
		}

		public static float MassUsage<T>(List<T> things, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreSpawnedCorpsesGearAndInventory = false) where T : Thing
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < things.Count; i++)
			{
				tmpThingCounts.Add(new ThingCount(things[i], things[i].stackCount));
			}
			float result = MassUsage(tmpThingCounts, ignoreInventory, includePawnsMass, ignoreSpawnedCorpsesGearAndInventory);
			tmpThingCounts.Clear();
			return result;
		}

		public static float MassUsageLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreSpawnedCorpsesGearAndInventory = false)
		{
			tmpThingCounts.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
			float result = MassUsage(tmpThingCounts, ignoreInventory, includePawnsMass, ignoreSpawnedCorpsesGearAndInventory);
			tmpThingCounts.Clear();
			return result;
		}
	}
}
