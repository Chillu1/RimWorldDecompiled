using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_JoinPlayer : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<bool> joinPlayer;

	public SlateRef<bool> makePrisoners;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_JoinPlayer questPart_JoinPlayer = new QuestPart_JoinPlayer();
			questPart_JoinPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_JoinPlayer.joinPlayer = joinPlayer.GetValue(slate);
			questPart_JoinPlayer.makePrisoners = makePrisoners.GetValue(slate);
			questPart_JoinPlayer.mapParent = QuestGen.slate.Get<Map>("map").Parent;
			questPart_JoinPlayer.pawns.AddRange(pawns.GetValue(slate));
			QuestGen.quest.AddPart(questPart_JoinPlayer);
		}
	}
}
