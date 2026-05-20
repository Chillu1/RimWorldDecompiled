using System;
using Verse;

namespace RimWorld
{
	public static class BedUtility
	{
		public static int GetSleepingSlotsCount(IntVec2 bedSize)
		{
			return bedSize.x;
		}

		private static IntVec3 GetSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize, bool head)
		{
			int sleepingSlotsCount = GetSleepingSlotsCount(bedSize);
			if (index < 0 || index >= sleepingSlotsCount)
			{
				Log.Error("Tried to get sleeping slot pos with index " + index + ", but there are only " + sleepingSlotsCount + " sleeping slots available.");
				return bedCenter;
			}
			CellRect cellRect = GenAdj.OccupiedRect(bedCenter, bedRot, bedSize);
			return bedRot.AsInt switch
			{
				2 => new IntVec3(cellRect.minX + index, bedCenter.y, head ? cellRect.maxZ : cellRect.minZ), 
				0 => new IntVec3(cellRect.maxX - index, bedCenter.y, head ? cellRect.minZ : cellRect.maxZ), 
				3 => new IntVec3(head ? cellRect.maxX : cellRect.minX, bedCenter.y, cellRect.maxZ - index), 
				1 => new IntVec3(head ? cellRect.minX : cellRect.maxX, bedCenter.y, cellRect.minZ + index), 
				_ => throw new NotImplementedException(), 
			};
		}

		public static IntVec3 GetFeetSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
		{
			return GetSlotPos(index, bedCenter, bedRot, bedSize, head: false);
		}

		public static IntVec3 GetSleepingSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
		{
			return GetSlotPos(index, bedCenter, bedRot, bedSize, head: true);
		}

		public static int GetSlotFromPosition(IntVec3 pos, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
		{
			CellRect cellRect = GenAdj.OccupiedRect(bedCenter, bedRot, bedSize);
			if (!cellRect.Contains(pos))
			{
				Log.Error($"{pos} was not inside bed at {bedCenter} with size {bedSize} and rotation {bedRot}");
				return 0;
			}
			int num = bedRot.AsInt switch
			{
				2 => pos.x - cellRect.minX, 
				0 => cellRect.maxX - pos.x, 
				3 => cellRect.maxZ - pos.z, 
				1 => pos.z - cellRect.minZ, 
				_ => throw new NotImplementedException(), 
			};
			if (num < 0 || num >= GetSleepingSlotsCount(bedSize))
			{
				Log.Error($"{num} was outside the valid slot range for this bed: [0, {GetSleepingSlotsCount(bedSize)}).");
				return 0;
			}
			return num;
		}

		public static bool WillingToShareBed(Pawn pawn1, Pawn pawn2)
		{
			if (pawn1 == pawn2)
			{
				return true;
			}
			if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed, pawn1) || !IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed, pawn2))
			{
				return false;
			}
			if (pawn1.relations.GetDirectRelation(PawnRelationDefOf.Spouse, pawn2) != null)
			{
				if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed_Spouse, pawn1) || !IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed_Spouse, pawn2))
				{
					return false;
				}
			}
			else if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed_NonSpouse, pawn1) || !IdeoUtility.DoerWillingToDo(HistoryEventDefOf.SharedBed_NonSpouse, pawn2))
			{
				return false;
			}
			return true;
		}
	}
}
