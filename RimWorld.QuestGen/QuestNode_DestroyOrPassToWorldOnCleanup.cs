using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_DestroyOrPassToWorldOnCleanup : QuestNode
	{
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
				QuestPart_DestroyThingsOrPassToWorldOnCleanup questPart_DestroyThingsOrPassToWorldOnCleanup = new QuestPart_DestroyThingsOrPassToWorldOnCleanup();
				questPart_DestroyThingsOrPassToWorldOnCleanup.things.AddRange(things.GetValue(slate));
				QuestGen.quest.AddPart(questPart_DestroyThingsOrPassToWorldOnCleanup);
			}
		}
	}
}
