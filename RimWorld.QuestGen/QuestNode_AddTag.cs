using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddTag : QuestNode
	{
		[NoTranslate]
		public SlateRef<IEnumerable<object>> targets;

		[NoTranslate]
		public SlateRef<string> tag;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (targets.GetValue(slate) != null)
			{
				string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
				foreach (object item in targets.GetValue(slate))
				{
					Thing thing = item as Thing;
					if (thing != null)
					{
						QuestUtility.AddQuestTag(ref thing.questTags, questTagToAdd);
					}
					else
					{
						WorldObject worldObject = item as WorldObject;
						if (worldObject != null)
						{
							QuestUtility.AddQuestTag(ref worldObject.questTags, questTagToAdd);
						}
					}
				}
			}
		}
	}
}
