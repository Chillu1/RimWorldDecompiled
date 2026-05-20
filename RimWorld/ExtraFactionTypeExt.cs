using Verse;

namespace RimWorld;

public static class ExtraFactionTypeExt
{
	public static string GetLabel(this ExtraFactionType factionType)
	{
		return ("ExtraFactionType_" + factionType).Translate();
	}
}
