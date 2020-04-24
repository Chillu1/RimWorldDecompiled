using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetFactionOf : QuestNode
	{
		public SlateRef<Thing> thing;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			DoWork(slate);
			return true;
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private void DoWork(Slate slate)
		{
			Faction var = null;
			Thing value = thing.GetValue(slate);
			if (value != null)
			{
				var = value.Faction;
			}
			slate.Set(storeAs.GetValue(slate), var);
		}
	}
}
