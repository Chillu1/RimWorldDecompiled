using Verse;

namespace RimWorld
{
	public class StyleItemSpawningProperties : IExposable
	{
		public StyleItemFrequency frequency;

		public StyleGender gender = StyleGender.Any;

		public void ExposeData()
		{
			Scribe_Values.Look(ref frequency, "frequency", StyleItemFrequency.Never);
			Scribe_Values.Look(ref gender, "gender", StyleGender.Male);
		}
	}
}
