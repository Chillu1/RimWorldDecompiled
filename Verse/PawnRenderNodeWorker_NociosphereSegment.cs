using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNodeWorker_NociosphereSegment : PawnRenderNodeWorker
{
	private static readonly Color LineColor = new Color(0.89f, 0.21f, 0.13f);

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		Vector3 vector = base.OffsetFor(node, parms, out pivot);
		if (!(node.Props is PawnRenderNodeProperties_NociosphereSegment pawnRenderNodeProperties_NociosphereSegment))
		{
			return vector;
		}
		if (!parms.pawn.TryGetComp<CompNociosphere>(out var comp))
		{
			return vector;
		}
		vector += pawnRenderNodeProperties_NociosphereSegment.offset.ToVector3() / 6f;
		Vector3 vector2 = vector * comp.segScale;
		return new Vector3(vector2.x, 0f, vector2.z);
	}

	public override MaterialPropertyBlock GetMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms)
	{
		MaterialPropertyBlock materialPropertyBlock = base.GetMaterialPropertyBlock(node, material, parms);
		if (!parms.pawn.TryGetComp<CompActivity>(out var comp))
		{
			return materialPropertyBlock;
		}
		Color lineColor = LineColor;
		lineColor.a = Mathf.Clamp01(comp.ActivityLevel);
		materialPropertyBlock.SetColor(ShaderPropertyIDs.ColorTwo, lineColor);
		return materialPropertyBlock;
	}
}
