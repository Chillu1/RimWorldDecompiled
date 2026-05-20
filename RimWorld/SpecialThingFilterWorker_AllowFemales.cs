using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_AllowFemales : SpecialThingFilterWorker_AllowGender
	{
		public SpecialThingFilterWorker_AllowFemales()
			: base(Gender.Female)
		{
		}
	}
}
