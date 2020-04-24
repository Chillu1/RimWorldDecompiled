using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectStoryteller : Page
	{
		private StorytellerDef storyteller;

		private DifficultyDef difficulty;

		private Listing_Standard selectedStorytellerInfoListing = new Listing_Standard();

		public override string PageTitle => "ChooseAIStoryteller".Translate();

		public override void PreOpen()
		{
			base.PreOpen();
			if (storyteller == null)
			{
				storyteller = (from d in DefDatabase<StorytellerDef>.AllDefs
					where d.listVisible
					orderby d.listOrder
					select d).First();
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			StorytellerUI.DrawStorytellerSelectionInterface(GetMainRect(rect), ref storyteller, ref difficulty, selectedStorytellerInfoListing);
			string midLabel = null;
			Action midAct = null;
			if (!Prefs.ExtremeDifficultyUnlocked)
			{
				midLabel = "UnlockExtremeDifficulty".Translate();
				midAct = delegate
				{
					OpenDifficultyUnlockConfirmation();
				};
			}
			DoBottomButtons(rect, null, midLabel, midAct);
			Rect rect2 = new Rect(rect.xMax - Page.BottomButSize.x - 200f - 6f, rect.yMax - Page.BottomButSize.y, 200f, Page.BottomButSize.y);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect2, "CanChangeStorytellerSettingsDuringPlay".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void OpenDifficultyUnlockConfirmation()
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnlockExtremeDifficulty".Translate(), delegate
			{
				Prefs.ExtremeDifficultyUnlocked = true;
				Prefs.Save();
			}, destructive: true));
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (difficulty == null)
			{
				if (!Prefs.DevMode)
				{
					Messages.Message("MustChooseDifficulty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				Messages.Message("Difficulty has been automatically selected (debug mode only)", MessageTypeDefOf.SilentInput, historical: false);
				difficulty = DifficultyDefOf.Rough;
			}
			if (!Find.GameInitData.permadeathChosen)
			{
				if (!Prefs.DevMode)
				{
					Messages.Message("MustChoosePermadeath".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				Messages.Message("Reload anytime mode has been automatically selected (debug mode only)", MessageTypeDefOf.SilentInput, historical: false);
				Find.GameInitData.permadeathChosen = true;
				Find.GameInitData.permadeath = false;
			}
			Current.Game.storyteller = new Storyteller(storyteller, difficulty);
			return true;
		}
	}
}
