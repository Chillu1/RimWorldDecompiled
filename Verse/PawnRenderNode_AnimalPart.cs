using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNode_AnimalPart : PawnRenderNode
{
	public PawnRenderNode_AnimalPart(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	public override GraphicMeshSet MeshSetFor(Pawn pawn)
	{
		Graphic graphic = GraphicFor(pawn);
		if (graphic != null)
		{
			return MeshPool.GetMeshSetForSize(graphic.drawSize.x, graphic.drawSize.y);
		}
		return null;
	}

	public override Graphic GraphicFor(Pawn pawn)
	{
		PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
		AlternateGraphic ag;
		int index;
		Graphic graphic = (pawn.TryGetAlternate(out ag, out index) ? ag.GetGraphic(curKindLifeStage.bodyGraphicData.Graphic) : ((pawn.gender == Gender.Female && curKindLifeStage.femaleGraphicData != null) ? curKindLifeStage.femaleGraphicData.Graphic : curKindLifeStage.bodyGraphicData.Graphic));
		if ((pawn.Dead || (pawn.IsMutant && pawn.mutant.Def.useCorpseGraphics)) && curKindLifeStage.corpseGraphicData != null)
		{
			graphic = ((pawn.gender == Gender.Female && curKindLifeStage.femaleCorpseGraphicData != null) ? curKindLifeStage.femaleCorpseGraphicData.Graphic.GetColoredVersion(curKindLifeStage.femaleCorpseGraphicData.Graphic.Shader, graphic.Color, graphic.ColorTwo) : curKindLifeStage.corpseGraphicData.Graphic.GetColoredVersion(curKindLifeStage.corpseGraphicData.Graphic.Shader, graphic.Color, graphic.ColorTwo));
		}
		Color baseColor = graphic.Color;
		Color baseColor2 = graphic.ColorTwo;
		switch (pawn.Drawer.renderer.CurRotDrawMode)
		{
		case RotDrawMode.Fresh:
			if (pawn.IsMutant)
			{
				baseColor = MutantUtility.GetMutantSkinColor(pawn, baseColor);
				baseColor2 = MutantUtility.GetMutantSkinColor(pawn, baseColor2);
			}
			baseColor = pawn.health.hediffSet.GetSkinColor(baseColor);
			baseColor2 = pawn.health.hediffSet.GetSkinColor(baseColor2);
			return graphic.GetColoredVersion(graphic.Shader, baseColor, baseColor2);
		case RotDrawMode.Rotting:
		{
			baseColor = pawn.health.hediffSet.GetSkinColor(baseColor);
			baseColor2 = pawn.health.hediffSet.GetSkinColor(baseColor2);
			baseColor = PawnRenderUtility.GetRottenColor(baseColor);
			baseColor2 = PawnRenderUtility.GetRottenColor(baseColor2);
			Graphic graphic3 = ((ag != null) ? ag.GetRottingGraphic(graphic) : ((curKindLifeStage.femaleRottingGraphicData != null && pawn.gender == Gender.Female) ? curKindLifeStage.femaleRottingGraphicData.Graphic : ((curKindLifeStage.rottingGraphicData == null) ? graphic : curKindLifeStage.rottingGraphicData.Graphic)));
			Shader newShader = graphic3.Shader;
			if (graphic3.Shader == ShaderDatabase.CutoutComplex)
			{
				newShader = ShaderDatabase.Cutout;
			}
			return graphic3.GetColoredVersion(newShader, baseColor, baseColor2);
		}
		case RotDrawMode.Dessicated:
			if (curKindLifeStage.dessicatedBodyGraphicData != null)
			{
				Graphic graphic2;
				if (pawn.RaceProps.FleshType != FleshTypeDefOf.Insectoid)
				{
					graphic2 = ((pawn.gender == Gender.Female && curKindLifeStage.femaleDessicatedBodyGraphicData != null) ? curKindLifeStage.femaleDessicatedBodyGraphicData.GraphicColoredFor(pawn) : curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn));
				}
				else
				{
					Color dessicatedColorInsect = PawnRenderUtility.DessicatedColorInsect;
					graphic2 = ((pawn.gender == Gender.Female && curKindLifeStage.femaleDessicatedBodyGraphicData != null) ? curKindLifeStage.femaleDessicatedBodyGraphicData.Graphic.GetColoredVersion(graphic.Shader, dessicatedColorInsect, dessicatedColorInsect) : curKindLifeStage.dessicatedBodyGraphicData.Graphic.GetColoredVersion(ShaderDatabase.Cutout, dessicatedColorInsect, dessicatedColorInsect));
				}
				if (pawn.IsShambler)
				{
					graphic2.ShadowGraphic = graphic.ShadowGraphic;
				}
				if (ag != null)
				{
					graphic2 = ag.GetDessicatedGraphic(graphic2);
				}
				return graphic2;
			}
			break;
		}
		return null;
	}
}
