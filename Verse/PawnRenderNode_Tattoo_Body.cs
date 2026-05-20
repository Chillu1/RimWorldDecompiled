namespace Verse;

public class PawnRenderNode_Tattoo_Body : PawnRenderNode_Tattoo
{
	public PawnRenderNode_Tattoo_Body(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		if (pawn.style?.BodyTattoo == null || pawn.style.BodyTattoo.noGraphic)
		{
			return null;
		}
		return HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (!ModLister.CheckIdeology("Body tattoo"))
		{
			return null;
		}
		if (pawn.style?.BodyTattoo == null || pawn.style.BodyTattoo.noGraphic)
		{
			return null;
		}
		return pawn.style.BodyTattoo.GraphicFor(pawn, ColorFor(pawn));
	}
}
