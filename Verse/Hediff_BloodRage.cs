using RimWorld;

namespace Verse;

public class Hediff_BloodRage : Hediff
{
	private float adjustedSeverityRaisePerTick;

	private const float SeverityRaisePerTick = 8.8E-05f;

	private const float SeverityRaisePerTickAnimals = 8.8E-05f;

	private const float SeverityFallPerTick = 0.000132f;

	private static readonly FloatRange SeverityRaiseRange = new FloatRange(0.75f, 1.25f);

	private static readonly FloatRange SeverityRaiseRangeAnimal = new FloatRange(0.1f, 0.8f);

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Blood rage"))
		{
			pawn.health.RemoveHediff(this);
			return;
		}
		base.PostAdd(dinfo);
		if (pawn.RaceProps.Animal)
		{
			adjustedSeverityRaisePerTick = 8.8E-05f * SeverityRaiseRangeAnimal.RandomInRange;
		}
		else if (pawn.RaceProps.Humanlike)
		{
			adjustedSeverityRaisePerTick = 8.8E-05f * SeverityRaiseRange.RandomInRange;
			if (pawn.story.traits.HasTrait(TraitDefOf.Wimp))
			{
				adjustedSeverityRaisePerTick *= 0.5f;
			}
			if (pawn.story.traits.HasTrait(TraitDefOf.Kind))
			{
				adjustedSeverityRaisePerTick *= 0.5f;
			}
		}
		adjustedSeverityRaisePerTick *= pawn.GetStatValue(StatDefOf.PsychicSensitivity);
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (Severity >= 1f)
		{
			if (pawn.RaceProps.Humanlike)
			{
				BloodRainUtility.TryTriggerBerserkShort(pawn);
			}
			if (pawn.RaceProps.Animal && pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterBloodRain) && MessagesRepeatAvoider.MessageShowAllowed("MessageManhunterBloodrain-" + pawn.LabelShort, 240f))
			{
				Messages.Message("Manhunter_BloodRain".Translate(pawn), pawn, MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}
		if (BloodRainUtility.ExposedToBloodRain(pawn))
		{
			Severity += adjustedSeverityRaisePerTick;
		}
		else
		{
			Severity -= 0.000132f;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref adjustedSeverityRaisePerTick, "adjustedSeverityRaisePerTick", 0f);
	}
}
