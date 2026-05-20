using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorytellerComp_MonolithMigration : StorytellerComp
{
	protected StorytellerCompProperties_MonolithMigration Props => (StorytellerCompProperties_MonolithMigration)props;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		if (ModsConfig.AnomalyActive && Find.Anomaly.monolith == null && Find.Anomaly.LevelDef != MonolithLevelDefOf.Disrupted && !Find.QuestManager.QuestsListForReading.Any((Quest q) => q.root == QuestScriptDefOf.MonolithMigration && (q.State == QuestState.Ongoing || q.State == QuestState.NotYetAccepted)) && Rand.MTBEventOccurs(DebugSettings.fastMonolithRespawn ? 0.1f : Props.mtbDays, 60000f, 1000f))
		{
			yield return new FiringIncident(Props.incident, this, GenerateParms(IncidentCategoryDefOf.Misc, target));
		}
	}
}
