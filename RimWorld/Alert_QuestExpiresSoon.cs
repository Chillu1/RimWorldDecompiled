using Verse;

namespace RimWorld
{
	public class Alert_QuestExpiresSoon : Alert
	{
		private const int TicksToAlert = 60000;

		private Quest QuestExpiring
		{
			get
			{
				foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
				{
					if (!item.dismissed && !item.Historical && !item.initiallyAccepted && item.State == QuestState.NotYetAccepted && item.ticksUntilAcceptanceExpiry > 0 && item.ticksUntilAcceptanceExpiry < 60000)
					{
						return item;
					}
				}
				return null;
			}
		}

		public Alert_QuestExpiresSoon()
		{
			defaultPriority = AlertPriority.High;
		}

		protected override void OnClick()
		{
			if (QuestExpiring != null)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(QuestExpiring);
			}
		}

		public override string GetLabel()
		{
			Quest questExpiring = QuestExpiring;
			if (questExpiring == null)
			{
				return string.Empty;
			}
			return "QuestExpiresSoon".Translate(questExpiring.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod());
		}

		public override TaggedString GetExplanation()
		{
			Quest questExpiring = QuestExpiring;
			if (questExpiring == null)
			{
				return string.Empty;
			}
			return "QuestExpiresSoonDesc".Translate(questExpiring.name, questExpiring.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
		}

		public override AlertReport GetReport()
		{
			return QuestExpiring != null;
		}
	}
}
