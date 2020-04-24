using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_Naked : ScenPart_PawnModifier
	{
		public override string Summary(Scenario scen)
		{
			return "ScenPart_PawnsAreNaked".Translate(context.ToStringHuman()).CapitalizeFirst();
		}

		protected override void ModifyPawnPostGenerate(Pawn pawn, bool redressed)
		{
			if (pawn.apparel != null)
			{
				pawn.apparel.DestroyAll();
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
		}
	}
}
