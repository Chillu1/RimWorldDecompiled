using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Message : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<MessageTypeDef> messageType;

	public SlateRef<string> text;

	public SlateRef<RulePack> rules;

	[NoTranslate]
	public SlateRef<IEnumerable<object>> lookTargets;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_Message message = new QuestPart_Message();
		message.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
		message.messageType = messageType.GetValue(slate) ?? MessageTypeDefOf.NeutralEvent;
		message.lookTargets = QuestGenUtility.ToLookTargets(lookTargets, slate);
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			message.message = x;
		}, QuestGenUtility.MergeRules(rules.GetValue(slate), text.GetValue(slate), "root"));
		QuestGen.quest.AddPart(message);
	}
}
