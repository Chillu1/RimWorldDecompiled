using RimWorld.Planet;
using Verse.Grammar;

namespace RimWorld
{
	public static class NamePlayerSettlementDialogUtility
	{
		public static bool IsValidName(string s)
		{
			if (s.Length == 0)
			{
				return false;
			}
			if (s.Length > 64)
			{
				return false;
			}
			if (GrammarResolver.ContainsSpecialChars(s))
			{
				return false;
			}
			return true;
		}

		public static void Named(Settlement factionBase, string s)
		{
			factionBase.Name = s;
			factionBase.namedByPlayer = true;
		}
	}
}
