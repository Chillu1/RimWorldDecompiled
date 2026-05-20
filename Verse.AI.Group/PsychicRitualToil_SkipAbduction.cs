using System.Collections.Generic;
using RimWorld;
using Verse.Sound;

namespace Verse.AI.Group;

public class PsychicRitualToil_SkipAbduction : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private const int ComaDurationTicks = 30000;

	protected PsychicRitualToil_SkipAbduction()
	{
	}

	public PsychicRitualToil_SkipAbduction(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
	{
		List<Pawn> freeColonistsSpawned = psychicRitual.Map.mapPawns.FreeColonistsSpawned;
		freeColonistsSpawned.AddRange(psychicRitual.Map.mapPawns.SlavesOfColonySpawned);
		freeColonistsSpawned.RemoveAll((Pawn p) => p.health.hediffSet.HasHediff(HediffDefOf.DarkPsychicShock));
		if (!freeColonistsSpawned.TryRandomElement(out var result))
		{
			return;
		}
		psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(result, result.Map), result.PositionHeld, 60);
		SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(result.PositionHeld, result.Map));
		IntVec3 result2 = psychicRitual.assignments.Target.Cell;
		if (!result2.Standable(psychicRitual.Map))
		{
			CellFinder.TryFindRandomSpawnCellForPawnNear(invoker.PositionHeld, psychicRitual.Map, out result2);
		}
		SkipUtility.SkipTo(result, result2, psychicRitual.Map);
		psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(result2, psychicRitual.Map), result2, 60);
		SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result2, psychicRitual.Map));
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, result);
		hediff.TryGetComp<HediffComp_Disappears>().disappearsAfterTicks = 30000;
		result.health.AddHediff(hediff);
		foreach (Pawn allAssignedPawn in psychicRitual.assignments.AllAssignedPawns)
		{
			result.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.UsedMeForPsychicRitual, allAssignedPawn);
		}
		if (!psychicRitual.def.letterAICompleteLabel.NullOrEmpty() && !psychicRitual.def.letterAICompleteText.NullOrEmpty())
		{
			Find.LetterStack.ReceiveLetter(psychicRitual.def.letterAICompleteLabel, psychicRitual.def.letterAICompleteText.Formatted(result.Named("PAWN")), LetterDefOf.ThreatBig, result);
		}
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		foreach (Pawn allAssignedPawn in psychicRitual.assignments.AllAssignedPawns)
		{
			SetPawnDuty(allAssignedPawn, psychicRitual, parent, DutyDefOf.Idle);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
