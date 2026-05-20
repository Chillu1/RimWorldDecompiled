namespace Verse;

public class PawnRenderNode_Hair : PawnRenderNode
{
	public PawnRenderNode_Hair(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (pawn.story?.hairDef == null || pawn.story.hairDef.noGraphic)
		{
			return null;
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (pawn.story?.hairDef == null || pawn.story.hairDef.noGraphic || pawn.DevelopmentalStage.Baby() || pawn.DevelopmentalStage.Newborn())
		{
			return null;
		}
		return pawn.story.hairDef.GraphicFor(pawn, ColorFor(pawn));
	}
}
