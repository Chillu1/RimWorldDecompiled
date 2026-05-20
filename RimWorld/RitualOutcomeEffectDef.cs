using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectDef : Def
{
	public class ExtraOutcomeChanceDescription
	{
		[MustTranslate]
		public string description;

		public Func<float, float> qualityToValue;
	}

	public Type workerClass;

	public float startingQuality;

	public float minQuality;

	public float maxQuality = 1f;

	public bool givesDevelopmentPoints = true;

	public ThoughtDef memoryDef;

	public List<RitualOutcomePossibility> outcomeChances = new List<RitualOutcomePossibility>();

	public SimpleCurve honorFromQuality;

	[MustTranslate]
	public List<string> extraPredictedOutcomeDescriptions;

	[MustTranslate]
	public List<string> extraInfoLines;

	public List<ExtraOutcomeChanceDescription> extraOutcomeDescriptions;

	public EffecterDef effecter;

	public FleckDef fleckDef;

	public int flecksPerCell;

	public FloatRange fleckRotationRange = new FloatRange(0f, 360f);

	public FloatRange fleckScaleRange = FloatRange.One;

	public FloatRange fleckVelocityAngle = FloatRange.Zero;

	public FloatRange fleckVelocitySpeed = FloatRange.Zero;

	public ThingDef filthDefToSpawn;

	public IntRange filthCountToSpawn = IntRange.Zero;

	public List<RitualOutcomeComp> comps;

	public bool warnOnLowQuality = true;

	public bool allowAttachableOutcome = true;

	public bool allowOutcomeWithNoAttendance;

	private float minMoodCached = float.MaxValue;

	private float maxMoodCached = float.MinValue;

	private float moodDurationCached = -1f;

	private RitualOutcomePossibility bestOutcomeCached;

	private RitualOutcomePossibility worstOutcomeCached;

	public string Description
	{
		get
		{
			if (minMoodCached == float.MaxValue && !outcomeChances.NullOrEmpty())
			{
				foreach (RitualOutcomePossibility outcomeChance in outcomeChances)
				{
					if (outcomeChance.memory != null)
					{
						float baseMoodEffect = outcomeChance.memory.stages[0].baseMoodEffect;
						if (baseMoodEffect > maxMoodCached)
						{
							maxMoodCached = baseMoodEffect;
						}
						if (baseMoodEffect < minMoodCached)
						{
							minMoodCached = baseMoodEffect;
						}
						moodDurationCached = outcomeChance.memory.durationDays;
					}
				}
			}
			return description.Formatted(minMoodCached.Named("MINMOOD"), maxMoodCached.ToStringWithSign().Named("MAXMOOD"), moodDurationCached.Named("MOODDAYS"));
		}
	}

	public RitualOutcomePossibility BestOutcome
	{
		get
		{
			if (bestOutcomeCached == null)
			{
				bestOutcomeCached = outcomeChances.MaxBy((RitualOutcomePossibility o) => o.positivityIndex);
			}
			return bestOutcomeCached;
		}
	}

	public RitualOutcomePossibility WorstOutcome
	{
		get
		{
			if (worstOutcomeCached == null)
			{
				worstOutcomeCached = outcomeChances.MinBy((RitualOutcomePossibility o) => o.positivityIndex);
			}
			return worstOutcomeCached;
		}
	}

	public RitualOutcomeEffectWorker GetInstance()
	{
		return (RitualOutcomeEffectWorker)Activator.CreateInstance(workerClass, this);
	}

	public string OutcomeMoodBreakdown(RitualOutcomePossibility outcome)
	{
		if (outcome.memory != null && outcome.memory.stages[0].baseMoodEffect != 0f)
		{
			return "RitualOutcomeExtraDesc_Mood".Translate(outcome.memory.stages[0].baseMoodEffect.ToStringWithSign(), outcome.memory.durationDays);
		}
		return "";
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		float num = 0f;
		foreach (RitualOutcomePossibility outcomeChance in outcomeChances)
		{
			num += outcomeChance.chance;
		}
		if (outcomeChances.Any() && Mathf.Abs(num - 1f) > 0.0001f)
		{
			yield return "Sum of outcome chances doesn't add up to 1.0. total=" + num;
		}
	}
}
