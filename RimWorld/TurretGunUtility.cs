using System.Linq;
using Verse;

namespace RimWorld;

public static class TurretGunUtility
{
	private const float WasterToxicShellChance = 0.5f;

	public static bool NeedsShells(ThingDef turret)
	{
		if (turret.category == ThingCategory.Building && turret.building.IsTurret)
		{
			return turret.building.turretGunDef.HasComp(typeof(CompChangeableProjectile));
		}
		return false;
	}

	public static ThingDef TryFindRandomShellDef(ThingDef turret, bool allowEMP = true, bool allowToxGas = false, bool mustHarmHealth = true, TechLevel techLevel = TechLevel.Undefined, bool allowAntigrainWarhead = false, float maxMarketValue = -1f, Faction faction = null)
	{
		if (!NeedsShells(turret))
		{
			return null;
		}
		ThingFilter fixedFilter = turret.building.turretGunDef.building.fixedStorageSettings.filter;
		if (ModsConfig.BiotechActive && faction != null && faction.def == FactionDefOf.PirateWaster && fixedFilter.Allows(ThingDefOf.Shell_Toxic) && Rand.Chance(0.5f))
		{
			return ThingDefOf.Shell_Toxic;
		}
		if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => fixedFilter.Allows(x) && (allowEMP || x.projectileWhenLoaded.projectile.damageDef != DamageDefOf.EMP) && (allowToxGas || !ModsConfig.BiotechActive || x.projectileWhenLoaded.projectile.damageDef != DamageDefOf.ToxGas) && (!mustHarmHealth || x.projectileWhenLoaded.projectile.damageDef.harmsHealth) && (techLevel == TechLevel.Undefined || (int)x.techLevel <= (int)techLevel) && (allowAntigrainWarhead || x != ThingDefOf.Shell_AntigrainWarhead) && (maxMarketValue < 0f || x.BaseMarketValue <= maxMarketValue)).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}
}
