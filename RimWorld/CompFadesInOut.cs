using UnityEngine;
using Verse;

namespace RimWorld;

public class CompFadesInOut : ThingComp
{
	private int ageTicks;

	public CompProperties_FadesInOut Props => (CompProperties_FadesInOut)props;

	public override void CompTick()
	{
		base.CompTick();
		if (parent.Spawned)
		{
			ageTicks++;
		}
	}

	public float Opacity()
	{
		float num = ageTicks.TicksToSeconds();
		if (num <= Props.fadeInSecs)
		{
			if (Props.fadeInSecs > 0f)
			{
				return num / Props.fadeInSecs;
			}
			return 1f;
		}
		if (num <= Props.fadeInSecs + Props.solidTimeSecs)
		{
			return 1f;
		}
		if (Props.fadeOutSecs > 0f)
		{
			return 1f - Mathf.InverseLerp(Props.fadeInSecs + Props.solidTimeSecs, Props.fadeInSecs + Props.solidTimeSecs + Props.fadeOutSecs, num);
		}
		return 1f;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
	}
}
