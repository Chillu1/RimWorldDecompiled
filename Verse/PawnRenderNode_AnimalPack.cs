using UnityEngine;

namespace Verse;

public class PawnRenderNode_AnimalPack : PawnRenderNode_AnimalPart
{
	public PawnRenderNode_AnimalPack(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		if (!pawn.RaceProps.packAnimal)
		{
			return null;
		}
		PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
		Graphic graphic = ((pawn.gender == Gender.Female && curKindLifeStage.femaleGraphicData != null) ? curKindLifeStage.femaleGraphicData.Graphic : curKindLifeStage.bodyGraphicData.Graphic);
		return GraphicDatabase.Get<Graphic_Multi>(graphic.path + "Pack", ShaderDatabase.Cutout, graphic.drawSize, Color.white);
	}
}
