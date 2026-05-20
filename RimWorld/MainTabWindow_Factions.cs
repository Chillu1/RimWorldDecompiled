using UnityEngine;

namespace RimWorld;

public class MainTabWindow_Factions : MainTabWindow
{
	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private Faction scrollToFaction;

	public override void PreOpen()
	{
		base.PreOpen();
		scrollToFaction = null;
	}

	public void ScrollToFaction(Faction faction)
	{
		scrollToFaction = faction;
	}

	public override void DoWindowContents(Rect fillRect)
	{
		FactionUIUtility.DoWindowContents(fillRect, ref scrollPosition, ref scrollViewHeight, scrollToFaction);
		if (scrollToFaction != null)
		{
			scrollToFaction = null;
		}
	}
}
