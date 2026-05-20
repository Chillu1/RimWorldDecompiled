using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_PrepareCaravan_GatherItems : LordToil
{
	private IntVec3 meetingPoint;

	public override float? CustomWakeThreshold => 0.5f;

	public override bool AllowRestingInBed => true;

	public LordToil_PrepareCaravan_GatherItems(IntVec3 meetingPoint)
	{
		this.meetingPoint = meetingPoint;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn.IsColonist)
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherItems);
			}
			else
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, meetingPoint);
			}
		}
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		if (Find.TickManager.TicksGame % 120 != 0 || !CaravanFormingUtility.AllItemsLoadedOntoCaravan(lord, base.Map))
		{
			return;
		}
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			ownedPawn.inventory.ClearHaulingCaravanCache();
		}
		lord.ReceiveMemo("AllItemsGathered");
	}
}
