using Verse;

namespace RimWorld
{
	public class ThoughtWorker_InSunlight : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return ThoughtState.Inactive;
			}
			if (p.Spawned && p.Position.InSunlight(p.Map))
			{
				return true;
			}
			return ThoughtState.Inactive;
		}
	}
}
