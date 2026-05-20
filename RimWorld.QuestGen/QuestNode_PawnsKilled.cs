using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_PawnsKilled : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	[NoTranslate]
	public SlateRef<string> outSignalPawnsNotAvailable;

	public SlateRef<ThingDef> race;

	public SlateRef<int> count;

	public QuestNode node;

	private const string PawnOfRaceKilledSignal = "PawnOfRaceKilled";

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		if (node != null)
		{
			return node.TestRun(slate);
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		MapParent parent = slate.Get<Map>("map").Parent;
		string text = QuestGen.GenerateNewSignal("PawnOfRaceKilled");
		QuestPart_PawnsKilled questPart_PawnsKilled = new QuestPart_PawnsKilled();
		questPart_PawnsKilled.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_PawnsKilled.race = race.GetValue(slate);
		questPart_PawnsKilled.requiredInstigatorFaction = Faction.OfPlayer;
		questPart_PawnsKilled.count = count.GetValue(slate);
		questPart_PawnsKilled.mapParent = parent;
		questPart_PawnsKilled.outSignalPawnKilled = text;
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_PawnsKilled);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_PawnsKilled.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_PawnsKilled);
		QuestPart_PawnsAvailable questPart_PawnsAvailable = new QuestPart_PawnsAvailable();
		questPart_PawnsAvailable.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		if (!outSignalPawnsNotAvailable.GetValue(slate).NullOrEmpty())
		{
			questPart_PawnsAvailable.outSignalPawnsNotAvailable = QuestGenUtility.HardcodedSignalWithQuestID(outSignalPawnsNotAvailable.GetValue(slate));
		}
		questPart_PawnsAvailable.race = race.GetValue(slate);
		questPart_PawnsAvailable.requiredCount = count.GetValue(slate);
		questPart_PawnsAvailable.mapParent = parent;
		questPart_PawnsAvailable.inSignalDecrement = text;
		QuestGen.quest.AddPart(questPart_PawnsAvailable);
	}
}
