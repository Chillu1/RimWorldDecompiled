using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_PleasurePulse : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	protected PsychicRitualToil_PleasurePulse()
	{
	}

	public PsychicRitualToil_PleasurePulse(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		float duration = ((PsychicRitualDef_PleasurePulse)psychicRitual.def).durationDaysFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn, duration);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float duration)
	{
		foreach (Pawn item in invoker.Map.mapPawns.AllHumanlikeSpawned)
		{
			if (!(item.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f))
			{
				Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.NeurosisPulse);
				if (firstHediffOfDef != null)
				{
					item.health.RemoveHediff(firstHediffOfDef);
				}
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PleasurePulse, item);
				HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
				if (hediffComp_Disappears != null)
				{
					hediffComp_Disappears.ticksToDisappear = Mathf.RoundToInt(duration * 60000f);
				}
				item.health.AddHediff(hediff);
			}
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), "PleasurePulseCompleteText".Translate(invoker, Mathf.FloorToInt(duration * 60000f).ToStringTicksToDays(), psychicRitual.def.Named("RITUAL")), LetterDefOf.NeutralEvent);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
