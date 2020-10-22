using UnityEngine;

namespace RimWorld
{
	public class MainTabWindow_Factions : MainTabWindow
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		private Faction scrollToFaction;

		public override void PreOpen()
		{
			scrollToFaction = null;
		}

		public void ScrollToFaction(Faction faction)
		{
			scrollToFaction = faction;
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			FactionUIUtility.DoWindowContents_NewTemp(fillRect, ref scrollPosition, ref scrollViewHeight, scrollToFaction);
			if (scrollToFaction != null)
			{
				scrollToFaction = null;
			}
		}
	}
}
