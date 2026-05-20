using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNode_Devourer : PawnRenderNode_AnimalPart_Body
{
	public PawnRenderNode_Devourer(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (pawn.TryGetComp<CompDevourer>(out var comp) && comp.Digesting)
		{
			Graphic graphic = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
			return GraphicDatabase.Get<Graphic_Multi>(graphic.path + "_Closed", ShaderDatabase.Cutout, graphic.drawSize, Color.white);
		}
		return base.GraphicFor(pawn);
	}
}
