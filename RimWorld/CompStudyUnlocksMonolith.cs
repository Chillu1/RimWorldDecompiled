using Verse;

namespace RimWorld;

public class CompStudyUnlocksMonolith : CompStudyUnlocks
{
	protected new CompProperties_StudyUnlocksMonolith Props => (CompProperties_StudyUnlocksMonolith)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		nextIndex = Find.Anomaly.MonolithNextIndex;
		studyProgress = Find.Anomaly.MonolithStudyProgress;
	}

	protected override void Notify_StudyLevelChanged(ChoiceLetter keptLetter)
	{
		Find.Anomaly.Notify_MonolithStudyIncreased(keptLetter, nextIndex, studyProgress);
		letters.Clear();
	}
}
