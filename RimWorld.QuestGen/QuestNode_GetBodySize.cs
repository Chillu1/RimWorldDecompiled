using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetBodySize : QuestNode
	{
		public SlateRef<PawnKindDef> pawnKind;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(storeAs.GetValue(slate), pawnKind.GetValue(slate).RaceProps.baseBodySize);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestGen.slate.Set(storeAs.GetValue(slate), pawnKind.GetValue(slate).RaceProps.baseBodySize);
		}
	}
}
