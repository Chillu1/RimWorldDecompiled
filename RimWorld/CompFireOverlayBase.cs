using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompFireOverlayBase : ThingComp
	{
		protected int startedGrowingAtTick = -1;

		public CompProperties_FireOverlay Props => (CompProperties_FireOverlay)props;

		public float FireSize
		{
			get
			{
				if (startedGrowingAtTick < 0)
				{
					return Props.fireSize;
				}
				return Mathf.Lerp(Props.fireSize, Props.finalFireSize, (float)(GenTicks.TicksAbs - startedGrowingAtTick) / Props.fireGrowthDurationTicks);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref startedGrowingAtTick, "startedGrowingAtTick", -1);
		}
	}
}
