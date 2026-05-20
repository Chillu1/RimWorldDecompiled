using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_EndGame : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

	public SlateRef<string> introText;

	public SlateRef<string> endingText;

	public SlateRef<RulePack> introTextRules;

	public SlateRef<RulePack> endingTextRules;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_EndGame endGame = new QuestPart_EndGame();
		endGame.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
		endGame.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			endGame.introText = x;
		}, QuestGenUtility.MergeRules(introTextRules.GetValue(slate), introText.GetValue(slate), "root"));
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			endGame.endingText = x;
		}, QuestGenUtility.MergeRules(endingTextRules.GetValue(slate), endingText.GetValue(slate), "root"));
		QuestGen.quest.AddPart(endGame);
	}
}
