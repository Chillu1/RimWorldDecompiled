using System;
using System.Collections.Generic;

namespace Verse;

public class KeyframeAnimationPart : AnimationPart
{
	public List<Keyframe> keyframes;

	protected override Type DefaultWorker => typeof(AnimationWorker_Keyframes);
}
