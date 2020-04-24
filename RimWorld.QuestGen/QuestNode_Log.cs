using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Log : QuestNode
	{
		[NoTranslate]
		public SlateRef<object> message;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Log.Message("QuestNode_Log: " + message.ToString(QuestGen.slate));
		}
	}
}
