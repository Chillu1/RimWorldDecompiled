using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SetAndRestore : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> name;

		public SlateRef<object> value;

		public QuestNode node;

		protected override bool TestRunInt(Slate slate)
		{
			Slate.VarRestoreInfo restoreInfo = slate.GetRestoreInfo(name.GetValue(slate));
			slate.Set(name.GetValue(slate), value.GetValue(slate));
			try
			{
				return node.TestRun(slate);
			}
			finally
			{
				slate.Restore(restoreInfo);
			}
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Slate.VarRestoreInfo restoreInfo = QuestGen.slate.GetRestoreInfo(name.GetValue(slate));
			QuestGen.slate.Set(name.GetValue(slate), value.GetValue(slate));
			try
			{
				node.Run();
			}
			finally
			{
				QuestGen.slate.Restore(restoreInfo);
			}
		}
	}
}
