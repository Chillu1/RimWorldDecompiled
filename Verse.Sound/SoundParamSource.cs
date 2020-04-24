namespace Verse.Sound
{
	[EditorShowClassName]
	[EditorReplaceable]
	public abstract class SoundParamSource
	{
		public abstract string Label
		{
			get;
		}

		public abstract float ValueFor(Sample samp);
	}
}
