using Verse;

namespace RimWorld
{
	public static class BreakdownableUtility
	{
		public static bool IsBrokenDown(this Thing t)
		{
			return t.TryGetComp<CompBreakdownable>()?.BrokenDown ?? false;
		}
	}
}
