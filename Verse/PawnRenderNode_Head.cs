using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNode_Head : PawnRenderNode
{
	public PawnRenderNode_Head(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (props.overrideMeshSize.HasValue)
		{
			return MeshPool.GetMeshSetForSize(props.overrideMeshSize.Value.x, props.overrideMeshSize.Value.y);
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (!pawn.health.hediffSet.HasHead)
		{
			return null;
		}
		if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
		{
			return HeadTypeDefOf.Skull.GetGraphic(pawn, Color.white);
		}
		return pawn.story?.headType?.GetGraphic(pawn, ColorFor(pawn));
	}
}
