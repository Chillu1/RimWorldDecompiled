using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_Eye : PawnRenderNodeWorker_FlipWhenCrawling
{
	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 result = base.OffsetFor(node, parms, out pivot);
		if (TryGetWoundAnchor(node.Props.anchorTag, parms, out var anchor))
		{
			PawnDrawUtility.CalcAnchorData(parms.pawn, anchor, parms.facing, out var anchorOffset, out var _);
			result += anchorOffset;
		}
		return result;
	}

	public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
	{
		return base.ScaleFor(node, parms) * (parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor ?? 1f);
	}

	protected bool TryGetWoundAnchor(string anchorTag, PawnDrawParms parms, out BodyTypeDef.WoundAnchor anchor)
	{
		anchor = null;
		if (anchorTag.NullOrEmpty())
		{
			return false;
		}
		List<BodyTypeDef.WoundAnchor> woundAnchors = parms.pawn.story.bodyType.woundAnchors;
		for (int i = 0; i < woundAnchors.Count; i++)
		{
			BodyTypeDef.WoundAnchor woundAnchor = woundAnchors[i];
			if (woundAnchor.tag == anchorTag)
			{
				Rot4? rotation = woundAnchor.rotation;
				Rot4 facing = parms.facing;
				if (rotation.HasValue && (!rotation.HasValue || rotation.GetValueOrDefault() == facing) && (parms.facing == Rot4.South || woundAnchor.narrowCrown == true == parms.pawn.story.headType.narrow))
				{
					anchor = woundAnchor;
					return true;
				}
			}
		}
		return false;
	}
}
