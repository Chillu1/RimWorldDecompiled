using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IdeoSymbolPartDef : Def
	{
		public List<MemeDef> memes;

		public List<CultureDef> cultures;

		public bool CanBeChosenForIdeo(Ideo ideo)
		{
			if (memes == null && cultures == null)
			{
				return true;
			}
			if (cultures != null && cultures.Contains(ideo.culture))
			{
				return true;
			}
			if (memes != null)
			{
				for (int i = 0; i < ideo.memes.Count; i++)
				{
					if (memes.Contains(ideo.memes[i]))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
