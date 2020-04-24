using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SlateDump : QuestNode
	{
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Log.Message(QuestGen.slate.ToString());
		}
	}
}
