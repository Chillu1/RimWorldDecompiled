using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyEcho : SoundParamTarget
	{
		private EchoFilterProperty filterProperty;

		public override string Label => "EchoFilter-" + filterProperty;

		public override Type NeededFilterType => typeof(SoundFilterEcho);

		public override void SetOn(Sample sample, float value)
		{
			AudioEchoFilter audioEchoFilter = sample.source.GetComponent<AudioEchoFilter>();
			if (audioEchoFilter == null)
			{
				audioEchoFilter = sample.source.gameObject.AddComponent<AudioEchoFilter>();
			}
			if (filterProperty == EchoFilterProperty.Delay)
			{
				audioEchoFilter.delay = value;
			}
			if (filterProperty == EchoFilterProperty.DecayRatio)
			{
				audioEchoFilter.decayRatio = value;
			}
			if (filterProperty == EchoFilterProperty.WetMix)
			{
				audioEchoFilter.wetMix = value;
			}
			if (filterProperty == EchoFilterProperty.DryMix)
			{
				audioEchoFilter.dryMix = value;
			}
		}
	}
}
