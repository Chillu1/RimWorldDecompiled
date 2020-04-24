using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectStorytellerInGame : Page
	{
		private Listing_Standard selectedStorytellerInfoListing = new Listing_Standard();

		public override string PageTitle => "ChooseAIStoryteller".Translate();

		public Page_SelectStorytellerInGame()
		{
			doCloseButton = true;
			doCloseX = true;
		}

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			Rect mainRect = GetMainRect(rect);
			Storyteller storyteller = Current.Game.storyteller;
			StorytellerDef def = Current.Game.storyteller.def;
			StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref storyteller.def, ref storyteller.difficulty, selectedStorytellerInfoListing);
			if (storyteller.def != def)
			{
				storyteller.Notify_DefChanged();
			}
		}
	}
}
