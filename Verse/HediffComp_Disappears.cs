using System;
using RimWorld;
using UnityEngine;
using Verse.Noise;

namespace Verse;

public class HediffComp_Disappears : HediffComp
{
	public int ticksToDisappear;

	public int disappearsAfterTicks;

	public int seed;

	private const float NoiseScale = 0.25f;

	private const float NoiseWiggliness = 9f;

	public HediffCompProperties_Disappears Props => (HediffCompProperties_Disappears)props;

	public override bool CompShouldRemove
	{
		get
		{
			if (!base.CompShouldRemove && ticksToDisappear > 0)
			{
				if (Props.requiredMentalState != null)
				{
					return base.Pawn.MentalStateDef != Props.requiredMentalState;
				}
				return false;
			}
			return true;
		}
	}

	public float Progress => 1f - (float)ticksToDisappear / (float)Math.Max(1, disappearsAfterTicks);

	public int EffectiveTicksToDisappear => ticksToDisappear / TicksLostPerTick;

	public float NoisyProgress => AddNoiseToProgress(Progress, seed);

	public virtual int TicksLostPerTick => 1;

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (Props.showRemainingTime)
			{
				if (EffectiveTicksToDisappear < 2500)
				{
					return EffectiveTicksToDisappear.ToStringSecondsFromTicks("F0");
				}
				return EffectiveTicksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true, allowYears: true, Props.canUseDecimalsShortForm);
			}
			return base.CompLabelInBracketsExtra;
		}
	}

	private static float AddNoiseToProgress(float progress, int seed)
	{
		float num = (float)Perlin.GetValue(progress, 0.0, 0.0, 9.0, seed);
		float num2 = 0.25f * (1f - progress);
		return Mathf.Clamp01(progress + num2 * num);
	}

	public override void CompPostMake()
	{
		base.CompPostMake();
		disappearsAfterTicks = Props.disappearsAfterTicks.RandomInRange;
		seed = Rand.Int;
		ticksToDisappear = disappearsAfterTicks;
	}

	public void ResetElapsedTicks()
	{
		ticksToDisappear = disappearsAfterTicks;
	}

	public void SetDuration(int ticks)
	{
		disappearsAfterTicks = ticks;
		ticksToDisappear = ticks;
	}

	public override void CompPostPostRemoved()
	{
		if (!Props.leaveFreshWounds)
		{
			foreach (BodyPartRecord partAndAllChildPart in parent.Part.GetPartAndAllChildParts())
			{
				if (base.Pawn.health.hediffSet.GetMissingPartFor(partAndAllChildPart) is Hediff_MissingPart hediff_MissingPart)
				{
					hediff_MissingPart.IsFresh = false;
				}
			}
		}
		if (CompShouldRemove && !Props.messageOnDisappear.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(base.Pawn))
		{
			Messages.Message(Props.messageOnDisappear.Formatted(base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
		}
		if (!Props.letterTextOnDisappear.NullOrEmpty() && !Props.letterLabelOnDisappear.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(base.Pawn) && (!base.Pawn.Dead || Props.sendLetterOnDisappearIfDead))
		{
			Find.LetterStack.ReceiveLetter(Props.letterLabelOnDisappear.Formatted(base.Pawn.Named("PAWN")), Props.letterTextOnDisappear.Formatted(base.Pawn.Named("PAWN")), LetterDefOf.PositiveEvent, base.Pawn);
		}
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		ticksToDisappear -= TicksLostPerTick;
	}

	public override void CompPostMerged(Hediff other)
	{
		base.CompPostMerged(other);
		HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
		if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > ticksToDisappear)
		{
			ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
		Scribe_Values.Look(ref disappearsAfterTicks, "disappearsAfterTicks", 0);
		Scribe_Values.Look(ref seed, "seed", 0);
	}

	public override string CompDebugString()
	{
		return "ticksToDisappear: " + ticksToDisappear;
	}

	public override void CopyFrom(HediffComp other)
	{
		if (other is HediffComp_Disappears hediffComp_Disappears)
		{
			ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			disappearsAfterTicks = hediffComp_Disappears.disappearsAfterTicks;
		}
	}
}
