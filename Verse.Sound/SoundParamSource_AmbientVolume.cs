namespace Verse.Sound
{
	public class SoundParamSource_AmbientVolume : SoundParamSource
	{
		public override string Label => "Ambient volume";

		public override float ValueFor(Sample samp)
		{
			return Prefs.VolumeAmbient;
		}
	}
}
