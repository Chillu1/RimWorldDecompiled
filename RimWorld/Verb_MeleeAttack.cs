using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class Verb_MeleeAttack : Verb
{
	private const int TargetCooldown = 50;

	protected override bool TryCastShot()
	{
		Pawn casterPawn = CasterPawn;
		if (!casterPawn.Spawned)
		{
			return false;
		}
		if (casterPawn.stances.FullBodyBusy)
		{
			return false;
		}
		Thing thing = currentTarget.Thing;
		if (!CanHitTarget(thing))
		{
			Log.Warning(casterPawn?.ToString() + " meleed " + thing?.ToString() + " from out of melee position.");
		}
		casterPawn.rotationTracker.Face(thing.DrawPos);
		if (!IsTargetImmobile(currentTarget) && casterPawn.skills != null && (currentTarget.Pawn == null || !currentTarget.Pawn.IsColonyMech))
		{
			casterPawn.skills.Learn(SkillDefOf.Melee, 200f * verbProps.AdjustedFullCycleTime(this, casterPawn));
		}
		Pawn pawn = thing as Pawn;
		if (pawn != null && !pawn.Dead && (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || pawn.MentalStateDef != MentalStateDefOf.SocialFighting) && (casterPawn.story == null || !casterPawn.story.traits.DisableHostilityFrom(pawn)))
		{
			pawn.mindState.meleeThreat = casterPawn;
			pawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
		}
		Map map = thing.Map;
		Vector3 drawPos = thing.DrawPos;
		SoundDef soundDef;
		bool result;
		if (Rand.Chance(GetNonMissChance(thing)))
		{
			if (!Rand.Chance(GetDodgeChance(thing)))
			{
				soundDef = ((thing.def.category != ThingCategory.Building) ? SoundHitPawn() : SoundHitBuilding());
				if (verbProps.impactMote != null)
				{
					MoteMaker.MakeStaticMote(drawPos, map, verbProps.impactMote);
				}
				if (verbProps.impactFleck != null)
				{
					FleckMaker.Static(drawPos, map, verbProps.impactFleck);
				}
				BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesHit, alwaysShow: true);
				result = true;
				DamageWorker.DamageResult damageResult = ApplyMeleeDamageToTarget(currentTarget);
				if (pawn != null && damageResult.totalDamageDealt > 0f)
				{
					ApplyMeleeSlaveSuppression(pawn, damageResult.totalDamageDealt);
				}
				if (damageResult.stunned && damageResult.parts.NullOrEmpty())
				{
					Find.BattleLog.RemoveEntry(battleLogEntry_MeleeCombat);
				}
				else
				{
					damageResult.AssociateWithLog(battleLogEntry_MeleeCombat);
					if (damageResult.deflected)
					{
						battleLogEntry_MeleeCombat.RuleDef = maneuver.combatLogRulesDeflect;
						battleLogEntry_MeleeCombat.alwaysShowInCompact = false;
					}
				}
			}
			else
			{
				result = false;
				soundDef = SoundDodge(thing);
				MoteMaker.ThrowText(drawPos, map, "TextMote_Dodge".Translate(), 1.9f);
				CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesDodge, alwaysShow: false);
			}
		}
		else
		{
			result = false;
			soundDef = SoundMiss();
			CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesMiss, alwaysShow: false);
		}
		soundDef?.PlayOneShot(new TargetInfo(thing.Position, map));
		if (casterPawn.Spawned)
		{
			casterPawn.Drawer.Notify_MeleeAttackOn(thing);
		}
		if (pawn != null && !pawn.Dead && pawn.Spawned)
		{
			pawn.stances.stagger.StaggerFor(95);
		}
		if (casterPawn.Spawned)
		{
			casterPawn.rotationTracker.FaceCell(thing.Position);
		}
		if (casterPawn.caller != null)
		{
			casterPawn.caller.Notify_DidMeleeAttack();
		}
		return result;
	}

	public BattleLogEntry_MeleeCombat CreateCombatLog(Func<ManeuverDef, RulePackDef> rulePackGetter, bool alwaysShow)
	{
		if (maneuver == null)
		{
			return null;
		}
		if (tool == null)
		{
			return null;
		}
		BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackGetter(maneuver), alwaysShow, CasterPawn, currentTarget.Thing, base.ImplementOwnerType, tool.labelUsedInLogging ? tool.label : "", (base.EquipmentSource == null) ? null : base.EquipmentSource.def, (base.HediffCompSource == null) ? null : base.HediffCompSource.Def, maneuver.logEntryDef);
		Find.BattleLog.Add(battleLogEntry_MeleeCombat);
		return battleLogEntry_MeleeCombat;
	}

	private float GetNonMissChance(LocalTargetInfo target)
	{
		if (surpriseAttack)
		{
			return 1f;
		}
		if (IsTargetImmobile(target))
		{
			return 1f;
		}
		float num = CasterPawn.GetStatValue(StatDefOf.MeleeHitChance);
		if (ModsConfig.IdeologyActive && target.HasThing)
		{
			if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
			{
				num += caster.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsLitOffset);
			}
			else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
			{
				num += caster.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsDarkOffset);
			}
			else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
			{
				num += caster.GetStatValue(StatDefOf.MeleeHitChanceIndoorsDarkOffset);
			}
			else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
			{
				num += caster.GetStatValue(StatDefOf.MeleeHitChanceIndoorsLitOffset);
			}
		}
		return num;
	}

	private float GetDodgeChance(LocalTargetInfo target)
	{
		if (surpriseAttack)
		{
			return 0f;
		}
		if (IsTargetImmobile(target))
		{
			return 0f;
		}
		if (!(target.Thing is Pawn pawn))
		{
			return 0f;
		}
		if (pawn.stances.curStance is Stance_Busy { verb: not null } stance_Busy && !stance_Busy.verb.verbProps.IsMeleeAttack)
		{
			return 0f;
		}
		float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
		if (ModsConfig.IdeologyActive)
		{
			if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
			{
				num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset);
			}
			else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
			{
				num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset);
			}
			else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
			{
				num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsDarkOffset);
			}
			else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
			{
				num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsLitOffset);
			}
		}
		return num;
	}

	private bool IsTargetImmobile(LocalTargetInfo target)
	{
		Thing thing = target.Thing;
		Pawn pawn = thing as Pawn;
		if (thing.def.category == ThingCategory.Pawn && !pawn.Downed)
		{
			return pawn.GetPosture() != PawnPosture.Standing;
		}
		return true;
	}

	protected abstract DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target);

	private bool CanApplyMeleeSlaveSuppression(Pawn targetPawn)
	{
		if (CasterPawn != null && CasterPawn.IsColonist && !CasterPawn.IsSlave && targetPawn != null && targetPawn.IsSlaveOfColony && targetPawn.health.capacities.CanBeAwake)
		{
			return !SlaveRebellionUtility.IsRebelling(targetPawn);
		}
		return false;
	}

	private void ApplyMeleeSlaveSuppression(Pawn targetPawn, float damageDealt)
	{
		if (CanApplyMeleeSlaveSuppression(targetPawn))
		{
			SlaveRebellionUtility.IncrementMeleeSuppression(CasterPawn, targetPawn, damageDealt);
		}
	}

	private SoundDef SoundHitPawn()
	{
		if (base.EquipmentSource != null && !base.EquipmentSource.def.meleeHitSound.NullOrUndefined())
		{
			return base.EquipmentSource.def.meleeHitSound;
		}
		if (tool != null && !tool.soundMeleeHit.NullOrUndefined())
		{
			return tool.soundMeleeHit;
		}
		if (base.EquipmentSource != null && base.EquipmentSource.Stuff != null)
		{
			if (verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
			{
				if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
				{
					return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp;
				}
			}
			else if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
			{
				return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt;
			}
		}
		if (CasterPawn != null && !CasterPawn.def.race.soundMeleeHitPawn.NullOrUndefined())
		{
			return CasterPawn.def.race.soundMeleeHitPawn;
		}
		return SoundDefOf.Pawn_Melee_Punch_HitPawn;
	}

	private SoundDef SoundHitBuilding()
	{
		if (currentTarget.Thing is Building building && !building.def.building.soundMeleeHitOverride.NullOrUndefined())
		{
			return building.def.building.soundMeleeHitOverride;
		}
		if (base.EquipmentSource != null && !base.EquipmentSource.def.meleeHitSound.NullOrUndefined())
		{
			return base.EquipmentSource.def.meleeHitSound;
		}
		if (tool != null && !tool.soundMeleeHit.NullOrUndefined())
		{
			return tool.soundMeleeHit;
		}
		if (base.EquipmentSource != null && base.EquipmentSource.Stuff != null)
		{
			if (verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
			{
				if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
				{
					return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp;
				}
			}
			else if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
			{
				return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt;
			}
		}
		if (CasterPawn != null && !CasterPawn.def.race.soundMeleeHitBuilding.NullOrUndefined())
		{
			return CasterPawn.def.race.soundMeleeHitBuilding;
		}
		return SoundDefOf.MeleeHit_Unarmed;
	}

	private SoundDef SoundMiss()
	{
		if (CasterPawn != null)
		{
			if (tool != null && !tool.soundMeleeMiss.NullOrUndefined())
			{
				return tool.soundMeleeMiss;
			}
			if (!CasterPawn.def.race.soundMeleeMiss.NullOrUndefined())
			{
				return CasterPawn.def.race.soundMeleeMiss;
			}
		}
		return SoundDefOf.Pawn_Melee_Punch_Miss;
	}

	private SoundDef SoundDodge(Thing target)
	{
		if (target.def.race != null && target.def.race.soundMeleeDodge != null)
		{
			return target.def.race.soundMeleeDodge;
		}
		return SoundMiss();
	}
}
