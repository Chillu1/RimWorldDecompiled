namespace Verse.Sound
{
	public class SoundParamSource_MusicPlayingFadeOut : SoundParamSource
	{
		public override string Label => "Music playing";

		public override float ValueFor(Sample samp)
		{
			if (Current.ProgramState != ProgramState.Playing || Find.MusicManagerPlay == null)
			{
				return 1f;
			}
			return Find.MusicManagerPlay.subtleAmbienceSoundVolumeMultiplier;
		}
	}
}
