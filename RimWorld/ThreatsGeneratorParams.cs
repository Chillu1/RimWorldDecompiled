using Verse;

namespace RimWorld
{
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
			string str = "(";
			str = str + "onDays=" + onDays.ToString("0.##");
			str = str + " offDays=" + offDays.ToString("0.##");
			str = str + " minSpacingDays=" + minSpacingDays.ToString("0.##");
			str = str + " numIncidentsRange=" + numIncidentsRange;
			if (threatPoints.HasValue)
			{
				str = str + " threatPoints=" + threatPoints.Value;
			}
			if (minThreatPoints.HasValue)
			{
				str = str + " minThreatPoints=" + minThreatPoints.Value;
			}
			if (faction != null)
			{
				str = str + " faction=" + faction;
			}
			return str + ")";
		}
	}
}
