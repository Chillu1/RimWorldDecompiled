using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetMapWealth : QuestNode
	{
		public SlateRef<Map> map;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(storeAs.GetValue(slate), map.GetValue(slate).wealthWatcher.WealthTotal);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			slate.Set(storeAs.GetValue(slate), map.GetValue(slate).wealthWatcher.WealthTotal);
		}
	}
}
