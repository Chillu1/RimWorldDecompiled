using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class GameCondition_PsychicEmanation : GameCondition
{
	public Gender gender;

	public PsychicDroneLevel level = PsychicDroneLevel.BadMedium;

	public const float MaxPointsDroneLow = 800f;

	public const float MaxPointsDroneMedium = 2000f;

	public override string Label
	{
		get
		{
			if (level == PsychicDroneLevel.GoodMedium)
			{
				return def.label + ": " + gender.GetLabel().CapitalizeFirst();
			}
			if (gender != Gender.None)
			{
				return def.label + ": " + level.GetLabel().CapitalizeFirst() + " (" + gender.GetLabel().ToLower() + ")";
			}
			return def.label + ": " + level.GetLabel().CapitalizeFirst();
		}
	}

	public override string LetterText
	{
		get
		{
			if (level == PsychicDroneLevel.GoodMedium)
			{
				return def.letterText.Formatted(gender.GetLabel().ToLower());
			}
			return def.letterText.Formatted(gender.GetLabel().ToLower(), level.GetLabel());
		}
	}

	public override string Description => base.Description.Formatted(gender.GetLabel().ToLower());

	public override void PostMake()
	{
		base.PostMake();
		level = def.defaultDroneLevel;
	}

	public override void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		if (def.defaultDroneLevel == PsychicDroneLevel.GoodMedium)
		{
			level = PsychicDroneLevel.GoodMedium;
		}
		else if (points < 800f)
		{
			level = PsychicDroneLevel.BadLow;
		}
		else if (points < 2000f)
		{
			level = PsychicDroneLevel.BadMedium;
		}
		else
		{
			level = PsychicDroneLevel.BadHigh;
		}
		if (map.mapPawns.FreeColonistsCount > 0)
		{
			gender = map.mapPawns.FreeColonists.RandomElement().gender;
		}
		else
		{
			gender = Rand.Element(Gender.Male, Gender.Female);
		}
		outExtraDescriptionRules.Add(new Rule_String("psychicDroneLevel", level.GetLabel()));
		outExtraDescriptionRules.Add(new Rule_String("psychicDroneGender", gender.GetLabel()));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref gender, "gender", Gender.None);
		Scribe_Values.Look(ref level, "level", PsychicDroneLevel.None);
	}
}
