using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherEvent_LightningStrikeDelayed : WeatherEvent_LightningFlash
	{
		private IntVec3 strikeLoc = IntVec3.Invalid;

		private Mesh boltMesh;

		private int delay;

		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

		public override bool Expired => age > duration + delay;

		protected override float LightningBrightness
		{
			get
			{
				if (age < delay)
				{
					return -1f;
				}
				if (age - delay <= 3)
				{
					return (float)(age - delay) / 3f;
				}
				return 1f - (float)(age - delay) / (float)duration;
			}
		}

		public override Vector2? OverrideShadowVector
		{
			get
			{
				if (age >= delay)
				{
					return shadowVector;
				}
				return null;
			}
		}

		public WeatherEvent_LightningStrikeDelayed(Map map)
			: base(map)
		{
		}

		public WeatherEvent_LightningStrikeDelayed(Map map, IntVec3 forcedStrikeLoc, int delay)
			: base(map)
		{
			strikeLoc = forcedStrikeLoc;
			this.delay = delay;
		}

		public override void FireEvent()
		{
		}

		public override void WeatherEventTick()
		{
			age++;
			if (age == delay)
			{
				WeatherEvent_LightningStrike.DoStrike(strikeLoc, map, ref boltMesh);
			}
		}

		public override void WeatherEventDraw()
		{
			if (age >= delay)
			{
				Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
			}
		}
	}
}
