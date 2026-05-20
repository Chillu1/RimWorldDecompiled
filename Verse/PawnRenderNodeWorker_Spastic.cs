using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Spastic : PawnRenderNodeWorker
{
	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 result = base.OffsetFor(node, parms, out pivot);
		if (node is PawnRenderNode_Spastic pawnRenderNode_Spastic && pawnRenderNode_Spastic.CheckAndDoSpasm(parms, out var dat, out var progress))
		{
			result += Vector3.Lerp(dat.offsetStart, dat.offsetTarget, progress);
		}
		return result;
	}

	public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Quaternion quaternion = base.RotationFor(node, parms);
		if (!(node is PawnRenderNode_Spastic pawnRenderNode_Spastic))
		{
			return quaternion;
		}
		float num = 0f;
		if (node.Props is PawnRenderNodeProperties_Spastic { rotateFacing: not false })
		{
			num += parms.facing.AsAngle;
		}
		if (pawnRenderNode_Spastic.CheckAndDoSpasm(parms, out var dat, out var progress))
		{
			num += Mathf.Lerp(dat.rotationStart, dat.rotationTarget, progress);
		}
		return quaternion * num.ToQuat();
	}

	public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Vector3 result = base.ScaleFor(node, parms);
		if (node is PawnRenderNode_Spastic pawnRenderNode_Spastic && pawnRenderNode_Spastic.CheckAndDoSpasm(parms, out var dat, out var progress))
		{
			result *= Mathf.Lerp(dat.scaleStart, dat.scaleTarget, progress);
			result.y = 1f;
		}
		return result;
	}
}
