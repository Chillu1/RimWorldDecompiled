using Verse;

namespace RimWorld
{
	public class ThoughtWorker_LookChangeDesired : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.style == null || !ModsConfig.IdeologyActive)
			{
				return ThoughtState.Inactive;
			}
			return p.style.LookChangeDesired;
		}
	}
}
