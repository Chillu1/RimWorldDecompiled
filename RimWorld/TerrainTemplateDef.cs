using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TerrainTemplateDef : Def
	{
		[NoTranslate]
		public string texturePath;

		[NoTranslate]
		public List<string> tags;

		public List<ResearchProjectDef> researchPrerequisites;

		public TerrainDef burnedDef;

		public List<ThingDefCountClass> costList;

		public DesignatorDropdownGroupDef designatorDropdown;

		public List<StatModifier> statBases;

		public int uiOrder;

		public int renderPrecedenceStart;

		public int constructionSkillPrerequisite;

		public bool canGenerateDefaultDesignator = true;

		public StyleCategoryDef dominantStyleCategory;
	}
}
