using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_AnimalBody : PawnRenderNodeWorker
{
	protected override GraphicStateDef GetGraphicState(PawnRenderNode node, PawnDrawParms parms)
	{
		if (node.tree.currentAnimation == null && parms.pawn.DrawNonHumanlikeSwimmingGraphic)
		{
			return GraphicStateDefOf.Swimming;
		}
		if (node.tree.currentAnimation == null && parms.pawn.DrawNonHumanlikeStationaryGraphic)
		{
			return GraphicStateDefOf.Stationary;
		}
		return base.GetGraphicState(node, parms);
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		return base.OffsetFor(node, parms, out pivot) + node.PrimaryGraphic.DrawOffset(parms.facing);
	}
}
