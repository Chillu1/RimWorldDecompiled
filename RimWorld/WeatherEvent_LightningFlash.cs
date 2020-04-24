using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class WeatherEvent_LightningFlash : WeatherEvent
	{
		private int duration;

		private Vector2 shadowVector;

		private int age;

		private const int FlashFadeInTicks = 3;

		private const int MinFlashDuration = 15;

		private const int MaxFlashDuration = 60;

		private const float FlashShadowDistance = 5f;

		private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new Color(0.9f, 0.95f, 1f), new Color(40f / 51f, 0.8235294f, 72f / 85f), new Color(0.9f, 0.95f, 1f), 1.15f);

		public override bool Expired => age > duration;

		public override SkyTarget SkyTarget => new SkyTarget(1f, LightningFlashColors, 1f, 1f);

		public override Vector2? OverrideShadowVector => shadowVector;

		public override float SkyTargetLerpFactor => LightningBrightness;

		protected float LightningBrightness
		{
			get
			{
				if (age <= 3)
				{
					return (float)age / 3f;
				}
				return 1f - (float)age / (float)duration;
			}
		}

		public WeatherEvent_LightningFlash(Map map)
			: base(map)
		{
			duration = Rand.Range(15, 60);
			shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
		}

		public override void FireEvent()
		{
			SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
		}

		public override void WeatherEventTick()
		{
			age++;
		}
	}
}
