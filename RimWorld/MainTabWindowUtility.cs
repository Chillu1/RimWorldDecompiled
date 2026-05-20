using Verse;

namespace RimWorld;

public static class MainTabWindowUtility
{
	public static void NotifyAllPawnTables_PawnsChanged()
	{
		if (Find.WindowStack == null || !UnityData.IsInMainThread)
		{
			return;
		}
		WindowStack windowStack = Find.WindowStack;
		for (int i = 0; i < windowStack.Count; i++)
		{
			if (windowStack[i] is MainTabWindow_PawnTable mainTabWindow_PawnTable)
			{
				mainTabWindow_PawnTable.Notify_PawnsChanged();
			}
		}
	}
}
