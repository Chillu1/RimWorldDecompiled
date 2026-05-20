using Verse;

namespace RimWorld;

public class CompStudiableMonolith : CompStudiable
{
	private const float FinishedStudyAmount = 4f;

	public override float AnomalyKnowledge
	{
		get
		{
			if (!Find.Anomaly.QuestlineEnded)
			{
				return base.AnomalyKnowledge;
			}
			return 4f;
		}
	}

	public override KnowledgeCategoryDef KnowledgeCategory => Find.Anomaly.LevelDef.monolithStudyCategory;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		anomalyKnowledgeGained = Find.Anomaly.monolithAnomalyKnowledge;
	}

	public override void Study(Pawn studier, float studyAmount, float anomalyKnowledgeAmount = 0f)
	{
		base.Study(studier, studyAmount, anomalyKnowledgeAmount);
		Find.Anomaly.monolithAnomalyKnowledge = anomalyKnowledgeGained;
	}
}
