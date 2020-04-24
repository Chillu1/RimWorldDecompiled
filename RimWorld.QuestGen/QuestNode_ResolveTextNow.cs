using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_ResolveTextNow : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> root;

		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<RulePack> rules;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			string var = QuestGenUtility.ResolveLocalTextWithDescriptionRules(rules.GetValue(slate), root.GetValue(slate));
			slate.Set(storeAs.GetValue(slate), var);
		}
	}
}
