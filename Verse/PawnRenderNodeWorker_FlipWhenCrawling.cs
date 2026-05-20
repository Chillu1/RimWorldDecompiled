using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_FlipWhenCrawling : PawnRenderNodeWorker
{
	protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
	{
		if (parms.flipHead)
		{
			parms.facing = parms.facing.Opposite;
		}
		return base.GetMaterial(node, parms);
	}

	public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
	{
		if (parms.flipHead)
		{
			parms.facing = parms.facing.Opposite;
		}
		return base.LayerFor(node, parms);
	}
}
