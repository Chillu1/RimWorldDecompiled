using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_UniqueWeapon : CompProperties
{
	public List<WeaponCategoryDef> weaponCategories = new List<WeaponCategoryDef>();

	[MustTranslate]
	public List<string> namerLabels = new List<string>();

	public CompProperties_UniqueWeapon()
	{
		compClass = typeof(CompUniqueWeapon);
	}
}
