using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AssaultColony : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<IEnumerable<Pawn>> pawns;

		public SlateRef<Faction> faction;

		public SlateRef<bool?> canTimeoutOrFlee;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (!pawns.GetValue(slate).EnumerableNullOrEmpty())
			{
				QuestPart_AssaultColony questPart_AssaultColony = new QuestPart_AssaultColony();
				questPart_AssaultColony.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
				questPart_AssaultColony.pawns.AddRange(pawns.GetValue(slate));
				questPart_AssaultColony.mapParent = slate.Get<Map>("map").Parent;
				questPart_AssaultColony.faction = faction.GetValue(slate);
				questPart_AssaultColony.canTimeoutOrFlee = canTimeoutOrFlee.GetValue(slate) ?? true;
				QuestGen.quest.AddPart(questPart_AssaultColony);
			}
		}
	}
}
