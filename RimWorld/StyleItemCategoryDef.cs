using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StyleItemCategoryDef : Def
	{
		private List<StyleItemDef> cachedStyleItems;

		public List<StyleItemDef> ItemsInCategory
		{
			get
			{
				if (cachedStyleItems == null)
				{
					cachedStyleItems = new List<StyleItemDef>();
					cachedStyleItems.AddRange(StyleItemDef.AllStyleItemDefs.Where((StyleItemDef x) => x.StyleItemCategory == this));
				}
				return cachedStyleItems;
			}
		}
	}
}
