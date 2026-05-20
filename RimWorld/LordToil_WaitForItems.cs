using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_WaitForItems : LordToil, IWaitForItemsLordToil
{
	public IntVec3 waitSpot;

	public Pawn target;

	public ThingDef requestedThingDef;

	public int requestedThingCount;

	private const float WanderRadius = 10f;

	public bool HasAllRequestedItems => CountRemaining <= 0;

	public int CountRemaining => GiveItemsToPawnUtility.GetCountRemaining(target, requestedThingDef, requestedThingCount);

	public LordToil_WaitForItems(Pawn target, ThingDef thingDef, int amount, IntVec3 waitSpot)
	{
		this.waitSpot = waitSpot;
		this.target = target;
		requestedThingDef = thingDef;
		requestedThingCount = amount;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.WanderClose_NoNeeds, waitSpot, 10f);
		}
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
