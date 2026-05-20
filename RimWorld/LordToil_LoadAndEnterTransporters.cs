using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_LoadAndEnterTransporters : LordToil
{
	public int transportersGroup = -1;

	public override bool AllowSatisfyLongNeeds => false;

	public LordToil_LoadAndEnterTransporters(int transportersGroup)
	{
		this.transportersGroup = transportersGroup;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty pawnDuty = new PawnDuty(DutyDefOf.LoadAndEnterTransporters);
			pawnDuty.transportersGroup = transportersGroup;
			lord.ownedPawns[i].mindState.duty = pawnDuty;
		}
	}
}
