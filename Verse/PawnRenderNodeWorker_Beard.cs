using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Beard : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (base.CanDrawNow(node, parms))
		{
			if (!(parms.facing != Rot4.North))
			{
				return parms.flipHead;
			}
			return true;
		}
		return false;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		HeadTypeDef headType = parms.pawn.story.headType;
		Vector3 vector = base.OffsetFor(node, parms, out pivot);
		if (parms.facing == Rot4.East)
		{
			vector += Vector3.right * headType.beardOffsetXEast;
		}
		else if (parms.facing == Rot4.West)
		{
			vector += Vector3.left * headType.beardOffsetXEast;
		}
		return vector + (headType.beardOffset + parms.pawn.style.beardDef.GetOffset(headType, parms.facing));
	}
}
