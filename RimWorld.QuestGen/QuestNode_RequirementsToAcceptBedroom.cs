using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_RequirementsToAcceptBedroom : QuestNode
	{
		public SlateRef<IEnumerable<Pawn>> pawns;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.quest.AddPart(new QuestPart_RequirementsToAcceptBedroom
			{
				targetPawns = (from p in pawns.GetValue(QuestGen.slate)
					where p.royalty != null && p.royalty.HighestTitleWithBedroomRequirements() != null
					orderby p.royalty.HighestTitleWithBedroomRequirements().def.seniority descending
					select p).ToList(),
				mapParent = slate.Get<Map>("map").Parent
			});
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
