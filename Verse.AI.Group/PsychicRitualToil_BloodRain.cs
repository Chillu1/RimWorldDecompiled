using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_BloodRain : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private FloatRange durationHoursFromQualityRange;

	protected PsychicRitualToil_BloodRain()
	{
	}

	public PsychicRitualToil_BloodRain(PsychicRitualRoleDef invokerRole, FloatRange durationHoursFromQualityRange)
	{
		this.invokerRole = invokerRole;
		this.durationHoursFromQualityRange = durationHoursFromQualityRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		float duration = durationHoursFromQualityRange.LerpThroughRange(psychicRitual.PowerPercent);
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn, duration);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float duration)
	{
		GameCondition_BloodRain gameCondition_BloodRain = invoker.Map.gameConditionManager.GetActiveCondition(GameConditionDefOf.BloodRain) as GameCondition_BloodRain;
		if (gameCondition_BloodRain == null)
		{
			gameCondition_BloodRain = (GameCondition_BloodRain)GameConditionMaker.MakeCondition(GameConditionDefOf.BloodRain, Mathf.FloorToInt(duration * 2500f));
			gameCondition_BloodRain.psychicRitualDef = psychicRitual.def;
			invoker.Map.GameConditionManager.RegisterCondition(gameCondition_BloodRain);
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), "BloodRainCompleteText".Translate(invoker, Mathf.FloorToInt(duration * 2500f).ToStringTicksToPeriod(), psychicRitual.def.Named("RITUAL")), LetterDefOf.ThreatBig);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Values.Look(ref durationHoursFromQualityRange, "durationHoursFromQualityRange");
	}
}
