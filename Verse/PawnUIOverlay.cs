using RimWorld;
using UnityEngine;

namespace Verse
{
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
			if (!pawn.Spawned || pawn.Map.fogGrid.IsFogged(pawn.Position))
			{
				return;
			}
			if (!pawn.RaceProps.Humanlike)
			{
				switch (Prefs.AnimalNameMode)
				{
				case AnimalNameDisplayMode.None:
					return;
				case AnimalNameDisplayMode.TameAll:
					if (pawn.Name == null)
					{
						return;
					}
					break;
				case AnimalNameDisplayMode.TameNamed:
					if (pawn.Name == null || pawn.Name.Numerical)
					{
						return;
					}
					break;
				}
			}
			Vector2 pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);
			GenMapUI.DrawPawnLabel(pawn, pos);
			if (pawn.CanTradeNow)
			{
				pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
			}
		}
	}
}
