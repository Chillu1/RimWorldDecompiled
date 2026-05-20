using UnityEngine;
using Verse;

namespace RimWorld;

public class VoidMonolithPyramid : ThingWithComps
{
	private const float PyramidMaxOffset = 0.1f;

	private const float PyramidOffsetTimeFactor = 2f;

	public override Vector3 DrawPos
	{
		get
		{
			Vector3 drawPos = base.DrawPos;
			float num = Mathf.Sin(GenTicks.TicksGame.TicksToSeconds() / 2f) * 0.1f;
			drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor() - 0.4f;
			drawPos.z += num;
			return drawPos;
		}
	}
}
