using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SetChildCount : QuestNode
	{
		public SlateRef<IEnumerable<Pawn>> pawns;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			int num = 0;
			foreach (Pawn item in pawns.GetValue(slate))
			{
				if (item.DevelopmentalStage.Juvenile())
				{
					num++;
				}
			}
			slate.Set("childCount", num);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
