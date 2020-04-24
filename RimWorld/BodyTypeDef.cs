using UnityEngine;
using Verse;

namespace RimWorld
{
	public class BodyTypeDef : Def
	{
		[NoTranslate]
		public string bodyNakedGraphicPath;

		[NoTranslate]
		public string bodyDessicatedGraphicPath;

		public Vector2 headOffset;
	}
}
