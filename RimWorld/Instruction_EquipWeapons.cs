using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_EquipWeapons : Lesson_Instruction
	{
		protected override float ProgressPercent => (float)base.Map.mapPawns.FreeColonists.Where((Pawn c) => c.equipment.Primary != null).Count() / (float)base.Map.mapPawns.FreeColonistsCount;

		private IEnumerable<Thing> Weapons => Find.TutorialState.startingItems.Where((Thing it) => IsWeapon(it) && it.Spawned);

		public static bool IsWeapon(Thing t)
		{
			if (t.def.IsWeapon)
			{
				return t.def.BaseMarketValue > 30f;
			}
			return false;
		}

		public override void LessonOnGUI()
		{
			foreach (Thing weapon in Weapons)
			{
				TutorUtility.DrawLabelOnThingOnGUI(weapon, def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			foreach (Thing weapon in Weapons)
			{
				GenDraw.DrawArrowPointingAt(weapon.DrawPos, offscreenOnly: true);
			}
			if (ProgressPercent > 0.9999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
