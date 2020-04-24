using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Character : ITab
	{
		private Pawn PawnToShowInfoAbout
		{
			get
			{
				Pawn pawn = null;
				if (base.SelPawn != null)
				{
					pawn = base.SelPawn;
				}
				else
				{
					Corpse corpse = base.SelThing as Corpse;
					if (corpse != null)
					{
						pawn = corpse.InnerPawn;
					}
				}
				if (pawn == null)
				{
					Log.Error("Character tab found no selected pawn to display.");
					return null;
				}
				return pawn;
			}
		}

		public override bool IsVisible => PawnToShowInfoAbout.story != null;

		public ITab_Pawn_Character()
		{
			labelKey = "TabCharacter";
			tutorTag = "Character";
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();
			size = CharacterCardUtility.PawnCardSize(PawnToShowInfoAbout) + new Vector2(17f, 17f) * 2f;
		}

		protected override void FillTab()
		{
			UpdateSize();
			Vector2 vector = CharacterCardUtility.PawnCardSize(PawnToShowInfoAbout);
			CharacterCardUtility.DrawCharacterCard(new Rect(17f, 17f, vector.x, vector.y), PawnToShowInfoAbout);
		}
	}
}
