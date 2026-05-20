using UnityEngine;

namespace Verse;

public static class AnimationUtility
{
	public static Vector3 AdjustOffsetForRotationMode(RotationMode mode, Vector3 offset, Pawn pawn)
	{
		switch (mode)
		{
		case RotationMode.None:
			return offset;
		case RotationMode.OneD:
			if (!(pawn.Rotation == Rot4.South))
			{
				return offset;
			}
			return offset * -1f;
		case RotationMode.TwoD:
			return offset.RotatedBy(pawn.Rotation);
		case RotationMode.PawnAimTarget:
			if (pawn.TargetCurrentlyAimingAt.IsValid)
			{
				return offset.RotatedBy((pawn.TargetCurrentlyAimingAt.CenterVector3 - pawn.Position.ToVector3()).ToAngleFlat());
			}
			break;
		}
		return offset;
	}
}
