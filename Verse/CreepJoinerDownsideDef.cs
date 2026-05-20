using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class CreepJoinerDownsideDef : CreepJoinerBaseDef
{
	public bool repeats;

	public bool hasLetter;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterDesc;

	public LetterDef letterDef;

	public List<BackstoryTrait> traits = new List<BackstoryTrait>();

	public List<HediffDef> hediffs = new List<HediffDef>();

	public FloatRange triggersAfterDays = FloatRange.Zero;

	public List<AbilityDef> abilities = new List<AbilityDef>();

	public float triggerMtbDays;

	public FloatRange triggerMinDays = FloatRange.Zero;

	public bool canOccurWhenImprisoned;

	public bool canOccurWhileDowned = true;

	public bool mustBeConscious;

	public Type workerType;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (triggerMtbDays != 0f && triggersAfterDays != FloatRange.Zero)
		{
			yield return "cannot use both triggerMtbDays and triggersAfterDays fields, use one or neither.";
		}
	}
}
