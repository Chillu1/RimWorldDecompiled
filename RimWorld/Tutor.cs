using Verse;

namespace RimWorld
{
	public class Tutor : IExposable
	{
		public ActiveLessonHandler activeLesson = new ActiveLessonHandler();

		public LearningReadout learningReadout = new LearningReadout();

		public TutorialState tutorialState = new TutorialState();

		public void ExposeData()
		{
			Scribe_Deep.Look(ref activeLesson, "activeLesson");
			Scribe_Deep.Look(ref learningReadout, "learningReadout");
			Scribe_Deep.Look(ref tutorialState, "tutorialState");
		}

		internal void TutorUpdate()
		{
			activeLesson.ActiveLessonUpdate();
			learningReadout.LearningReadoutUpdate();
		}

		internal void TutorOnGUI()
		{
			activeLesson.ActiveLessonOnGUI();
			learningReadout.LearningReadoutOnGUI();
		}
	}
}
