using Verse;

namespace RimWorld;

[DefOf]
public static class LogEntryDefOf
{
	public static LogEntryDef MeleeAttack;

	static LogEntryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(LogEntryDefOf));
	}
}
