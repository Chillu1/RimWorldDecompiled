using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_HuntDownColonists : LordToil
{
	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].mindState != null)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.HuntDownColonists);
				lord.ownedPawns[i].TryGetComp<CompCanBeDormant>()?.WakeUp();
			}
		}
	}
}
