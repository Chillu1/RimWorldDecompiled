using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

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
		if (targets.GetValue(slate) == null)
		{
			return;
		}
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
		foreach (object item in targets.GetValue(slate))
		{
			if (item is Thing thing)
			{
				QuestUtility.AddQuestTag(ref thing.questTags, questTagToAdd);
			}
			else if (item is WorldObject worldObject)
			{
				QuestUtility.AddQuestTag(ref worldObject.questTags, questTagToAdd);
			}
			else if (item is TransportShip transportShip)
			{
				QuestUtility.AddQuestTag(ref transportShip.questTags, questTagToAdd);
			}
		}
	}
}
