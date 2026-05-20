using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_LoadAndEnterPortal : LordToil
{
	public MapPortal portal;

	public override bool AllowSatisfyLongNeeds => false;

	public LordToil_LoadAndEnterPortal(MapPortal portal)
	{
		this.portal = portal;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty pawnDuty = new PawnDuty(DutyDefOf.LoadAndEnterPortal);
			pawnDuty.focus = new LocalTargetInfo(portal);
			lord.ownedPawns[i].mindState.duty = pawnDuty;
		}
	}
}
