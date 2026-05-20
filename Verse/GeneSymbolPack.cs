using System.Collections.Generic;

namespace Verse
{
	public class GeneSymbolPack
	{
		public class WeightedSymbol
		{
			[MustTranslate]
			public string symbol;

			public float weight = 1f;
		}

		public List<WeightedSymbol> prefixSymbols;

		public List<WeightedSymbol> suffixSymbols;

		public List<WeightedSymbol> wholeNameSymbols;
	}
}
