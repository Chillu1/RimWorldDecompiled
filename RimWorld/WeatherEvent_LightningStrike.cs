using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherEvent_LightningStrike : WeatherEvent_LightningFlash
	{
		private IntVec3 strikeLoc = IntVec3.Invalid;

		private Mesh boltMesh;

		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

		public WeatherEvent_LightningStrike(Map map)
			: base(map)
		{
		}

		public WeatherEvent_LightningStrike(Map map, IntVec3 forcedStrikeLoc)
			: base(map)
		{
			strikeLoc = forcedStrikeLoc;
		}

		public override void FireEvent()
		{
			base.FireEvent();
			if (!strikeLoc.IsValid)
			{
				strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
			}
			boltMesh = LightningBoltMeshPool.RandomBoltMesh;
			if (!strikeLoc.Fogged(map))
			{
				GenExplosion.DoExplosion(strikeLoc, map, 1.9f, DamageDefOf.Flame, null);
				Vector3 loc = strikeLoc.ToVector3Shifted();
				for (int i = 0; i < 4; i++)
				{
					MoteMaker.ThrowSmoke(loc, map, 1.5f);
					MoteMaker.ThrowMicroSparks(loc, map);
					MoteMaker.ThrowLightningGlow(loc, map, 1.5f);
				}
			}
			SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
			SoundDefOf.Thunder_OnMap.PlayOneShot(info);
		}

		public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, base.LightningBrightness), 0);
		}
	}
}
