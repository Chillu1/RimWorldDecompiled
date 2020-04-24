using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class NewQuestLetter : ChoiceLetter
	{
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (quest != null)
				{
					yield return Option_ViewInQuestsTab("ViewQuest");
				}
				if (lookTargets.IsValid())
				{
					yield return base.Option_JumpToLocation;
				}
				yield return base.Option_Close;
			}
		}

		public override void OpenLetter()
		{
			if (quest != null && !base.ArchivedOnly)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
				Find.LetterStack.RemoveLetter(this);
			}
			else
			{
				base.OpenLetter();
			}
		}
	}
}
