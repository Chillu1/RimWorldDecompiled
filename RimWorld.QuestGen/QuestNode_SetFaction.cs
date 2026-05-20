using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SetFaction : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<Faction> faction;

		public SlateRef<IEnumerable<Thing>> things;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (!things.GetValue(slate).EnumerableNullOrEmpty())
			{
				QuestPart_SetFaction questPart_SetFaction = new QuestPart_SetFaction();
				questPart_SetFaction.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
				questPart_SetFaction.faction = faction.GetValue(slate);
				questPart_SetFaction.things.AddRange(things.GetValue(slate));
				QuestGen.quest.AddPart(questPart_SetFaction);
			}
		}
	}
}
