namespace Verse
{
	public class HediffCompProperties_SkillDecay : HediffCompProperties
	{
		public SimpleCurve decayPerDayPercentageLevelCurve;

		public HediffCompProperties_SkillDecay()
		{
			compClass = typeof(HediffComp_SkillDecay);
		}
	}
}
