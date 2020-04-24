using Verse;

namespace RimWorld
{
	public static class MainTabWindowUtility
	{
		public static void NotifyAllPawnTables_PawnsChanged()
		{
			if (Find.WindowStack != null)
			{
				WindowStack windowStack = Find.WindowStack;
				for (int i = 0; i < windowStack.Count; i++)
				{
					(windowStack[i] as MainTabWindow_PawnTable)?.Notify_PawnsChanged();
				}
			}
		}
	}
}
