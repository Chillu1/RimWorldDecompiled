using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ActiveLessonHandler : IExposable
	{
		private Lesson activeLesson;

		public Lesson Current => activeLesson;

		public bool ActiveLessonVisible
		{
			get
			{
				if (activeLesson != null)
				{
					return !Find.WindowStack.WindowsPreventDrawTutor;
				}
				return false;
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref activeLesson, "activeLesson");
		}

		public void Activate(InstructionDef id)
		{
			Lesson_Instruction lesson_Instruction = activeLesson as Lesson_Instruction;
			if (lesson_Instruction == null || id != lesson_Instruction.def)
			{
				Lesson_Instruction lesson_Instruction2 = (Lesson_Instruction)Activator.CreateInstance(id.instructionClass);
				lesson_Instruction2.def = id;
				activeLesson = lesson_Instruction2;
				activeLesson.OnActivated();
			}
		}

		public void Activate(Lesson lesson)
		{
			Lesson_Note lesson_Note = lesson as Lesson_Note;
			if (lesson_Note != null && activeLesson != null)
			{
				lesson_Note.doFadeIn = false;
			}
			activeLesson = lesson;
			activeLesson.OnActivated();
		}

		public void Deactivate()
		{
			Lesson lesson = activeLesson;
			activeLesson = null;
			lesson?.PostDeactivated();
		}

		public void ActiveLessonOnGUI()
		{
			if (!(Time.timeSinceLevelLoad < 0.01f) && ActiveLessonVisible)
			{
				activeLesson.LessonOnGUI();
			}
		}

		public void ActiveLessonUpdate()
		{
			if (!(Time.timeSinceLevelLoad < 0.01f) && ActiveLessonVisible)
			{
				activeLesson.LessonUpdate();
			}
		}

		public void Notify_KnowledgeDemonstrated(ConceptDef conc)
		{
			if (Current != null)
			{
				Current.Notify_KnowledgeDemonstrated(conc);
			}
		}
	}
}
