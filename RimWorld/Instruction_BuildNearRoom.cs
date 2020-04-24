using Verse;

namespace RimWorld
{
	public class Instruction_BuildNearRoom : Instruction_BuildAtRoom
	{
		protected override CellRect BuildableRect => Find.TutorialState.roomRect.ExpandedBy(10);

		protected override bool AllowBuildAt(IntVec3 c)
		{
			if (!base.AllowBuildAt(c))
			{
				return false;
			}
			return !Find.TutorialState.roomRect.Contains(c);
		}
	}
}
