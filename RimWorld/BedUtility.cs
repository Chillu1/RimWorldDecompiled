using Verse;

namespace RimWorld
{
	public static class BedUtility
	{
		public static int GetSleepingSlotsCount(IntVec2 bedSize)
		{
			return bedSize.x;
		}

		public static IntVec3 GetSleepingSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
		{
			int sleepingSlotsCount = GetSleepingSlotsCount(bedSize);
			if (index < 0 || index >= sleepingSlotsCount)
			{
				Log.Error("Tried to get sleeping slot pos with index " + index + ", but there are only " + sleepingSlotsCount + " sleeping slots available.");
				return bedCenter;
			}
			CellRect cellRect = GenAdj.OccupiedRect(bedCenter, bedRot, bedSize);
			if (bedRot == Rot4.North)
			{
				return new IntVec3(cellRect.minX + index, bedCenter.y, cellRect.minZ);
			}
			if (bedRot == Rot4.East)
			{
				return new IntVec3(cellRect.minX, bedCenter.y, cellRect.maxZ - index);
			}
			if (bedRot == Rot4.South)
			{
				return new IntVec3(cellRect.minX + index, bedCenter.y, cellRect.maxZ);
			}
			return new IntVec3(cellRect.maxX, bedCenter.y, cellRect.maxZ - index);
		}
	}
}
