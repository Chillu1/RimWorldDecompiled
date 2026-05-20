using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnRenderNode_AnimalPart_Body : PawnRenderNode_AnimalPart
{
	public PawnRenderNode_AnimalPart_Body(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
		: base(pawn, props, tree)
	{
	}

	protected override IEnumerable<(GraphicStateDef state, Graphic graphic)> StateGraphicsFor(Pawn pawn)
	{
		foreach (var item in base.StateGraphicsFor(pawn))
		{
			yield return item;
		}
		(GraphicStateDef, Graphic)? swimmingGraphic = GetSwimmingGraphic(pawn);
		if (swimmingGraphic.HasValue)
		{
			yield return swimmingGraphic.Value;
		}
		(GraphicStateDef, Graphic)? stationaryGraphic = GetStationaryGraphic(pawn);
		if (stationaryGraphic.HasValue)
		{
			yield return stationaryGraphic.Value;
		}
	}

	private (GraphicStateDef state, Graphic graphic)? GetSwimmingGraphic(Pawn pawn)
	{
		PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
		if (curKindLifeStage.swimmingGraphicData == null)
		{
			return null;
		}
		Graphic graphic = curKindLifeStage.swimmingGraphicData.Graphic;
		if (pawn.gender == Gender.Female && curKindLifeStage.femaleSwimmingGraphicData != null)
		{
			graphic = curKindLifeStage.femaleSwimmingGraphicData.Graphic;
		}
		if (pawn.TryGetAlternate(out var ag, out var _))
		{
			graphic = ag.GetSwimmingGraphic(graphic);
		}
		Color baseColor = graphic.Color;
		Color baseColor2 = graphic.ColorTwo;
		if (pawn.IsMutant)
		{
			baseColor = MutantUtility.GetMutantSkinColor(pawn, baseColor);
			baseColor2 = MutantUtility.GetMutantSkinColor(pawn, baseColor2);
		}
		baseColor = pawn.health.hediffSet.GetSkinColor(baseColor);
		baseColor2 = pawn.health.hediffSet.GetSkinColor(baseColor2);
		graphic = graphic.GetColoredVersion(graphic.Shader, baseColor, baseColor2);
		return (GraphicStateDefOf.Swimming, graphic);
	}

	private (GraphicStateDef state, Graphic graphic)? GetStationaryGraphic(Pawn pawn)
	{
		PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
		if (curKindLifeStage.stationaryGraphicData == null)
		{
			return null;
		}
		Graphic graphic = curKindLifeStage.stationaryGraphicData.Graphic;
		if (pawn.gender == Gender.Female && curKindLifeStage.femaleStationaryGraphicData != null)
		{
			graphic = curKindLifeStage.femaleStationaryGraphicData.Graphic;
		}
		if (pawn.TryGetAlternate(out var ag, out var _))
		{
			graphic = ag.GetStationaryGraphic(graphic);
		}
		Color baseColor = graphic.Color;
		Color baseColor2 = graphic.ColorTwo;
		if (pawn.IsMutant)
		{
			baseColor = MutantUtility.GetMutantSkinColor(pawn, baseColor);
			baseColor2 = MutantUtility.GetMutantSkinColor(pawn, baseColor2);
		}
		baseColor = pawn.health.hediffSet.GetSkinColor(baseColor);
		baseColor2 = pawn.health.hediffSet.GetSkinColor(baseColor2);
		graphic = graphic.GetColoredVersion(graphic.Shader, baseColor, baseColor2);
		return (GraphicStateDefOf.Stationary, graphic);
	}
}
