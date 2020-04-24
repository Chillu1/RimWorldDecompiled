using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Prefix : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> prefix;

		public SlateRef<bool> allowNonPrefixedLookup;

		public List<PrefixCapturedVar> parms = new List<PrefixCapturedVar>();

		[NoTranslate]
		public List<SlateRef<string>> returnVarNames = new List<SlateRef<string>>();

		public QuestNode node;

		protected override bool TestRunInt(Slate slate)
		{
			string value = prefix.GetValue(slate);
			List<Slate.VarRestoreInfo> varsRestoreInfo = QuestGenUtility.SetVarsForPrefix(parms, value, slate);
			if (!value.NullOrEmpty())
			{
				slate.PushPrefix(value, allowNonPrefixedLookup.GetValue(slate));
			}
			try
			{
				return node.TestRun(slate);
			}
			finally
			{
				if (!value.NullOrEmpty())
				{
					slate.PopPrefix();
				}
				QuestGenUtility.GetReturnedVars(returnVarNames, value, slate);
				QuestGenUtility.RestoreVarsForPrefix(varsRestoreInfo, slate);
			}
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			string value = prefix.GetValue(slate);
			List<Slate.VarRestoreInfo> varsRestoreInfo = QuestGenUtility.SetVarsForPrefix(parms, value, QuestGen.slate);
			if (!value.NullOrEmpty())
			{
				QuestGen.slate.PushPrefix(value, allowNonPrefixedLookup.GetValue(slate));
			}
			try
			{
				node.Run();
			}
			finally
			{
				if (!value.NullOrEmpty())
				{
					QuestGen.slate.PopPrefix();
				}
				QuestGenUtility.GetReturnedVars(returnVarNames, value, QuestGen.slate);
				QuestGenUtility.RestoreVarsForPrefix(varsRestoreInfo, QuestGen.slate);
			}
		}
	}
}
