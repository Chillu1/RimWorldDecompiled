using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AllowDecreesForLodger : QuestNode
{
	public SlateRef<Pawn> lodger;

	protected override void RunInt()
	{
		QuestGen.quest.AddPart(new QuestPart_AllowDecreesForLodger
		{
			lodger = lodger.GetValue(QuestGen.slate)
		});
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
