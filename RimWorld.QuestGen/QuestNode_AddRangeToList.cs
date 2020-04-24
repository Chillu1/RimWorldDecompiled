using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddRangeToList : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		public SlateRef<List<object>> value;

		protected override bool TestRunInt(Slate slate)
		{
			List<object> list = value.GetValue(slate);
			if (list != null)
			{
				QuestGenUtility.AddRangeToOrMakeList(slate, name.GetValue(slate), list);
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			List<object> list = value.GetValue(slate);
			if (list != null)
			{
				QuestGenUtility.AddRangeToOrMakeList(slate, name.GetValue(slate), list);
			}
		}
	}
}
