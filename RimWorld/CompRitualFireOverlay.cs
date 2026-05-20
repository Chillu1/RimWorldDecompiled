using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompRitualFireOverlay : CompFireOverlayBase
	{
		public const int FireGlowIntervalTicks = 30;

		public new CompProperties_FireOverlayRitual Props => (CompProperties_FireOverlayRitual)props;

		public override void PostDraw()
		{
			base.PostDraw();
			LordJob_Ritual lordJob_Ritual = parent.TargetOfRitual();
			if (lordJob_Ritual != null && !(lordJob_Ritual.Progress < Props.minRitualProgress))
			{
				Vector3 loc = parent.TrueCenter();
				loc.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
				CompFireOverlay.FireGraphic.Draw(loc, Rot4.North, parent);
			}
		}

		public override void CompTick()
		{
			LordJob_Ritual lordJob_Ritual = parent.TargetOfRitual();
			if (lordJob_Ritual != null && !(lordJob_Ritual.Progress < Props.minRitualProgress))
			{
				if (startedGrowingAtTick < 0)
				{
					startedGrowingAtTick = GenTicks.TicksAbs;
				}
				if (GenTicks.TicksAbs % 30 == 0)
				{
					FleckMaker.ThrowFireGlow(parent.TrueCenter(), parent.Map, 1f);
				}
			}
		}
	}
}
