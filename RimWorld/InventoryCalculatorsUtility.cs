using System;
using Verse;

namespace RimWorld
{
	public static class InventoryCalculatorsUtility
	{
		public static bool ShouldIgnoreInventoryOf(Pawn pawn, IgnorePawnsInventoryMode ignoreMode)
		{
			switch (ignoreMode)
			{
			case IgnorePawnsInventoryMode.Ignore:
				return true;
			case IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload:
				if (pawn.Spawned)
				{
					return pawn.inventory.UnloadEverything;
				}
				return false;
			case IgnorePawnsInventoryMode.IgnoreIfAssignedToUnloadOrPlayerPawn:
				if (!pawn.Spawned || !pawn.inventory.UnloadEverything)
				{
					return Dialog_FormCaravan.CanListInventorySeparately(pawn);
				}
				return true;
			case IgnorePawnsInventoryMode.DontIgnore:
				return false;
			default:
				throw new NotImplementedException("IgnorePawnsInventoryMode");
			}
		}
	}
}
