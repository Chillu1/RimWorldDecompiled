using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_TravelAndWaitForItems : LordToil_Travel, IWaitForItemsLordToil
	{
		public Pawn target;

		public ThingDef requestedThingDef;

		public int requestedThingCount;

		public bool HasAllRequestedItems => CountRemaining <= 0;

		public int CountRemaining => GiveItemsToPawnUtility.GetCountRemaining(target, requestedThingDef, requestedThingCount);

		public LordToil_TravelAndWaitForItems(IntVec3 dest, Pawn target, ThingDef thingDef, int amount)
			: base(dest)
		{
			this.target = target;
			requestedThingDef = thingDef;
			requestedThingCount = amount;
		}

		public override void DrawPawnGUIOverlay(Pawn pawn)
		{
			if (pawn == target)
			{
				pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
			}
		}

		public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn requester, Pawn current)
		{
			if (target != requester)
			{
				yield break;
			}
			foreach (FloatMenuOption item in GiveItemsToPawnUtility.GetFloatMenuOptionsForPawn(requester, current, requestedThingDef, requestedThingCount))
			{
				yield return item;
			}
		}
	}
}
