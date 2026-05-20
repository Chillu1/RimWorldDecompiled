using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_ReleaseParalyzedAnimals : QuestNode
	{
		public SlateRef<IEnumerable<Pawn>> pawns;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			IEnumerable<Pawn> value = pawns.GetValue(slate);
			if (!value.EnumerableNullOrEmpty())
			{
				QuestPart_ReleaseParalyzedAnimals questPart_ReleaseParalyzedAnimals = new QuestPart_ReleaseParalyzedAnimals();
				questPart_ReleaseParalyzedAnimals.pawns.AddRange(value);
				QuestGen.quest.AddPart(questPart_ReleaseParalyzedAnimals);
			}
		}
	}
}
