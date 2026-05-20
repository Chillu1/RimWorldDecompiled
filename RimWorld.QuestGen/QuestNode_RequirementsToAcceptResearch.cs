using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_RequirementsToAcceptResearch : QuestNode
{
	public SlateRef<ResearchProjectDef> reserach;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestGen.quest.AddPart(new QuestPart_RequirementsToAcceptResearch
		{
			project = reserach.GetValue(slate)
		});
	}

	protected override bool TestRunInt(Slate slate)
	{
		ResearchProjectDef value = reserach.GetValue(slate);
		if (value != null && !value.IsFinished)
		{
			return false;
		}
		return true;
	}
}
