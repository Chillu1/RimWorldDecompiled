namespace Verse;

public class PawnRenderNode_Beard : PawnRenderNode
{
	public PawnRenderNode_Beard(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (pawn.style?.beardDef == null || pawn.style.beardDef.noGraphic)
		{
			return null;
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (pawn.style?.beardDef == null || pawn.style.beardDef.noGraphic)
		{
			return null;
		}
		return pawn.style.beardDef.GraphicFor(pawn, ColorFor(pawn));
	}
}
