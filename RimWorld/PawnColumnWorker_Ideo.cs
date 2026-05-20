using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_Ideo : PawnColumnWorker_Icon
{
	public override bool VisibleCurrently
	{
		get
		{
			if (ModsConfig.IdeologyActive)
			{
				return !Find.IdeoManager.classicMode;
			}
			return false;
		}
	}

	protected override Texture2D GetIconFor(Pawn pawn)
	{
		if (Find.IdeoManager.classicMode)
		{
			return null;
		}
		return pawn.Ideo?.Icon;
	}

	protected override Color GetIconColor(Pawn pawn)
	{
		if (pawn.Ideo == null)
		{
			return Color.white;
		}
		return pawn.Ideo.Color;
	}

	protected override string GetIconTip(Pawn pawn)
	{
		if (pawn.Ideo != null)
		{
			return pawn.Ideo.name + "\n\n" + "ClickForMoreInfo".Translate();
		}
		return null;
	}

	protected override void ClickedIcon(Pawn pawn)
	{
		if (pawn.Ideo != null)
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Ideos);
			IdeoUIUtility.OpenIdeoInfo(pawn.Ideo);
		}
	}
}
