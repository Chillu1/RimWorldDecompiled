using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Infestation : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<int> hivesCount;

	public SlateRef<string> tag;

	public SlateRef<string> customLetterLabel;

	public SlateRef<string> customLetterText;

	public SlateRef<RulePack> customLetterLabelRules;

	public SlateRef<RulePack> customLetterTextRules;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		if (!slate.Exists("map"))
		{
			return false;
		}
		if (Faction.OfInsects == null)
		{
			return false;
		}
		Map map = slate.Get<Map>("map");
		if (!InfestationCellFinder.TryFindCell(out var _, map))
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map map = QuestGen.slate.Get<Map>("map");
		if (map == null)
		{
			return;
		}
		QuestPart_Infestation questPart = new QuestPart_Infestation();
		questPart.mapParent = map.Parent;
		questPart.hivesCount = hivesCount.GetValue(slate);
		questPart.tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
		if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.customLetterLabel = x;
			}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
		}
		if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				questPart.customLetterText = x;
			}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
		}
		questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		QuestGen.quest.AddPart(questPart);
	}
}
