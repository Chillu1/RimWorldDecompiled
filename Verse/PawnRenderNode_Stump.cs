namespace Verse;

public class PawnRenderNode_Stump : PawnRenderNode
{
	public PawnRenderNode_Stump(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (!pawn.health.hediffSet.HasHead)
		{
			return base.GraphicFor(pawn);
		}
		return null;
	}
}
