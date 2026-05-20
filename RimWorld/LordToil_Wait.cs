using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_Wait : LordToil
{
	public bool allowRandomInteractions = true;

	protected virtual DutyDef IdleDutyDef
	{
		get
		{
			if (!allowRandomInteractions)
			{
				return DutyDefOf.IdleNoInteraction;
			}
			return DutyDefOf.Idle;
		}
	}

	public LordToil_Wait(bool allowRandomInteractions = true)
	{
		this.allowRandomInteractions = allowRandomInteractions;
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty duty = new PawnDuty(IdleDutyDef);
			DecoratePawnDuty(duty);
			lord.ownedPawns[i].mindState.duty = duty;
		}
	}

	protected virtual void DecoratePawnDuty(PawnDuty duty)
	{
	}
}
