using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Faction : PawnColumnWorker_Icon
	{
		protected override Texture2D GetIconFor(Pawn pawn)
		{
			Faction factionOrExtraMiniOrHomeFaction = pawn.FactionOrExtraMiniOrHomeFaction;
			if (factionOrExtraMiniOrHomeFaction != null && factionOrExtraMiniOrHomeFaction != Faction.OfPlayer)
			{
				return factionOrExtraMiniOrHomeFaction.def.FactionIcon;
			}
			return null;
		}

		protected override Color GetIconColor(Pawn pawn)
		{
			Faction factionOrExtraMiniOrHomeFaction = pawn.FactionOrExtraMiniOrHomeFaction;
			if (factionOrExtraMiniOrHomeFaction != null && factionOrExtraMiniOrHomeFaction != Faction.OfPlayer)
			{
				return factionOrExtraMiniOrHomeFaction.Color;
			}
			return Color.white;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			string text = pawn.FactionOrExtraMiniOrHomeFaction?.Name;
			if (!text.NullOrEmpty())
			{
				return "PawnFactionInfo".Translate(text, pawn);
			}
			return null;
		}

		protected override void ClickedIcon(Pawn pawn)
		{
			Faction factionOrExtraMiniOrHomeFaction = pawn.FactionOrExtraMiniOrHomeFaction;
			if (factionOrExtraMiniOrHomeFaction != null)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
				((MainTabWindow_Factions)Find.MainTabsRoot.OpenTab.TabWindow).ScrollToFaction(factionOrExtraMiniOrHomeFaction);
			}
		}
	}
}
