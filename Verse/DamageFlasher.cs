using UnityEngine;

namespace Verse
{
	public class DamageFlasher
	{
		private int lastDamageTick = -9999;

		private const int DamagedMatTicksTotal = 16;

		private int DamageFlashTicksLeft => lastDamageTick + 16 - Find.TickManager.TicksGame;

		public bool FlashingNowOrRecently => DamageFlashTicksLeft >= -1;

		public DamageFlasher(Pawn pawn)
		{
		}

		public Material GetDamagedMat(Material baseMat)
		{
			return DamagedMatPool.GetDamageFlashMat(baseMat, (float)DamageFlashTicksLeft / 16f);
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (dinfo.Def.harmsHealth)
			{
				lastDamageTick = Find.TickManager.TicksGame;
			}
		}
	}
}
