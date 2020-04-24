using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_QuestUnique : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> tag;

		public SlateRef<Faction> faction;

		[NoTranslate]
		public SlateRef<string> storeProcessedTagAs;

		public static string GetProcessedTag(string tag, Faction faction)
		{
			if (faction == null)
			{
				return tag;
			}
			return tag + "_" + faction.Name;
		}

		private string GetProcessedTag(Slate slate)
		{
			return GetProcessedTag(tag.GetValue(slate), faction.GetValue(slate));
		}

		protected override void RunInt()
		{
			string processedTag = GetProcessedTag(QuestGen.slate);
			QuestUtility.AddQuestTag(ref QuestGen.quest.tags, processedTag);
			if (storeProcessedTagAs.GetValue(QuestGen.slate) != null)
			{
				QuestGen.slate.Set(storeProcessedTagAs.GetValue(QuestGen.slate), processedTag);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			string processedTag = GetProcessedTag(slate);
			if (storeProcessedTagAs.GetValue(slate) != null)
			{
				slate.Set(storeProcessedTagAs.GetValue(slate), processedTag);
			}
			foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
			{
				if (item.State == QuestState.Ongoing && item.tags.Contains(processedTag))
				{
					return false;
				}
			}
			return true;
		}
	}
}
