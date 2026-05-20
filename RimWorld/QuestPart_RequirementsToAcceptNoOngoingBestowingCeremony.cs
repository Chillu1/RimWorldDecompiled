using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptNoOngoingBestowingCeremony : QuestPart_RequirementsToAccept
{
	public override AcceptanceReport CanAccept()
	{
		if (Find.QuestManager.QuestsListForReading.Any((Quest q) => q.State == QuestState.Ongoing && q.root == QuestScriptDefOf.BestowingCeremony))
		{
			return new AcceptanceReport("QuestCanNotStartUntilBestowingCeremonyFinished".Translate());
		}
		return true;
	}
}
