using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_AssaultThings : LordToil
{
	private List<Thing> things;

	public const int UpdateIntervalTicks = 300;

	public override bool ForceHighStoryDanger => true;

	public override bool AllowSatisfyLongNeeds => false;

	public LordToil_AssaultThings(IEnumerable<Thing> things)
	{
		this.things = new List<Thing>(things);
	}

	public override void Notify_ReachedDutyLocation(Pawn pawn)
	{
		UpdateAllDuties();
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty duty = lord.ownedPawns[i].mindState.duty;
			if (duty == null || duty.def != DutyDefOf.AssaultThing || duty.focus.ThingDestroyed)
			{
				if (!things.Where((Thing t) => t?.Spawned ?? false).TryRandomElement(out var result))
				{
					break;
				}
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultThing, result);
			}
		}
	}

	public override void LordToilTick()
	{
		if (lord.ticksInToil % 300 == 0)
		{
			UpdateAllDuties();
		}
	}
}
