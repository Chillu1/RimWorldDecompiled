using Verse;

namespace RimWorld
{
	public class IdeoSymbolPack
	{
		[MustTranslate]
		public string ideoName;

		[MustTranslate]
		public string theme;

		[MustTranslate]
		public string adjective;

		[MustTranslate]
		public string member;

		public bool prefix;

		public string PrimarySymbol
		{
			get
			{
				if (!ideoName.NullOrEmpty())
				{
					return ideoName;
				}
				return theme;
			}
		}
	}
}
