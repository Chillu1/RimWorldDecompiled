using LudeonTK;

namespace Verse;

public class DebugOutputsFactions
{
	[DebugOutput("Factions", false)]
	public static void AllFactions()
	{
		Find.FactionManager.LogAllFactions();
	}

	[DebugOutput("Factions", false)]
	public static void AllFactionsToRemove()
	{
		Find.FactionManager.LogFactionsToRemove();
	}

	[DebugOutput("Factions", false)]
	public static void AllFactionsFromPawns()
	{
		Find.FactionManager.LogFactionsOnPawns();
	}
}
