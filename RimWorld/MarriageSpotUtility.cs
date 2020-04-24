using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class MarriageSpotUtility
	{
		public static bool IsValidMarriageSpot(IntVec3 cell, Map map, StringBuilder outFailReason = null)
		{
			if (!cell.Standable(map))
			{
				outFailReason?.Append("MarriageSpotNotStandable".Translate());
				return false;
			}
			if (!cell.Roofed(map) && !JoyUtility.EnjoyableOutsideNow(map, outFailReason))
			{
				return false;
			}
			return true;
		}

		public static bool IsValidMarriageSpotFor(IntVec3 cell, Pawn firstFiance, Pawn secondFiance, StringBuilder outFailReason = null)
		{
			if (!firstFiance.Spawned || !secondFiance.Spawned)
			{
				Log.Warning("Can't check if a marriage spot is valid because one of the fiances isn't spawned.");
				return false;
			}
			if (firstFiance.Map != secondFiance.Map)
			{
				return false;
			}
			if (!IsValidMarriageSpot(cell, firstFiance.Map, outFailReason))
			{
				return false;
			}
			if (!cell.Roofed(firstFiance.Map))
			{
				if (!JoyUtility.EnjoyableOutsideNow(firstFiance, outFailReason))
				{
					return false;
				}
				if (!JoyUtility.EnjoyableOutsideNow(secondFiance, outFailReason))
				{
					return false;
				}
			}
			if (cell.GetDangerFor(firstFiance, firstFiance.Map) != Danger.None)
			{
				outFailReason?.Append("MarriageSpotDangerous".Translate(firstFiance.LabelShort, firstFiance));
				return false;
			}
			if (cell.GetDangerFor(secondFiance, secondFiance.Map) != Danger.None)
			{
				outFailReason?.Append("MarriageSpotDangerous".Translate(secondFiance.LabelShort, secondFiance));
				return false;
			}
			if (cell.IsForbidden(firstFiance))
			{
				outFailReason?.Append("MarriageSpotForbidden".Translate(firstFiance.LabelShort, firstFiance));
				return false;
			}
			if (cell.IsForbidden(secondFiance))
			{
				outFailReason?.Append("MarriageSpotForbidden".Translate(secondFiance.LabelShort, secondFiance));
				return false;
			}
			if (!firstFiance.CanReserve(cell) || !secondFiance.CanReserve(cell))
			{
				outFailReason?.Append("MarriageSpotReserved".Translate());
				return false;
			}
			if (!firstFiance.CanReach(cell, PathEndMode.OnCell, Danger.None))
			{
				outFailReason?.Append("MarriageSpotUnreachable".Translate(firstFiance.LabelShort, firstFiance));
				return false;
			}
			if (!secondFiance.CanReach(cell, PathEndMode.OnCell, Danger.None))
			{
				outFailReason?.Append("MarriageSpotUnreachable".Translate(secondFiance.LabelShort, secondFiance));
				return false;
			}
			if (!firstFiance.IsPrisoner && !secondFiance.IsPrisoner)
			{
				Room room = cell.GetRoom(firstFiance.Map);
				if (room != null && room.isPrisonCell)
				{
					outFailReason?.Append("MarriageSpotInPrisonCell".Translate());
					return false;
				}
			}
			return true;
		}
	}
}
