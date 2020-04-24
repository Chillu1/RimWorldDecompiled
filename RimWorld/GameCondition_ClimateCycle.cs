using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_ClimateCycle : GameCondition
	{
		private int ticksOffset;

		private const float PeriodYears = 4f;

		private const float MaxTempOffset = 20f;

		public override void Init()
		{
			ticksOffset = ((!(Rand.Value < 0.5f)) ? 7200000 : 0);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksOffset, "ticksOffset", 0);
		}

		public override float TemperatureOffset()
		{
			return Mathf.Sin((GenDate.YearsPassedFloat + (float)ticksOffset / 3600000f) / 4f * (float)Math.PI * 2f) * 20f;
		}
	}
}
