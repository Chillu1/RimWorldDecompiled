using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Hediff : PawnRenderNodeWorker
{
	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 anchorOffset = Vector3.zero;
		if (node.Props.drawData != null && node.hediff != null && node.Props.drawData.useBodyPartAnchor)
		{
			foreach (BodyTypeDef.WoundAnchor item in PawnDrawUtility.FindAnchors(parms.pawn, node.hediff.Part))
			{
				if (PawnDrawUtility.AnchorUsable(parms.pawn, item, parms.facing))
				{
					PawnDrawUtility.CalcAnchorData(parms.pawn, item, parms.facing, out anchorOffset, out var _);
				}
			}
		}
		anchorOffset += base.OffsetFor(node, parms, out pivot);
		DrawData drawData = node.Props.drawData;
		if (drawData != null && !drawData.useBodyPartAnchor && node.hediff?.Part?.flipGraphic == true)
		{
			anchorOffset.x *= -1f;
		}
		return anchorOffset;
	}
}
