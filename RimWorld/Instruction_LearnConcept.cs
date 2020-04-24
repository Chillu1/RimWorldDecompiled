using Verse;

namespace RimWorld
{
	public class Instruction_LearnConcept : Lesson_Instruction
	{
		protected override float ProgressPercent => PlayerKnowledgeDatabase.GetKnowledge(def.concept);

		public override void OnActivated()
		{
			PlayerKnowledgeDatabase.SetKnowledge(def.concept, 0f);
			base.OnActivated();
		}

		public override void LessonUpdate()
		{
			base.LessonUpdate();
			if (PlayerKnowledgeDatabase.IsComplete(def.concept))
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
