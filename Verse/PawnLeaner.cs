using UnityEngine;

namespace Verse;

public class PawnLeaner
{
	private readonly Pawn pawn;

	private IntVec3 shootSourceOffset = new IntVec3(0, 0, 0);

	private float leanOffsetCurPct;

	private const float LeanOffsetPctChangeRate = 0.075f;

	private const float LeanOffsetDistanceMultiplier = 0.5f;

	public Vector3 LeanOffset => shootSourceOffset.ToVector3() * 0.5f * leanOffsetCurPct;

	public PawnLeaner(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (ShouldLean())
		{
			leanOffsetCurPct += 0.075f * (float)ticksPassed;
			if (leanOffsetCurPct > 1f)
			{
				leanOffsetCurPct = 1f;
			}
		}
		else
		{
			leanOffsetCurPct -= 0.075f * (float)ticksPassed;
			if (leanOffsetCurPct < 0f)
			{
				leanOffsetCurPct = 0f;
			}
		}
	}

	public bool ShouldLean()
	{
		if (!(pawn.stances.curStance is Stance_Busy))
		{
			return false;
		}
		if (shootSourceOffset == new IntVec3(0, 0, 0))
		{
			return false;
		}
		return true;
	}

	public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 shootPosition)
	{
		shootSourceOffset = newShootLine.Source - pawn.Position;
	}
}
