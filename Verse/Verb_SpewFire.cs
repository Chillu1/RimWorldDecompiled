using RimWorld;
using UnityEngine;

namespace Verse;

public class Verb_SpewFire : Verb
{
	protected override bool TryCastShot()
	{
		if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
		{
			return false;
		}
		if (base.EquipmentSource != null)
		{
			base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
			base.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
		}
		IntVec3 position = caster.Position;
		float num = Mathf.Atan2(-(currentTarget.Cell.z - position.z), currentTarget.Cell.x - position.x) * 57.29578f;
		FloatRange value = new FloatRange(num - 13f, num + 13f);
		IntVec3 center = position;
		Map mapHeld = caster.MapHeld;
		float effectiveRange = EffectiveRange;
		DamageDef flame = DamageDefOf.Flame;
		Thing instigator = caster;
		ThingDef filth_FlammableBile = ThingDefOf.Filth_FlammableBile;
		FloatRange? affectedAngle = value;
		GenExplosion.DoExplosion(center, mapHeld, effectiveRange, flame, instigator, -1, -1f, null, null, null, null, filth_FlammableBile, 1f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 1f, damageFalloff: false, null, null, affectedAngle, doVisualEffects: false, 0.6f, 0f, doSoundEffects: false);
		AddEffecterToMaintain(EffecterDefOf.Fire_SpewShort.Spawn(caster.Position, currentTarget.Cell, caster.Map), caster.Position, currentTarget.Cell, 14, caster.Map);
		lastShotTick = Find.TickManager.TicksGame;
		return true;
	}

	public override bool Available()
	{
		if (!base.Available())
		{
			return false;
		}
		if (CasterIsPawn)
		{
			Pawn casterPawn = CasterPawn;
			if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
			{
				return false;
			}
		}
		return true;
	}
}
