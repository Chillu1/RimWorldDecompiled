using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_AllowMales : SpecialThingFilterWorker_AllowGender
	{
		public SpecialThingFilterWorker_AllowMales()
			: base(Gender.Male)
		{
		}
	}
}
