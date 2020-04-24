using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TurretGunUtility
	{
		public static bool NeedsShells(ThingDef turret)
		{
			if (turret.category == ThingCategory.Building && turret.building.IsTurret)
			{
				return turret.building.turretGunDef.HasComp(typeof(CompChangeableProjectile));
			}
			return false;
		}

		public static ThingDef TryFindRandomShellDef(ThingDef turret, bool allowEMP = true, bool mustHarmHealth = true, TechLevel techLevel = TechLevel.Undefined, bool allowAntigrainWarhead = false, float maxMarketValue = -1f)
		{
			if (!NeedsShells(turret))
			{
				return null;
			}
			ThingFilter fixedFilter = turret.building.turretGunDef.building.fixedStorageSettings.filter;
			if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => fixedFilter.Allows(x) && (allowEMP || x.projectileWhenLoaded.projectile.damageDef != DamageDefOf.EMP) && (!mustHarmHealth || x.projectileWhenLoaded.projectile.damageDef.harmsHealth) && (techLevel == TechLevel.Undefined || (int)x.techLevel <= (int)techLevel) && (allowAntigrainWarhead || x != ThingDefOf.Shell_AntigrainWarhead) && (maxMarketValue < 0f || x.BaseMarketValue <= maxMarketValue)).TryRandomElement(out ThingDef result))
			{
				return result;
			}
			return null;
		}
	}
}
