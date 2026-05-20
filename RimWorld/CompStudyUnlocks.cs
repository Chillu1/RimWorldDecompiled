using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompStudyUnlocks : ThingComp, IThingStudied
{
	private List<float> studyThresholds = new List<float>();

	protected List<ChoiceLetter> letters = new List<ChoiceLetter>();

	protected int nextIndex;

	protected int studyProgress;

	private CompStudiable studyInt;

	protected CompProperties_StudyUnlocks Props => (CompProperties_StudyUnlocks)props;

	protected CompStudiable StudyComp => studyInt ?? (studyInt = parent.TryGetComp<CompStudiable>());

	public IReadOnlyList<ChoiceLetter> Letters => letters;

	public int Progress => studyProgress;

	public bool Completed => nextIndex >= Props.studyNotes.Count;

	public float? StudyKnowledgeAmount
	{
		get
		{
			if (studyProgress == 0)
			{
				return Props.defaultStudyAmount;
			}
			return Props.studyNotes[studyProgress - 1].studyKnowledgeAmount;
		}
	}

	public override void PostPostMake()
	{
		SetupStudyThresholds();
	}

	public void OnStudied(Pawn studier, float amount, KnowledgeCategoryDef category = null)
	{
		if (!Completed)
		{
			float anomalyKnowledgeGained = StudyComp.anomalyKnowledgeGained;
			for (int i = nextIndex; i < Props.studyNotes.Count && !(anomalyKnowledgeGained < studyThresholds[i]); i++)
			{
				RegisterStudyLevel(studier, i);
			}
		}
	}

	private void RegisterStudyLevel(Pawn studier, int i)
	{
		if (nextIndex <= i)
		{
			StudyNote studyNote = Props.studyNotes[i];
			nextIndex = i + 1;
			studyProgress = nextIndex;
			TaggedString label = studyNote.label.Formatted(studier.Named("PAWN"));
			TaggedString text = studyNote.text.Formatted(studier.Named("PAWN"));
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
			Find.LetterStack.ReceiveLetter(choiceLetter);
			ChoiceLetter choiceLetter2 = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
			choiceLetter2.arrivalTick = Find.TickManager.TicksGame;
			letters.Add(choiceLetter2);
			Notify_StudyLevelChanged(choiceLetter2);
		}
	}

	protected virtual void Notify_StudyLevelChanged(ChoiceLetter keptLetter)
	{
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos && !Completed)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Advance study",
				action = delegate
				{
					RegisterStudyLevel(parent.MapHeld.mapPawns.FreeColonists.RandomElement(), nextIndex);
				}
			};
		}
	}

	private void SetupStudyThresholds()
	{
		foreach (StudyNote studyNote in Props.studyNotes)
		{
			studyThresholds.Add((studyNote.threshold != 0f) ? studyNote.threshold : studyNote.thresholdRange.RandomInRange);
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref nextIndex, "nextIndex", 0);
		Scribe_Values.Look(ref studyProgress, "studyProgress", 0);
		Scribe_Collections.Look(ref studyThresholds, "studyThresholds", LookMode.Value);
		Scribe_Collections.Look(ref letters, "letters", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (letters == null)
			{
				letters = new List<ChoiceLetter>();
			}
			if (studyThresholds == null)
			{
				studyThresholds = new List<float>();
				SetupStudyThresholds();
			}
		}
	}
}
