using Verse;

namespace RimWorld;

public class ThreatsGeneratorParams : IExposable
{
	public AllowedThreatsGeneratorThreats allowedThreats;

	public int randSeed;

	public float onDays;

	public float offDays;

	public float minSpacingDays;

	public FloatRange numIncidentsRange;

	public float? threatPoints;

	public float? minThreatPoints;

	public float currentThreatPointsFactor = 1f;

	public Faction faction;

	public void ExposeData()
	{
		Scribe_Values.Look(ref allowedThreats, "allowedThreats", AllowedThreatsGeneratorThreats.None);
		Scribe_Values.Look(ref randSeed, "randSeed", 0);
		Scribe_Values.Look(ref onDays, "onDays", 0f);
		Scribe_Values.Look(ref offDays, "offDays", 0f);
		Scribe_Values.Look(ref minSpacingDays, "minSpacingDays", 0f);
		Scribe_Values.Look(ref numIncidentsRange, "numIncidentsRange");
		Scribe_Values.Look(ref threatPoints, "threatPoints");
		Scribe_Values.Look(ref minThreatPoints, "minThreatPoints");
		Scribe_Values.Look(ref currentThreatPointsFactor, "currentThreatPointsFactor", 1f);
		Scribe_References.Look(ref faction, "faction");
	}

	public override string ToString()
	{
		string text = "(";
		text = text + "onDays=" + onDays.ToString("0.##");
		text = text + " offDays=" + offDays.ToString("0.##");
		text = text + " minSpacingDays=" + minSpacingDays.ToString("0.##");
		string text2 = text;
		FloatRange floatRange = numIncidentsRange;
		text = text2 + " numIncidentsRange=" + floatRange.ToString();
		if (threatPoints.HasValue)
		{
			text = text + " threatPoints=" + threatPoints.Value;
		}
		if (minThreatPoints.HasValue)
		{
			text = text + " minThreatPoints=" + minThreatPoints.Value;
		}
		if (faction != null)
		{
			text = text + " faction=" + faction;
		}
		return text + ")";
	}
}
