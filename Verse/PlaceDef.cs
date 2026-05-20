using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class PlaceDef : Def
	{
		public RulePack placeRules;

		[NoTranslate]
		public List<string> tags = new List<string>();
	}
}
