using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_Pawn_Training : ITab
{
	public override bool IsVisible
	{
		get
		{
			if (SelPawn.training != null && SelPawn.Faction == Faction.OfPlayer && !SelPawn.RaceProps.hideTrainingTab)
			{
				if (SelPawn.IsMutant)
				{
					return SelPawn.mutant.Def.tameable;
				}
				return true;
			}
			return false;
		}
	}

	public ITab_Pawn_Training()
	{
		labelKey = "TabTraining";
		tutorTag = "Training";
	}

	protected override void FillTab()
	{
		Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(17f);
		rect.yMin += 10f;
		TrainingCardUtility.DrawTrainingCard(rect, SelPawn);
	}

	protected override void UpdateSize()
	{
		base.UpdateSize();
		size = new Vector2(300f, TrainingCardUtility.TotalHeightForPawn(SelPawn));
	}
}
