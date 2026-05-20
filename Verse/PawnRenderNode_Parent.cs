namespace Verse;

public class PawnRenderNode_Parent : PawnRenderNode
{
	public PawnRenderNode_Parent(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		return null;
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		return null;
	}
}
