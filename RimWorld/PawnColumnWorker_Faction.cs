using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_Faction : PawnColumnWorker_Icon
{
	protected override Texture2D GetIconFor(Pawn pawn)
	{
		Faction homeFaction = pawn.HomeFaction;
		if (homeFaction != null && homeFaction != Faction.OfPlayer)
		{
			return homeFaction.def.FactionIcon;
		}
		return null;
	}

	protected override Color GetIconColor(Pawn pawn)
	{
		Faction homeFaction = pawn.HomeFaction;
		if (homeFaction != null && homeFaction != Faction.OfPlayer)
		{
			return homeFaction.Color;
		}
		return Color.white;
	}

	protected override string GetIconTip(Pawn pawn)
	{
		string text = pawn.HomeFaction?.Name;
		if (!text.NullOrEmpty())
		{
			return "PawnFactionInfo".Translate(text, pawn);
		}
		return null;
	}

	protected override void ClickedIcon(Pawn pawn)
	{
		Faction homeFaction = pawn.HomeFaction;
		if (homeFaction != null)
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
			((MainTabWindow_Factions)Find.MainTabsRoot.OpenTab.TabWindow).ScrollToFaction(homeFaction);
		}
	}
}
