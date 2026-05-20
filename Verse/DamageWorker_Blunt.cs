using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class DamageWorker_Blunt : DamageWorker_AddInjury
{
	protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
	{
		return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
	}

	protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
	{
		bool flag = Rand.Chance(def.bluntInnerHitChance);
		float num = (flag ? def.bluntInnerHitDamageFractionToConvert.RandomInRange : 0f);
		float num2 = totalDamage * (1f - num);
		DamageInfo lastInfo = dinfo;
		while (true)
		{
			num2 -= FinalizeAndAddInjury(pawn, num2, lastInfo, result);
			if (!pawn.health.hediffSet.PartIsMissing(lastInfo.HitPart) || num2 <= 1f)
			{
				break;
			}
			BodyPartRecord parent = lastInfo.HitPart.parent;
			if (parent == null)
			{
				break;
			}
			lastInfo.SetHitPart(parent);
		}
		if (flag && !lastInfo.HitPart.def.IsSolid(lastInfo.HitPart, pawn.health.hediffSet.hediffs) && lastInfo.HitPart.depth == BodyPartDepth.Outside && (from bodyPartRecord in pawn.health.hediffSet.GetNotMissingParts()
			where bodyPartRecord.parent == lastInfo.HitPart && bodyPartRecord.def.IsSolid(bodyPartRecord, pawn.health.hediffSet.hediffs) && bodyPartRecord.depth == BodyPartDepth.Inside
			select bodyPartRecord).TryRandomElementByWeight((BodyPartRecord bodyPartRecord) => bodyPartRecord.coverageAbs, out var result2))
		{
			DamageInfo dinfo2 = lastInfo;
			dinfo2.SetHitPart(result2);
			float totalDamage2 = totalDamage * num + totalDamage * def.bluntInnerHitDamageFractionToAdd.RandomInRange;
			FinalizeAndAddInjury(pawn, totalDamage2, dinfo2, result);
		}
		if (pawn.Dead)
		{
			return;
		}
		SimpleCurve simpleCurve = null;
		if (lastInfo.HitPart.parent == null)
		{
			simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
		}
		else
		{
			foreach (BodyPartRecord item in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource))
			{
				if (InSameBranch(item, lastInfo.HitPart))
				{
					simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
					break;
				}
			}
		}
		if (simpleCurve != null)
		{
			float x = totalDamage / pawn.def.race.body.corePart.def.GetMaxHealth(pawn);
			if (Rand.Chance(simpleCurve.Evaluate(x)))
			{
				DamageInfo dinfo3 = dinfo;
				dinfo3.Def = DamageDefOf.Stun;
				dinfo3.SetAmount((float)def.bluntStunDuration.SecondsToTicks() / 30f);
				pawn.TakeDamage(dinfo3);
			}
		}
	}

	[DebugOutput]
	public static void StunChances()
	{
		Func<ThingDef, float, bool, string> bluntBodyStunChance = delegate(ThingDef d, float dam, bool onHead)
		{
			SimpleCurve obj = (onHead ? DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToHeadCurve : DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToBodyCurve);
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(d.race.AnyPawnKind, Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionDef), PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true));
			float x = dam / d.race.body.corePart.def.GetMaxHealth(pawn);
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			return Mathf.Clamp01(obj.Evaluate(x)).ToStringPercent();
		};
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
		list.Add(new TableDataGetter<ThingDef>("body size", (ThingDef d) => d.race.baseBodySize.ToString("F2")));
		list.Add(new TableDataGetter<ThingDef>("health scale", (ThingDef d) => d.race.baseHealthScale.ToString("F2")));
		list.Add(new TableDataGetter<ThingDef>("body size\n* health scale", (ThingDef d) => (d.race.baseHealthScale * d.race.baseBodySize).ToString("F2")));
		list.Add(new TableDataGetter<ThingDef>("core part\nhealth", delegate(ThingDef d)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(d.race.AnyPawnKind, Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionDef), PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true));
			float maxHealth = d.race.body.corePart.def.GetMaxHealth(pawn);
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			return maxHealth;
		}));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n5", (ThingDef d) => bluntBodyStunChance(d, 5f, arg3: false)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n10", (ThingDef d) => bluntBodyStunChance(d, 10f, arg3: false)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n15", (ThingDef d) => bluntBodyStunChance(d, 15f, arg3: false)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n20", (ThingDef d) => bluntBodyStunChance(d, 20f, arg3: false)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n5", (ThingDef d) => bluntBodyStunChance(d, 5f, arg3: true)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n10", (ThingDef d) => bluntBodyStunChance(d, 10f, arg3: true)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n15", (ThingDef d) => bluntBodyStunChance(d, 15f, arg3: true)));
		list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n20", (ThingDef d) => bluntBodyStunChance(d, 20f, arg3: true)));
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn), list.ToArray());
	}

	private bool InSameBranch(BodyPartRecord lhs, BodyPartRecord rhs)
	{
		while (lhs.parent != null && lhs.parent.parent != null)
		{
			lhs = lhs.parent;
		}
		while (rhs.parent != null && rhs.parent.parent != null)
		{
			rhs = rhs.parent;
		}
		return lhs == rhs;
	}
}
