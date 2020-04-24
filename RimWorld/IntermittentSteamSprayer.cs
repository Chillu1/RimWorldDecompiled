using System;
using Verse;

namespace RimWorld
{
	public class IntermittentSteamSprayer
	{
		private Thing parent;

		private int ticksUntilSpray = 500;

		private int sprayTicksLeft;

		public Action startSprayCallback;

		public Action endSprayCallback;

		private const int MinTicksBetweenSprays = 500;

		private const int MaxTicksBetweenSprays = 2000;

		private const int MinSprayDuration = 200;

		private const int MaxSprayDuration = 500;

		private const float SprayThickness = 0.6f;

		public IntermittentSteamSprayer(Thing parent)
		{
			this.parent = parent;
		}

		public void SteamSprayerTick()
		{
			if (sprayTicksLeft > 0)
			{
				sprayTicksLeft--;
				if (Rand.Value < 0.6f)
				{
					MoteMaker.ThrowAirPuffUp(parent.TrueCenter(), parent.Map);
				}
				if (Find.TickManager.TicksGame % 20 == 0)
				{
					GenTemperature.PushHeat(parent, 40f);
				}
				if (sprayTicksLeft <= 0)
				{
					if (endSprayCallback != null)
					{
						endSprayCallback();
					}
					ticksUntilSpray = Rand.RangeInclusive(500, 2000);
				}
				return;
			}
			ticksUntilSpray--;
			if (ticksUntilSpray <= 0)
			{
				if (startSprayCallback != null)
				{
					startSprayCallback();
				}
				sprayTicksLeft = Rand.RangeInclusive(200, 500);
			}
		}
	}
}
