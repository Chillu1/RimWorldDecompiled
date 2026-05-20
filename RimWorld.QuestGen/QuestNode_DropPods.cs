using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_DropPods : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Thing>> contents;

	public SlateRef<IEnumerable<ThingDefCountClass>> contentsDefs;

	public SlateRef<IntVec3?> dropSpot;

	public SlateRef<bool> useTradeDropSpot;

	public SlateRef<bool> joinPlayer;

	public SlateRef<bool> makePrisoners;

	public SlateRef<bool?> sendStandardLetter;

	public SlateRef<string> customLetterLabel;

	public SlateRef<string> customLetterText;

	public SlateRef<RulePack> customLetterLabelRules;

	public SlateRef<RulePack> customLetterTextRules;

	public SlateRef<IEnumerable<Thing>> thingsToExcludeFromHyperlinks;

	public SlateRef<bool> canRetargetAnyMap;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		return slate.Exists("map");
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (contents.GetValue(slate).EnumerableNullOrEmpty() && contentsDefs.GetValue(slate).EnumerableNullOrEmpty())
		{
			return;
		}
		QuestPart_DropPods dropPods = new QuestPart_DropPods();
		dropPods.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				dropPods.customLetterLabel = x;
			}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
		}
		if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				dropPods.customLetterText = x;
			}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
		}
		dropPods.sendStandardLetter = sendStandardLetter.GetValue(slate) ?? dropPods.sendStandardLetter;
		dropPods.useTradeDropSpot = useTradeDropSpot.GetValue(slate);
		dropPods.dropSpot = dropSpot.GetValue(slate) ?? IntVec3.Invalid;
		dropPods.joinPlayer = joinPlayer.GetValue(slate);
		dropPods.makePrisoners = makePrisoners.GetValue(slate);
		dropPods.mapParent = QuestGen.slate.Get<Map>("map").Parent;
		dropPods.Things = contents.GetValue(slate);
		dropPods.canRetargetAnyMap = canRetargetAnyMap.GetValue(slate);
		if (contentsDefs.GetValue(slate) != null)
		{
			dropPods.thingDefs.AddRange(contentsDefs.GetValue(slate));
		}
		if (thingsToExcludeFromHyperlinks.GetValue(slate) != null)
		{
			dropPods.thingsToExcludeFromHyperlinks.AddRange(from t in thingsToExcludeFromHyperlinks.GetValue(slate)
				select t.GetInnerIfMinified().def);
		}
		QuestGen.quest.AddPart(dropPods);
	}
}
