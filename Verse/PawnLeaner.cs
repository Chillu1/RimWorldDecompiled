using UnityEngine;

namespace Verse
{
	public class PawnLeaner
	{
		private Pawn pawn;

		private IntVec3 shootSourceOffset = new IntVec3(0, 0, 0);

		private float leanOffsetCurPct;

		private const float LeanOffsetPctChangeRate = 0.075f;

		private const float LeanOffsetDistanceMultiplier = 0.5f;

		public Vector3 LeanOffset => shootSourceOffset.ToVector3() * 0.5f * leanOffsetCurPct;

		public PawnLeaner(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void LeanerTick()
		{
			if (ShouldLean())
			{
				leanOffsetCurPct += 0.075f;
				if (leanOffsetCurPct > 1f)
				{
					leanOffsetCurPct = 1f;
				}
			}
			else
			{
				leanOffsetCurPct -= 0.075f;
				if (leanOffsetCurPct < 0f)
				{
					leanOffsetCurPct = 0f;
				}
			}
		}

		public bool ShouldLean()
		{
			if (pawn.stances.curStance is Stance_Busy)
			{
				if (shootSourceOffset == new IntVec3(0, 0, 0))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
		{
			shootSourceOffset = newShootLine.Source - pawn.Position;
		}
	}
}
