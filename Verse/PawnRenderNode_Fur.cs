using UnityEngine;

namespace Verse;

public class PawnRenderNode_Fur : PawnRenderNode
{
	protected override Shader DefaultShader => ShaderDatabase.CutoutSkinOverlay;

	public PawnRenderNode_Fur(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (!ModLister.CheckBiotech("Fur"))
		{
			return null;
		}
		if (pawn.story?.furDef == null)
		{
			return null;
		}
		return GraphicDatabase.Get<Graphic_Multi>(pawn.story?.furDef.GetFurBodyGraphicPath(pawn), ShaderFor(pawn), Vector2.one, ColorFor(pawn));
	}

	public override Color ColorFor(Pawn pawn)
	{
		return pawn.story.HairColor;
	}
}
