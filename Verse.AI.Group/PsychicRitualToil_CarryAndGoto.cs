using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_CarryAndGoto : PsychicRitualToil_Goto
{
	public PsychicRitualRoleDef carrierRole;

	public PsychicRitualRoleDef payloadRole;

	protected List<IntVec3> payloadPositions;

	protected PsychicRitualToil_CarryAndGoto()
	{
	}

	public PsychicRitualToil_CarryAndGoto(PsychicRitualRoleDef carrierRole, PsychicRitualRoleDef payloadRole, IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> rolePositions)
		: base(rolePositions.Slice(carrierRole))
	{
		payloadPositions = new List<IntVec3>(rolePositions[payloadRole]);
		this.carrierRole = carrierRole;
		this.payloadRole = payloadRole;
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.UpdateAllDuties(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(payloadRole);
		if (pawn == null)
		{
			return;
		}
		Pawn pawn2 = psychicRitual.assignments.FirstAssignedPawn(carrierRole);
		if (pawn2 != null)
		{
			if (pawn.IsPrisoner)
			{
				SetPawnDuty(pawn, psychicRitual, parent, DutyDefOf.Idle);
				pawn.mindState.duty.focus = psychicRitual.assignments.Target.Cell;
			}
			SetPawnDuty(pawn2, psychicRitual, parent, DutyDefOf.DeliverPawnToPsychicRitualCell, pawn2.mindState.duty.focus, pawn, payloadPositions[0], base.FinalGatherPhase ? "final" : "initial");
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref carrierRole, "carrierRole");
		Scribe_Defs.Look(ref payloadRole, "payloadRole");
		Scribe_Collections.Look(ref payloadPositions, "payloadPositions", LookMode.Value);
	}
}
