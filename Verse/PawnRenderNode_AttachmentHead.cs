namespace Verse;

public class PawnRenderNode_AttachmentHead : PawnRenderNode
{
	public PawnRenderNode_AttachmentHead(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		return HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
	}
}
