using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_SetGrowingZonePlant : Lesson_Instruction
	{
		private Zone_Growing GrowZone => (Zone_Growing)base.Map.zoneManager.AllZones.FirstOrDefault((Zone z) => z is Zone_Growing);

		public override void LessonOnGUI()
		{
			TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(GrowZone.cells), def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(Gen.AveragePosition(GrowZone.cells));
		}
	}
}
