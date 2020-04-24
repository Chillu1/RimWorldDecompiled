using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Training : ITab
	{
		public override bool IsVisible
		{
			get
			{
				if (base.SelPawn.training != null)
				{
					return base.SelPawn.Faction == Faction.OfPlayer;
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
			TrainingCardUtility.DrawTrainingCard(rect, base.SelPawn);
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();
			size = new Vector2(300f, TrainingCardUtility.TotalHeightForPawn(base.SelPawn));
		}
	}
}
