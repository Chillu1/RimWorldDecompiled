using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetPawnKindCombatPower : QuestNode
	{
		public SlateRef<PawnKindDef> kindDef;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(storeAs.GetValue(slate), kindDef.GetValue(slate).combatPower);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			slate.Set(storeAs.GetValue(slate), kindDef.GetValue(slate).combatPower);
		}
	}
}
