using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetSameQuestsCount : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			return true;
		}

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			int var = Find.QuestManager.QuestsListForReading.Count((Quest x) => x.root == QuestGen.Root);
			slate.Set("sameQuestsCount", var);
		}
	}
}
