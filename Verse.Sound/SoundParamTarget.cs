using System;

namespace Verse.Sound
{
	[EditorShowClassName]
	public abstract class SoundParamTarget
	{
		public abstract string Label
		{
			get;
		}

		public virtual Type NeededFilterType => null;

		public abstract void SetOn(Sample sample, float value);
	}
}
