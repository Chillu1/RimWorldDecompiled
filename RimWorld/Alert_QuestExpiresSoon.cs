using Verse;

namespace RimWorld;

public class Alert_QuestExpiresSoon : Alert
{
	private Quest questExpiring;

	private const int TicksToAlert = 60000;

	private Quest QuestExpiring
	{
		get
		{
			int num = int.MaxValue;
			questExpiring = null;
			foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
			{
				if (!item.dismissed && !item.Historical && !item.initiallyAccepted && item.State == QuestState.NotYetAccepted && item.TicksUntilExpiry > 0 && item.TicksUntilExpiry < 60000 && item.TicksUntilExpiry < num)
				{
					questExpiring = item;
					num = item.TicksUntilExpiry;
				}
			}
			return questExpiring;
		}
	}

	public Alert_QuestExpiresSoon()
	{
		defaultPriority = AlertPriority.High;
	}

	protected override void OnClick()
	{
		if (questExpiring != null)
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
			((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(questExpiring);
		}
	}

	public override string GetLabel()
	{
		if (questExpiring == null)
		{
			return string.Empty;
		}
		return "QuestExpiresSoon".Translate(questExpiring.TicksUntilExpiry.ToStringTicksToPeriodVerbose());
	}

	public override TaggedString GetExplanation()
	{
		if (questExpiring == null)
		{
			return string.Empty;
		}
		return "QuestExpiresSoonDesc".Translate(questExpiring.name, questExpiring.TicksUntilExpiry.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor));
	}

	public override AlertReport GetReport()
	{
		return QuestExpiring != null;
	}
}
