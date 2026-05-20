using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_GiveNearPawn : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<IEnumerable<Thing>> contents;

		public SlateRef<IEnumerable<ThingDefCountClass>> contentsDefs;

		public SlateRef<Pawn> nearPawn;

		public SlateRef<bool> joinPlayer;

		public SlateRef<bool> makePrisoners;

		public SlateRef<bool?> sendStandardLetter;

		public SlateRef<string> customDropPodsLetterLabel;

		public SlateRef<string> customDropPodsLetterText;

		public SlateRef<string> customCaravanInventoryLetterLabel;

		public SlateRef<string> customCaravanInventoryLetterText;

		public SlateRef<RulePack> customDropPodsLetterLabelRules;

		public SlateRef<RulePack> customDropPodsLetterTextRules;

		public SlateRef<RulePack> customCaravanInventoryLetterLabelRules;

		public SlateRef<RulePack> customCaravanInventoryLetterTextRules;

		private const string RootSymbol = "root";

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (contents.GetValue(slate).EnumerableNullOrEmpty() && contentsDefs.GetValue(slate).EnumerableNullOrEmpty())
			{
				return;
			}
			QuestPart_GiveNearPawn give = new QuestPart_GiveNearPawn();
			give.nearPawn = nearPawn.GetValue(slate);
			give.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
			if (!customDropPodsLetterLabel.GetValue(slate).NullOrEmpty() || customDropPodsLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					give.customDropPodsLetterLabel = x;
				}, QuestGenUtility.MergeRules(customDropPodsLetterLabelRules.GetValue(slate), customDropPodsLetterLabel.GetValue(slate), "root"));
			}
			if (!customDropPodsLetterText.GetValue(slate).NullOrEmpty() || customDropPodsLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					give.customDropPodsLetterText = x;
				}, QuestGenUtility.MergeRules(customDropPodsLetterTextRules.GetValue(slate), customDropPodsLetterText.GetValue(slate), "root"));
			}
			if (!customCaravanInventoryLetterLabel.GetValue(slate).NullOrEmpty() || customCaravanInventoryLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					give.customCaravanInventoryLetterLabel = x;
				}, QuestGenUtility.MergeRules(customCaravanInventoryLetterLabelRules.GetValue(slate), customCaravanInventoryLetterLabel.GetValue(slate), "root"));
			}
			if (!customCaravanInventoryLetterText.GetValue(slate).NullOrEmpty() || customCaravanInventoryLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					give.customCaravanInventoryLetterText = x;
				}, QuestGenUtility.MergeRules(customCaravanInventoryLetterTextRules.GetValue(slate), customCaravanInventoryLetterText.GetValue(slate), "root"));
			}
			give.sendStandardLetter = sendStandardLetter.GetValue(slate) ?? give.sendStandardLetter;
			give.joinPlayer = joinPlayer.GetValue(slate);
			give.makePrisoners = makePrisoners.GetValue(slate);
			give.Things = contents.GetValue(slate);
			if (contentsDefs.GetValue(slate) != null)
			{
				give.thingDefs.AddRange(contentsDefs.GetValue(slate));
			}
			QuestGen.quest.AddPart(give);
		}
	}
}
