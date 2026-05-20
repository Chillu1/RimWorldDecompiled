using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GiveTechprints : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<ResearchProjectDef> fixedProject;

	[NoTranslate]
	public SlateRef<string> storeProjectAs;

	protected override bool TestRunInt(Slate slate)
	{
		ResearchProjectDef researchProjectDef = FindTargetProject(slate);
		if (researchProjectDef == null || researchProjectDef.TechprintRequirementMet)
		{
			return false;
		}
		if (storeProjectAs.GetValue(slate) != null)
		{
			slate.Set(storeProjectAs.GetValue(slate), researchProjectDef);
		}
		return true;
	}

	private ResearchProjectDef FindTargetProject(Slate slate)
	{
		if (fixedProject.GetValue(slate) != null)
		{
			return fixedProject.GetValue(slate);
		}
		return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef p) => !p.IsFinished && !p.TechprintRequirementMet).RandomElement();
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		ResearchProjectDef researchProjectDef = FindTargetProject(slate);
		QuestPart_GiveTechprints questPart_GiveTechprints = new QuestPart_GiveTechprints();
		questPart_GiveTechprints.amount = 1;
		questPart_GiveTechprints.project = researchProjectDef;
		questPart_GiveTechprints.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_GiveTechprints.outSignalWasGiven = QuestGenUtility.HardcodedSignalWithQuestID("AddedTechprints");
		QuestGen.quest.AddPart(questPart_GiveTechprints);
		if (storeProjectAs.GetValue(slate) != null)
		{
			QuestGen.slate.Set(storeProjectAs.GetValue(slate), researchProjectDef);
		}
	}
}
