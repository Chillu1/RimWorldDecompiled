namespace Verse;

public class PawnRenderNode_NociosphereSegment : PawnRenderNode
{
	public new PawnRenderNodeProperties_NociosphereSegment Props => (PawnRenderNodeProperties_NociosphereSegment)props;

	public PawnRenderNode_NociosphereSegment(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		return GraphicDatabase.Get<Graphic_Single>(Props.texPath, ShaderDatabase.CutoutComplexBlend, Props.maskPath);
	}
}
