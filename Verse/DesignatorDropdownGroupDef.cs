using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DesignatorDropdownGroupDef : Def
	{
		public enum IconSource : byte
		{
			Cost,
			Placed
		}

		public IconSource iconSource;

		public bool useGridMenu;

		public bool includeEyeDropperTool;

		public IEnumerable<BuildableDef> BuildablesWithoutDefaultDesignators()
		{
			return from x in ((IEnumerable<BuildableDef>)DefDatabase<ThingDef>.AllDefs).Concat((IEnumerable<BuildableDef>)DefDatabase<TerrainDef>.AllDefs)
				where x.designatorDropdown == this && !x.canGenerateDefaultDesignator
				select x;
		}
	}
}
