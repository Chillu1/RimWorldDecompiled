using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Letter : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<Faction> relatedFaction;

	public SlateRef<LetterDef> letterDef;

	public SlateRef<string> label;

	public SlateRef<string> text;

	public SlateRef<RulePack> labelRules;

	public SlateRef<RulePack> textRules;

	public SlateRef<IEnumerable<object>> lookTargets;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

	[NoTranslate]
	public SlateRef<string> chosenPawnSignal;

	public SlateRef<MapParent> useColonistsOnMap;

	public SlateRef<bool> useColonistsFromCaravanArg;

	[NoTranslate]
	public SlateRef<string> acceptedVisitorsSignal;

	public SlateRef<List<Pawn>> visitors;

	public SlateRef<bool> filterDeadPawnsFromLookTargets;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_Letter questPart_Letter = new QuestPart_Letter();
		questPart_Letter.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
		LetterDef letterDef = this.letterDef.GetValue(slate) ?? LetterDefOf.NeutralEvent;
		if (typeof(ChoiceLetter).IsAssignableFrom(letterDef.letterClass))
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter("error", "error", letterDef, QuestGenUtility.ToLookTargets(lookTargets, slate), relatedFaction.GetValue(slate), QuestGen.quest);
			questPart_Letter.letter = choiceLetter;
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				choiceLetter.Label = x;
			}, QuestGenUtility.MergeRules(labelRules.GetValue(slate), label.GetValue(slate), "root"));
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				choiceLetter.Text = x;
			}, QuestGenUtility.MergeRules(textRules.GetValue(slate), text.GetValue(slate), "root"));
		}
		else
		{
			questPart_Letter.letter = LetterMaker.MakeLetter(letterDef);
			questPart_Letter.letter.lookTargets = QuestGenUtility.ToLookTargets(lookTargets, slate);
			questPart_Letter.letter.relatedFaction = relatedFaction.GetValue(slate);
		}
		questPart_Letter.chosenPawnSignal = QuestGenUtility.HardcodedSignalWithQuestID(chosenPawnSignal.GetValue(slate));
		questPart_Letter.useColonistsOnMap = useColonistsOnMap.GetValue(slate);
		questPart_Letter.useColonistsFromCaravanArg = useColonistsFromCaravanArg.GetValue(slate);
		questPart_Letter.acceptedVisitorsSignal = QuestGenUtility.HardcodedSignalWithQuestID(acceptedVisitorsSignal.GetValue(slate));
		questPart_Letter.visitors = visitors.GetValue(slate);
		questPart_Letter.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		questPart_Letter.filterDeadPawnsFromLookTargets = filterDeadPawnsFromLookTargets.GetValue(slate);
		QuestGen.quest.AddPart(questPart_Letter);
	}
}
