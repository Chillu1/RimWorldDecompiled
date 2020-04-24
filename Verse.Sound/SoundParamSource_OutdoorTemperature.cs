namespace Verse.Sound
{
	public class SoundParamSource_OutdoorTemperature : SoundParamSource
	{
		public override string Label => "Outdoor temperature";

		public override float ValueFor(Sample samp)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 0f;
			}
			if (Find.CurrentMap == null)
			{
				return 0f;
			}
			return Find.CurrentMap.mapTemperature.OutdoorTemp;
		}
	}
}
