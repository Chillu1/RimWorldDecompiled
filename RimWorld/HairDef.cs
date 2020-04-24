using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HairDef : Def
	{
		[NoTranslate]
		public string texPath;

		public HairGender hairGender = HairGender.Any;

		[NoTranslate]
		public List<string> hairTags = new List<string>();
	}
}
