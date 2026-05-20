using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_HediffEye : PawnRenderNodeWorker_Eye
{
	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 result = base.OffsetFor(node, parms, out pivot);
		if (TryGetWoundAnchor(node.bodyPart?.woundAnchorTag, parms, out var anchor))
		{
			PawnDrawUtility.CalcAnchorData(parms.pawn, anchor, parms.facing, out var anchorOffset, out var _);
			result += anchorOffset;
		}
		return result;
	}
}
