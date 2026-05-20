using RimWorld;
using RimWorld.Planet;

namespace Verse
{
	public class WorkModeDrawer_Escort : WorkModeDrawer
	{
		protected override bool DrawIconAtTarget => false;

		public override GlobalTargetInfo GetTargetForLine(MechanitorControlGroup group)
		{
			return group.Tracker.Pawn;
		}
	}
}
