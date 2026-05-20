using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Head : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			return !parms.flags.FlagSet(PawnRenderFlags.HeadStump);
		}
		return false;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 result = base.OffsetFor(node, parms, out pivot) + parms.pawn.Drawer.renderer.BaseHeadOffsetAt(parms.facing);
		if (parms.pawn.story.headType.narrow && node.Props.narrowCrownHorizontalOffset != 0f && parms.facing.IsHorizontal)
		{
			if (parms.facing == Rot4.East)
			{
				result.x -= node.Props.narrowCrownHorizontalOffset;
			}
			else if (parms.facing == Rot4.West)
			{
				result.x += node.Props.narrowCrownHorizontalOffset;
			}
			result.z -= node.Props.narrowCrownHorizontalOffset;
		}
		if (!parms.Portrait && parms.swimming)
		{
			result.z -= 0.5f;
		}
		return result;
	}

	public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Quaternion result = base.RotationFor(node, parms);
		if (!parms.Portrait && parms.pawn.Crawling)
		{
			result *= PawnRenderUtility.CrawlingHeadAngle(parms.facing).ToQuat();
			if (parms.flipHead)
			{
				result *= 180f.ToQuat();
			}
		}
		if (parms.pawn.IsShambler && parms.pawn.mutant != null && parms.pawn.mutant.HasTurned && !parms.pawn.Dead)
		{
			result *= Quaternion.Euler(Vector3.up * ((parms.pawn.mutant.Hediff as Hediff_Shambler)?.headRotation ?? 0f));
		}
		return result;
	}
}
