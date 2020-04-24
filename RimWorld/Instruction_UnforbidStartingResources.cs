using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_UnforbidStartingResources : Lesson_Instruction
	{
		protected override float ProgressPercent => (float)Find.TutorialState.startingItems.Where((Thing it) => !it.IsForbidden(Faction.OfPlayer) || it.Destroyed).Count() / (float)Find.TutorialState.startingItems.Count;

		private IEnumerable<Thing> NeedUnforbidItems()
		{
			return Find.TutorialState.startingItems.Where((Thing it) => it.IsForbidden(Faction.OfPlayer) && !it.Destroyed);
		}

		public override void PostDeactivated()
		{
			base.PostDeactivated();
			Find.TutorialState.startingItems.RemoveAll((Thing it) => !Instruction_EquipWeapons.IsWeapon(it));
		}

		public override void LessonOnGUI()
		{
			foreach (Thing item in NeedUnforbidItems())
			{
				TutorUtility.DrawLabelOnThingOnGUI(item, def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
			foreach (Thing item in NeedUnforbidItems())
			{
				GenDraw.DrawArrowPointingAt(item.DrawPos, offscreenOnly: true);
			}
		}
	}
}
