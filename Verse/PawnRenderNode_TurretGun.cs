using RimWorld;

namespace Verse;

public class PawnRenderNode_TurretGun : PawnRenderNode
{
	public CompTurretGun turretComp;

	public PawnRenderNode_TurretGun(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		return GraphicDatabase.Get<Graphic_Single>(turretComp.Props.turretDef.graphicData.texPath, ShaderDatabase.Cutout);
	}
}
