using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public class PawnUIOverlay
{
	private Pawn pawn;

	private const float PawnLabelOffsetY = -0.6f;

	private const int PawnStatBarWidth = 32;

	private const float ActivityIconSize = 13f;

	private const float ActivityIconOffsetY = 12f;

	public PawnUIOverlay(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void DrawPawnGUIOverlay()
	{
		if (!pawn.Spawned || pawn.Map.fogGrid.IsFogged(pawn.Position) || WorldComponent_GravshipController.CutsceneInProgress)
		{
			return;
		}
		if (!pawn.RaceProps.Humanlike)
		{
			if (pawn.RaceProps.Animal)
			{
				if (!Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn))
				{
					return;
				}
			}
			else
			{
				if (!pawn.IsColonyMech)
				{
					return;
				}
				if (pawn.IsSelfShutdown())
				{
					pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.SelfShutdown);
				}
				if (!Prefs.MechNameMode.ShouldDisplayMechName(pawn))
				{
					return;
				}
			}
		}
		if (!pawn.IsMutant || !pawn.mutant.Def.hideLabel)
		{
			Vector2 pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);
			GenMapUI.DrawPawnLabel(pawn, pos);
			if (pawn.ShouldShowQuestionMark())
			{
				pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.CurLordToil != null)
			{
				lord.CurLordToil.DrawPawnGUIOverlay(pawn);
			}
		}
	}
}
