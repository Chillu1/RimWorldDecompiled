using Verse;

namespace RimWorld;

public class CompPorcupine : ThingComp
{
	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (dinfo.Instigator is Pawn pawn && pawn.RaceProps.IsFlesh && (dinfo.WeaponBodyPartGroup != null || dinfo.Weapon == null || dinfo.Weapon.IsMeleeWeapon))
		{
			BodyPartRecord corePart = pawn.RaceProps.body.corePart;
			pawn.health.AddHediff(HediffDefOf.PorcupineQuill, corePart);
		}
	}
}
