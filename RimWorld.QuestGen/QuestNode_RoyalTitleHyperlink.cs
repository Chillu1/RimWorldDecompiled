namespace RimWorld.QuestGen;

public class QuestNode_RoyalTitleHyperlink : QuestNode
{
	public SlateRef<RoyalTitleDef> title;

	public SlateRef<FactionDef> faction;

	protected override void RunInt()
	{
		QuestPart_RoyalTitleHyperlink questPart_RoyalTitleHyperlink = new QuestPart_RoyalTitleHyperlink();
		questPart_RoyalTitleHyperlink.titleDef = title.GetValue(QuestGen.slate);
		questPart_RoyalTitleHyperlink.factionDef = faction.GetValue(QuestGen.slate);
		QuestGen.quest.AddPart(questPart_RoyalTitleHyperlink);
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
