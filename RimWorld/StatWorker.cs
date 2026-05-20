using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatWorker
{
	public const int IGNORE_CACHE = -1;

	private Dictionary<Thing, StatCacheEntry> temporaryStatCache;

	private ConcurrentDictionary<Thing, float> immutableStatCache;

	protected StatDef stat;

	public void InitSetStat(StatDef newStat)
	{
		stat = newStat;
	}

	public void SetCacheability(bool immutable)
	{
		immutableStatCache = (immutable ? new ConcurrentDictionary<Thing, float>() : null);
		if (stat.cacheable)
		{
			temporaryStatCache = new Dictionary<Thing, StatCacheEntry>();
		}
	}

	public float GetValue(Thing thing, bool applyPostProcess = true, int cacheStaleAfterTicks = -1)
	{
		if (stat.immutable)
		{
			if (immutableStatCache.TryGetValue(thing, out var value))
			{
				return value;
			}
			float value2 = GetValue(StatRequest.For(thing), applyPostProcess);
			immutableStatCache[thing] = value2;
			return value2;
		}
		int ticksGame = Find.TickManager.TicksGame;
		if (cacheStaleAfterTicks != -1 && temporaryStatCache != null && temporaryStatCache.TryGetValue(thing, out var value3) && ticksGame - value3.gameTick < cacheStaleAfterTicks)
		{
			return value3.statValue;
		}
		float value4 = GetValue(StatRequest.For(thing));
		if (temporaryStatCache != null)
		{
			if (!temporaryStatCache.ContainsKey(thing))
			{
				temporaryStatCache[thing] = new StatCacheEntry(value4, ticksGame);
			}
			else
			{
				value3 = temporaryStatCache[thing];
				value3.statValue = value4;
				value3.gameTick = ticksGame;
				temporaryStatCache[thing] = value3;
			}
		}
		return value4;
	}

	public float GetValue(Thing thing, Pawn pawn, bool applyPostProcess = true)
	{
		return GetValue(StatRequest.For(thing, pawn), applyPostProcess);
	}

	public float GetValue(StatRequest req, bool applyPostProcess = true)
	{
		if (stat.minifiedThingInherits && req.Thing is MinifiedThing minifiedThing)
		{
			if (minifiedThing.InnerThing != null)
			{
				return minifiedThing.InnerThing.GetStatValue(stat, applyPostProcess);
			}
			Log.Error("MinifiedThing's inner thing is null.");
		}
		float val = GetValueUnfinalized(req, applyPostProcess);
		FinalizeValue(req, ref val, applyPostProcess);
		return val;
	}

	public float GetValueAbstract(BuildableDef def, ThingDef stuffDef = null)
	{
		return GetValue(StatRequest.For(def, stuffDef));
	}

	public float GetValueAbstract(AbilityDef def, Pawn forPawn = null)
	{
		return GetValue(StatRequest.For(def, forPawn));
	}

	public virtual float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		if (!stat.supressDisabledError && Prefs.DevMode && IsDisabledFor(req.Thing))
		{
			Log.ErrorOnce($"Attempted to calculate value for disabled stat {stat}; this is meant as a consistency check, either set the stat to neverDisabled or ensure this pawn cannot accidentally use this stat (thing={req.Thing.ToStringSafe()})", 75193282 + stat.index);
		}
		float num = GetBaseValueFor(req);
		Pawn pawn = req.Thing as Pawn;
		if (pawn != null)
		{
			if (pawn.skills != null)
			{
				if (stat.skillNeedOffsets != null)
				{
					for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
					{
						num += stat.skillNeedOffsets[i].ValueFor(pawn);
					}
				}
			}
			else
			{
				num += stat.noSkillOffset;
			}
			if (stat.capacityOffsets != null)
			{
				for (int j = 0; j < stat.capacityOffsets.Count; j++)
				{
					PawnCapacityOffset pawnCapacityOffset = stat.capacityOffsets[j];
					num += pawnCapacityOffset.GetOffset(pawn.health.capacities.GetLevel(pawnCapacityOffset.capacity));
				}
			}
			if (pawn.story != null)
			{
				for (int k = 0; k < pawn.story.traits.allTraits.Count; k++)
				{
					if (!pawn.story.traits.allTraits[k].Suppressed)
					{
						num += pawn.story.traits.allTraits[k].OffsetOfStat(stat);
					}
				}
			}
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int l = 0; l < hediffs.Count; l++)
			{
				HediffStage curStage = hediffs[l].CurStage;
				if (curStage != null)
				{
					num += HediffStatsUtility.GetStatOffsetForSeverity(stat, curStage, pawn, hediffs[l].Severity);
				}
			}
			if (pawn.Ideo != null)
			{
				List<Precept> preceptsListForReading = pawn.Ideo.PreceptsListForReading;
				for (int m = 0; m < preceptsListForReading.Count; m++)
				{
					if (preceptsListForReading[m].def.statOffsets != null)
					{
						num += preceptsListForReading[m].def.statOffsets.GetStatOffsetFromList(stat);
					}
					if (preceptsListForReading[m].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int n = 0; n < preceptsListForReading[m].def.conditionalStatAffecters.Count; n++)
					{
						ConditionalStatAffecter conditionalStatAffecter = preceptsListForReading[m].def.conditionalStatAffecters[n];
						if (conditionalStatAffecter.statOffsets != null && conditionalStatAffecter.Applies(req))
						{
							num += conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
						}
					}
				}
				Precept_Role role = pawn.Ideo.GetRole(pawn);
				if (role?.def.roleEffects != null)
				{
					foreach (RoleEffect roleEffect in role.def.roleEffects)
					{
						if (roleEffect is RoleEffect_PawnStatOffset roleEffect_PawnStatOffset && roleEffect_PawnStatOffset.statDef == stat)
						{
							num += roleEffect_PawnStatOffset.modifier;
						}
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				List<Gene> genesListForReading = pawn.genes.GenesListForReading;
				for (int num2 = 0; num2 < genesListForReading.Count; num2++)
				{
					if (!genesListForReading[num2].Active)
					{
						continue;
					}
					num += genesListForReading[num2].def.statOffsets.GetStatOffsetFromList(stat);
					if (genesListForReading[num2].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int num3 = 0; num3 < genesListForReading[num2].def.conditionalStatAffecters.Count; num3++)
					{
						ConditionalStatAffecter conditionalStatAffecter2 = genesListForReading[num2].def.conditionalStatAffecters[num3];
						if (conditionalStatAffecter2.Applies(req))
						{
							num += conditionalStatAffecter2.statOffsets.GetStatOffsetFromList(stat);
						}
					}
				}
			}
			num += pawn.ageTracker.CurLifeStage.statOffsets.GetStatOffsetFromList(stat);
			if (pawn.apparel != null)
			{
				for (int num4 = 0; num4 < pawn.apparel.WornApparel.Count; num4++)
				{
					num += StatOffsetFromGear(pawn.apparel.WornApparel[num4], stat);
				}
			}
			if (pawn.equipment?.Primary != null)
			{
				num += StatOffsetFromGear(pawn.equipment.Primary, stat);
			}
			if (pawn.story != null)
			{
				for (int num5 = 0; num5 < pawn.story.traits.allTraits.Count; num5++)
				{
					if (!pawn.story.traits.allTraits[num5].Suppressed)
					{
						num *= pawn.story.traits.allTraits[num5].MultiplierOfStat(stat);
					}
				}
			}
			for (int num6 = 0; num6 < hediffs.Count; num6++)
			{
				HediffStage curStage2 = hediffs[num6].CurStage;
				if (curStage2 != null)
				{
					num *= HediffStatsUtility.GetStatFactorForSeverity(stat, curStage2, pawn, hediffs[num6].Severity);
				}
			}
			if (pawn.Ideo != null)
			{
				List<Precept> preceptsListForReading2 = pawn.Ideo.PreceptsListForReading;
				for (int num7 = 0; num7 < preceptsListForReading2.Count; num7++)
				{
					if (preceptsListForReading2[num7].def.statFactors != null)
					{
						num *= preceptsListForReading2[num7].def.statFactors.GetStatFactorFromList(stat);
					}
					if (preceptsListForReading2[num7].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int num8 = 0; num8 < preceptsListForReading2[num7].def.conditionalStatAffecters.Count; num8++)
					{
						ConditionalStatAffecter conditionalStatAffecter3 = preceptsListForReading2[num7].def.conditionalStatAffecters[num8];
						if (conditionalStatAffecter3.statFactors != null && conditionalStatAffecter3.Applies(req))
						{
							num *= conditionalStatAffecter3.statFactors.GetStatFactorFromList(stat);
						}
					}
				}
				Precept_Role role2 = pawn.Ideo.GetRole(pawn);
				if (role2?.def.roleEffects != null)
				{
					foreach (RoleEffect roleEffect2 in role2.def.roleEffects)
					{
						if (roleEffect2 is RoleEffect_PawnStatFactor roleEffect_PawnStatFactor && roleEffect_PawnStatFactor.statDef == stat)
						{
							num *= roleEffect_PawnStatFactor.modifier;
						}
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				List<Gene> genesListForReading2 = pawn.genes.GenesListForReading;
				for (int num9 = 0; num9 < genesListForReading2.Count; num9++)
				{
					if (!genesListForReading2[num9].Active)
					{
						continue;
					}
					num *= genesListForReading2[num9].def.statFactors.GetStatFactorFromList(stat);
					if (genesListForReading2[num9].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int num10 = 0; num10 < genesListForReading2[num9].def.conditionalStatAffecters.Count; num10++)
					{
						ConditionalStatAffecter conditionalStatAffecter4 = genesListForReading2[num9].def.conditionalStatAffecters[num10];
						if (conditionalStatAffecter4.Applies(req))
						{
							num *= conditionalStatAffecter4.statFactors.GetStatFactorFromList(stat);
						}
					}
				}
			}
			num *= pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
		}
		if (req.StuffDef != null)
		{
			if (num > 0f || stat.applyFactorsIfNegative)
			{
				num *= req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
				if (req.Thing.TryGetQuality(out var qc))
				{
					num *= req.StuffDef.stuffProps.statFactorsQuality.GetStatFactorFromList(stat, qc);
				}
			}
			num += req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
			if (req.Thing.TryGetQuality(out var qc2))
			{
				num += req.StuffDef.stuffProps.statOffsetsQuality.GetStatOffsetFromList(stat, qc2);
			}
		}
		if (req.ForAbility)
		{
			if (stat.statFactors != null)
			{
				for (int num11 = 0; num11 < stat.statFactors.Count; num11++)
				{
					num *= req.AbilityDef.statBases.GetStatValueFromList(stat.statFactors[num11], 1f);
				}
			}
			Pawn pawn2 = req.Pawn;
			if (pawn2?.Ideo != null)
			{
				List<Precept> preceptsListForReading3 = pawn2.Ideo.PreceptsListForReading;
				for (int num12 = 0; num12 < preceptsListForReading3.Count; num12++)
				{
					if (preceptsListForReading3[num12].def.statFactors != null)
					{
						num *= preceptsListForReading3[num12].def.statFactors.GetStatFactorFromList(stat);
					}
					if (preceptsListForReading3[num12].def.abilityStatFactors == null)
					{
						continue;
					}
					foreach (AbilityStatModifiers abilityStatFactor in preceptsListForReading3[num12].def.abilityStatFactors)
					{
						if (abilityStatFactor.ability == req.AbilityDef)
						{
							num *= abilityStatFactor.modifiers.GetStatFactorFromList(stat);
						}
					}
				}
			}
		}
		if (req.HasThing)
		{
			if (req.Thing is ThingWithComps { AllComps: var allComps })
			{
				for (int num13 = 0; num13 < allComps.Count; num13++)
				{
					num += allComps[num13].GetStatOffset(stat);
				}
				for (int num14 = 0; num14 < allComps.Count; num14++)
				{
					num *= allComps[num14].GetStatFactor(stat);
				}
			}
			if (stat.statFactors != null)
			{
				for (int num15 = 0; num15 < stat.statFactors.Count; num15++)
				{
					num *= req.Thing.GetStatValue(stat.statFactors[num15]);
				}
			}
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedFactors != null)
					{
						for (int num16 = 0; num16 < stat.skillNeedFactors.Count; num16++)
						{
							num *= stat.skillNeedFactors[num16].ValueFor(pawn);
						}
					}
				}
				else
				{
					num *= stat.noSkillFactor;
				}
				if (stat.capacityFactors != null)
				{
					for (int num17 = 0; num17 < stat.capacityFactors.Count; num17++)
					{
						PawnCapacityFactor pawnCapacityFactor = stat.capacityFactors[num17];
						float factor = pawnCapacityFactor.GetFactor(pawn.health.capacities.GetLevel(pawnCapacityFactor.capacity));
						num = Mathf.Lerp(num, num * factor, pawnCapacityFactor.weight);
					}
				}
				if (pawn.Inspired)
				{
					num += pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
					num *= pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
				}
			}
		}
		return num;
	}

	public virtual string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		float baseValueFor = GetBaseValueFor(req);
		if (baseValueFor != 0f || stat.showZeroBaseValue)
		{
			stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(baseValueFor, numberSense));
		}
		GetOffsetsAndFactorsExplanation(req, stringBuilder, baseValueFor);
		return stringBuilder.ToString();
	}

	public void GetOffsetsAndFactorsExplanation(StatRequest req, StringBuilder sb, float baseValue, string whitespace = "")
	{
		Pawn pawn = req.Thing as Pawn;
		if (pawn != null)
		{
			if (pawn.skills != null)
			{
				if (stat.skillNeedOffsets != null)
				{
					sb.AppendLine(whitespace + "StatsReport_Skills".Translate());
					for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
					{
						SkillNeed skillNeed = stat.skillNeedOffsets[i];
						int level = pawn.skills.GetSkill(skillNeed.skill).Level;
						float val = skillNeed.ValueFor(pawn);
						sb.AppendLine(string.Concat("    " + skillNeed.skill.LabelCap + " (", level.ToString(), "): ", val.ToStringSign(), ValueToString(val, finalized: false)));
					}
				}
			}
			else if (stat.noSkillOffset != 0f)
			{
				sb.AppendLine(whitespace + "StatsReport_Skills".Translate());
				sb.AppendLine(whitespace + "    " + "default".Translate().CapitalizeFirst() + " : " + stat.noSkillOffset.ToStringSign() + ValueToString(stat.noSkillOffset, finalized: false));
			}
			if (stat.capacityOffsets != null)
			{
				sb.AppendLine(whitespace + ("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate()));
				foreach (PawnCapacityOffset item in stat.capacityOffsets.OrderBy((PawnCapacityOffset hfa) => hfa.capacity.listOrder))
				{
					string text = item.capacity.GetLabelFor(pawn).CapitalizeFirst();
					float level2 = pawn.health.capacities.GetLevel(item.capacity);
					float offset = item.GetOffset(pawn.health.capacities.GetLevel(item.capacity));
					string text2 = ValueToString(offset, finalized: false);
					string text3 = Mathf.Min(level2, item.max).ToStringPercent() + ", " + "HealthOffsetScale".Translate(item.scale + "x");
					if (item.max < 999f)
					{
						text3 += ", " + "HealthFactorMaxImpact".Translate(item.max.ToStringPercent());
					}
					sb.AppendLine(whitespace + "    " + text + ": " + offset.ToStringSign() + text2 + " (" + text3 + ")");
				}
			}
			if ((int)pawn.RaceProps.intelligence >= 1)
			{
				if (pawn.story?.traits != null)
				{
					List<Trait> list = pawn.story.traits.allTraits.Where((Trait tr) => !tr.Suppressed && tr.CurrentData.statOffsets != null && tr.CurrentData.statOffsets.Any((StatModifier se) => se.stat == stat)).ToList();
					List<Trait> list2 = pawn.story.traits.allTraits.Where((Trait tr) => !tr.Suppressed && tr.CurrentData.statFactors != null && tr.CurrentData.statFactors.Any((StatModifier se) => se.stat == stat)).ToList();
					if (list.Count > 0 || list2.Count > 0)
					{
						sb.AppendLine(whitespace + "StatsReport_RelevantTraits".Translate());
						for (int num = 0; num < list.Count; num++)
						{
							Trait trait = list[num];
							string valueToStringAsOffset = trait.CurrentData.statOffsets.First((StatModifier se) => se.stat == stat).ValueToStringAsOffset;
							sb.AppendLine(whitespace + "    " + trait.LabelCap + ": " + valueToStringAsOffset);
						}
						for (int num2 = 0; num2 < list2.Count; num2++)
						{
							Trait trait2 = list2[num2];
							string toStringAsFactor = trait2.CurrentData.statFactors.First((StatModifier se) => se.stat == stat).ToStringAsFactor;
							sb.AppendLine(whitespace + "    " + trait2.LabelCap + ": " + toStringAsFactor);
						}
					}
				}
				if (RelevantGear(pawn, stat).Any())
				{
					sb.AppendLine(whitespace + "StatsReport_RelevantGear".Translate());
					if (pawn.apparel != null)
					{
						for (int num3 = 0; num3 < pawn.apparel.WornApparel.Count; num3++)
						{
							Apparel apparel = pawn.apparel.WornApparel[num3];
							if (GearAffectsStat(apparel.def, stat))
							{
								sb.AppendLine(whitespace + InfoTextLineFromGear(apparel, stat));
							}
						}
					}
					if (pawn.equipment?.Primary != null && (GearAffectsStat(pawn.equipment.Primary.def, stat) || GearHasCompsThatAffectStat(pawn.equipment.Primary, stat)))
					{
						sb.AppendLine(whitespace + InfoTextLineFromGear(pawn.equipment.Primary, stat));
					}
				}
			}
			bool flag = false;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int num4 = 0; num4 < hediffs.Count; num4++)
			{
				HediffStage curStage = hediffs[num4].CurStage;
				if (curStage == null)
				{
					continue;
				}
				float num5 = curStage.statOffsets.GetStatOffsetFromList(stat);
				if (num5 != 0f)
				{
					float val2 = num5;
					if (curStage.statOffsetEffectMultiplier != null)
					{
						num5 *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
					}
					if (curStage.multiplyStatChangesBySeverity)
					{
						num5 *= hediffs[num4].Severity;
					}
					if (!flag)
					{
						sb.AppendLine(whitespace + "StatsReport_RelevantHediffs".Translate());
						flag = true;
					}
					sb.Append(whitespace + "    " + hediffs[num4].LabelBaseCap + ": " + ValueToString(num5, finalized: false, ToStringNumberSense.Offset));
					if (curStage.statOffsetEffectMultiplier != null)
					{
						sb.Append(whitespace + " (" + ValueToString(val2, finalized: false, ToStringNumberSense.Offset) + " x " + ValueToString(pawn.GetStatValue(curStage.statOffsetEffectMultiplier), finalized: true, curStage.statOffsetEffectMultiplier.toStringNumberSense) + " " + curStage.statOffsetEffectMultiplier.LabelCap + ")");
					}
					sb.AppendLine();
				}
				float num6 = curStage.statFactors.GetStatFactorFromList(stat);
				if (Math.Abs(num6 - 1f) > float.Epsilon)
				{
					float val3 = num6;
					if (curStage.multiplyStatChangesBySeverity)
					{
						num6 = ScaleFactor(num6, hediffs[num4].Severity);
					}
					if (curStage.statFactorEffectMultiplier != null)
					{
						num6 = ScaleFactor(num6, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
					}
					if (!flag)
					{
						sb.AppendLine(whitespace + "StatsReport_RelevantHediffs".Translate());
						flag = true;
					}
					sb.Append("    " + hediffs[num4].LabelBaseCap + ": " + ValueToString(num6, finalized: false, ToStringNumberSense.Factor));
					if (curStage.statFactorEffectMultiplier != null)
					{
						sb.Append(whitespace + " (" + ValueToString(val3, finalized: false, ToStringNumberSense.Factor) + " x " + ValueToString(pawn.GetStatValue(curStage.statFactorEffectMultiplier), finalized: false) + " " + curStage.statFactorEffectMultiplier.LabelCap + ")");
					}
					sb.AppendLine();
				}
			}
			if (pawn.Ideo != null)
			{
				List<Precept> preceptsListForReading = pawn.Ideo.PreceptsListForReading;
				for (int num7 = 0; num7 < preceptsListForReading.Count; num7++)
				{
					float statOffsetFromList = preceptsListForReading[num7].def.statOffsets.GetStatOffsetFromList(stat);
					if (statOffsetFromList != 0f)
					{
						sb.AppendLine(whitespace + "StatsReport_Ideoligion".Translate() + ": " + ValueToString(statOffsetFromList, finalized: false, ToStringNumberSense.Offset));
					}
					float statFactorFromList = preceptsListForReading[num7].def.statFactors.GetStatFactorFromList(stat);
					if (Math.Abs(statFactorFromList - 1f) > float.Epsilon)
					{
						sb.AppendLine(whitespace + "StatsReport_Ideoligion".Translate() + ": " + ValueToString(statFactorFromList, finalized: false, ToStringNumberSense.Factor));
					}
					if (preceptsListForReading[num7].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int num8 = 0; num8 < preceptsListForReading[num7].def.conditionalStatAffecters.Count; num8++)
					{
						ConditionalStatAffecter conditionalStatAffecter = preceptsListForReading[num7].def.conditionalStatAffecters[num8];
						if (conditionalStatAffecter.Applies(req))
						{
							float statOffsetFromList2 = conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
							if (statOffsetFromList2 != 0f)
							{
								sb.AppendLine(whitespace + "StatsReport_Ideoligion".Translate() + ": " + ValueToString(statOffsetFromList2, finalized: false, ToStringNumberSense.Offset));
							}
							float statFactorFromList2 = conditionalStatAffecter.statFactors.GetStatFactorFromList(stat);
							if (statFactorFromList2 != 1f)
							{
								sb.AppendLine(whitespace + "StatsReport_Ideoligion".Translate() + ": " + ValueToString(statFactorFromList2, finalized: false, ToStringNumberSense.Factor));
							}
						}
					}
				}
				Precept_Role role = pawn.Ideo.GetRole(pawn);
				if (role?.def.roleEffects != null)
				{
					foreach (RoleEffect roleEffect in role.def.roleEffects)
					{
						if (roleEffect is RoleEffect_PawnStatOffset roleEffect_PawnStatOffset)
						{
							if (roleEffect_PawnStatOffset.statDef == stat)
							{
								sb.AppendLine(whitespace + role.LabelCap + ": " + ValueToString(roleEffect_PawnStatOffset.modifier, finalized: false, ToStringNumberSense.Offset));
							}
						}
						else if (roleEffect is RoleEffect_PawnStatFactor roleEffect_PawnStatFactor && roleEffect_PawnStatFactor.statDef == stat)
						{
							sb.AppendLine(whitespace + role.LabelCap + ": " + ValueToString(roleEffect_PawnStatFactor.modifier, finalized: false, ToStringNumberSense.Factor));
						}
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				bool flag2 = false;
				List<Gene> genesListForReading = pawn.genes.GenesListForReading;
				for (int num9 = 0; num9 < genesListForReading.Count; num9++)
				{
					if (!genesListForReading[num9].Active)
					{
						continue;
					}
					float statOffsetFromList3 = genesListForReading[num9].def.statOffsets.GetStatOffsetFromList(stat);
					if (statOffsetFromList3 != 0f)
					{
						if (!flag2)
						{
							sb.AppendLine(whitespace + "StatsReport_Genes".Translate());
							flag2 = true;
						}
						sb.AppendLine(whitespace + "    " + genesListForReading[num9].LabelCap + ": " + ValueToString(statOffsetFromList3, finalized: false, ToStringNumberSense.Offset));
					}
					float statFactorFromList3 = genesListForReading[num9].def.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList3 != 1f)
					{
						if (!flag2)
						{
							sb.AppendLine(whitespace + "StatsReport_Genes".Translate());
							flag2 = true;
						}
						sb.AppendLine(whitespace + "    " + genesListForReading[num9].LabelCap + ": " + ValueToString(statFactorFromList3, finalized: false, ToStringNumberSense.Factor));
					}
					if (genesListForReading[num9].def.conditionalStatAffecters == null)
					{
						continue;
					}
					for (int num10 = 0; num10 < genesListForReading[num9].def.conditionalStatAffecters.Count; num10++)
					{
						ConditionalStatAffecter conditionalStatAffecter2 = genesListForReading[num9].def.conditionalStatAffecters[num10];
						if (!conditionalStatAffecter2.Applies(req))
						{
							continue;
						}
						float statOffsetFromList4 = conditionalStatAffecter2.statOffsets.GetStatOffsetFromList(stat);
						if (statOffsetFromList4 != 0f)
						{
							if (!flag2)
							{
								sb.AppendLine(whitespace + "StatsReport_Genes".Translate());
								flag2 = true;
							}
							sb.AppendLine(whitespace + "    " + genesListForReading[num9].LabelCap + " (" + conditionalStatAffecter2.Label + "): " + ValueToString(statOffsetFromList4, finalized: false, ToStringNumberSense.Offset));
						}
						float statFactorFromList4 = conditionalStatAffecter2.statFactors.GetStatFactorFromList(stat);
						if (statFactorFromList4 != 1f)
						{
							if (!flag2)
							{
								sb.AppendLine(whitespace + "StatsReport_Genes".Translate());
								flag2 = true;
							}
							sb.AppendLine(whitespace + "    " + genesListForReading[num9].LabelCap + " (" + conditionalStatAffecter2.Label + "): " + ValueToString(statFactorFromList4, finalized: false, ToStringNumberSense.Factor));
						}
					}
				}
			}
			float statOffsetFromList5 = pawn.ageTracker.CurLifeStage.statOffsets.GetStatOffsetFromList(stat);
			if (statOffsetFromList5 != 0f)
			{
				sb.AppendLine(whitespace + "StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statOffsetFromList5.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
			}
			float statFactorFromList5 = pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
			if (statFactorFromList5 != 1f)
			{
				sb.AppendLine(whitespace + "StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statFactorFromList5.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
			}
		}
		if (req.StuffDef != null)
		{
			if (baseValue > 0f || stat.applyFactorsIfNegative)
			{
				float statFactorFromList6 = req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
				if (statFactorFromList6 != 1f)
				{
					sb.AppendLine(whitespace + "StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statFactorFromList6.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
				}
				if (req.StuffDef.stuffProps.statFactorsQuality != null && req.StuffDef.stuffProps.statFactorsQuality.TryGetStatFactorRangeFromList(stat, out var range))
				{
					string text4 = range.min.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
					string text5 = range.max.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
					sb.AppendLine(whitespace + string.Format("{0} ({1}): {2} ~ {3}", "StatsReport_Material".Translate(), req.StuffDef.LabelCap, text4, text5));
				}
			}
			float statOffsetFromList6 = req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
			if (statOffsetFromList6 != 0f)
			{
				sb.AppendLine(whitespace + "StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statOffsetFromList6.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
			}
			if (req.StuffDef.stuffProps.statOffsetsQuality != null && req.StuffDef.stuffProps.statOffsetsQuality.TryGetStatOffsetRangeFromList(stat, out var range2))
			{
				string text6 = range2.min.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
				string text7 = range2.max.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
				sb.AppendLine(whitespace + string.Format("{0} ({1}): {2} ~ {3}", "StatsReport_Material".Translate(), req.StuffDef.LabelCap, text6, text7));
			}
		}
		if (req.Thing is ThingWithComps { AllComps: var allComps })
		{
			for (int num11 = 0; num11 < allComps.Count; num11++)
			{
				allComps[num11].GetStatsExplanation(stat, sb, whitespace);
			}
		}
		if (stat.statFactors != null)
		{
			sb.AppendLine(whitespace + stat.statFactorsExplanationHeader);
			for (int num12 = 0; num12 < stat.statFactors.Count; num12++)
			{
				StatDef statDef = stat.statFactors[num12];
				sb.AppendLine(whitespace + "    " + statDef.LabelCap + ": x" + statDef.Worker.GetValue(req).ToStringPercent());
			}
		}
		if (pawn == null)
		{
			return;
		}
		if (pawn.skills != null)
		{
			if (stat.skillNeedFactors != null)
			{
				sb.AppendLine(whitespace + "StatsReport_Skills".Translate());
				for (int num13 = 0; num13 < stat.skillNeedFactors.Count; num13++)
				{
					SkillNeed skillNeed2 = stat.skillNeedFactors[num13];
					int level3 = pawn.skills.GetSkill(skillNeed2.skill).Level;
					sb.AppendLine(string.Concat(whitespace + "    " + skillNeed2.skill.LabelCap + " (", level3.ToString(), "): x", skillNeed2.ValueFor(pawn).ToStringPercent()));
				}
			}
		}
		else if (stat.noSkillFactor != 1f)
		{
			sb.AppendLine(whitespace + "StatsReport_Skills".Translate());
			sb.AppendLine(whitespace + "    " + "default".Translate().CapitalizeFirst() + " : x" + stat.noSkillFactor.ToStringPercent());
		}
		if (stat.capacityFactors != null)
		{
			sb.AppendLine(whitespace + ("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate()));
			if (stat.capacityFactors != null)
			{
				foreach (PawnCapacityFactor item2 in stat.capacityFactors.OrderBy((PawnCapacityFactor hfa) => hfa.capacity.listOrder))
				{
					string text8 = item2.capacity.GetLabelFor(pawn).CapitalizeFirst();
					string text9 = item2.GetFactor(pawn.health.capacities.GetLevel(item2.capacity)).ToStringPercent();
					string text10 = "HealthFactorPercentImpact".Translate(item2.weight.ToStringPercent());
					if (item2.max < 999f)
					{
						text10 += ", " + "HealthFactorMaxImpact".Translate(item2.max.ToStringPercent());
					}
					if (item2.allowedDefect != 0f)
					{
						text10 += ", " + "HealthFactorAllowedDefect".Translate((1f - item2.allowedDefect).ToStringPercent());
					}
					sb.AppendLine(whitespace + "    " + text8 + ": x" + text9 + " (" + text10 + ")");
				}
			}
		}
		if (pawn.Inspired)
		{
			float statOffsetFromList7 = pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
			if (statOffsetFromList7 != 0f)
			{
				sb.AppendLine(whitespace + "StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + ValueToString(statOffsetFromList7, finalized: false, ToStringNumberSense.Offset));
			}
			float statFactorFromList7 = pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
			if (statFactorFromList7 != 1f)
			{
				sb.AppendLine(whitespace + "StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + statFactorFromList7.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
			}
		}
	}

	public virtual void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
	{
		if (stat.parts != null)
		{
			for (int i = 0; i < stat.parts.Count; i++)
			{
				stat.parts[i].TransformValue(req, ref val);
			}
		}
		if (applyPostProcess && stat.postProcessCurve != null)
		{
			val = stat.postProcessCurve.Evaluate(val);
		}
		if (applyPostProcess && !stat.postProcessStatFactors.NullOrEmpty() && req.HasThing)
		{
			for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
			{
				val *= req.Thing.GetStatValue(stat.postProcessStatFactors[j]);
			}
		}
		if (Find.Scenario != null)
		{
			val *= Find.Scenario.GetStatFactor(stat);
		}
		if (Mathf.Abs(val) > stat.roundToFiveOver)
		{
			val = Mathf.Round(val / 5f) * 5f;
		}
		if (stat.roundValue)
		{
			val = Mathf.RoundToInt(val);
		}
		if (applyPostProcess)
		{
			val = Mathf.Clamp(val, stat.minValue, stat.maxValue);
		}
	}

	public virtual string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetAdditionalOffsetsAndFactorsExplanation(req, numberSense, stringBuilder);
		stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(finalVal, stat.toStringNumberSense));
		if (stat.displayMaxWhenAboveOrEqual && finalVal >= stat.maxValue)
		{
			stringBuilder.AppendInNewLine("StatsReport_MaxValue".Translate() + ": " + stat.ValueToString(stat.maxValue, stat.toStringNumberSense));
		}
		return stringBuilder.ToString();
	}

	public void GetAdditionalOffsetsAndFactorsExplanation(StatRequest req, ToStringNumberSense numberSense, StringBuilder sb, string whitespace = "")
	{
		if (stat.parts != null)
		{
			for (int i = 0; i < stat.parts.Count; i++)
			{
				string text = stat.parts[i].ExplanationPart(req);
				if (!text.NullOrEmpty())
				{
					sb.AppendLine(whitespace + text);
				}
			}
		}
		if (stat.postProcessCurve != null)
		{
			float value = GetValue(req, applyPostProcess: false);
			float num = stat.postProcessCurve.Evaluate(value);
			if (!Mathf.Approximately(value, num))
			{
				string text2 = ValueToString(value, finalized: false);
				string text3 = stat.ValueToString(num, numberSense);
				sb.AppendLine(whitespace + "StatsReport_PostProcessed".Translate() + ": " + text2 + " => " + text3);
			}
		}
		if (stat.postProcessStatFactors != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
			{
				if (stat.postProcessStatFactors[j].Worker.ShouldShowFor(req))
				{
					StatDef statDef = stat.postProcessStatFactors[j];
					stringBuilder.AppendLine(whitespace + $"    {statDef.LabelCap}: x{statDef.Worker.GetValue(req).ToStringPercent()}");
				}
			}
			if (stringBuilder.Length > 0)
			{
				sb.AppendLine(whitespace + "StatsReport_OtherStats".Translate());
				sb.AppendLine(stringBuilder.ToString());
			}
		}
		float statFactor = Find.Scenario.GetStatFactor(stat);
		if (statFactor != 1f)
		{
			sb.AppendLine(whitespace + "StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
		}
	}

	public string GetExplanationFull(StatRequest req, ToStringNumberSense numberSense, float value)
	{
		if (IsDisabledFor(req.Thing))
		{
			return "StatsReport_PermanentlyDisabled".Translate();
		}
		string text = stat.Worker.GetExplanationUnfinalized(req, numberSense).TrimEndNewlines();
		if (!text.NullOrEmpty())
		{
			text += "\n\n";
		}
		return text + stat.Worker.GetExplanationFinalizePart(req, numberSense, value);
	}

	public virtual bool ShouldShowFor(StatRequest req)
	{
		if (stat.alwaysHide)
		{
			return false;
		}
		Def def = req.Def;
		if (!stat.showIfUndefined && !req.StatBases.StatListContains(stat))
		{
			return false;
		}
		if (!stat.CanShowWithLoadedMods())
		{
			return false;
		}
		if (stat.hideInClassicMode && Find.IdeoManager.classicMode)
		{
			return false;
		}
		if (stat.parts != null)
		{
			foreach (StatPart part in stat.parts)
			{
				if (part.ForceShow(req))
				{
					return true;
				}
			}
		}
		if (req.Thing is Pawn pawn)
		{
			if (pawn.health != null && !stat.showIfHediffsPresent.NullOrEmpty())
			{
				for (int i = 0; i < stat.showIfHediffsPresent.Count; i++)
				{
					if (!pawn.health.hediffSet.HasHediff(stat.showIfHediffsPresent[i]))
					{
						return false;
					}
				}
			}
			if (stat.showOnSlavesOnly && !pawn.IsSlave)
			{
				return false;
			}
		}
		if (stat == StatDefOf.MaxHitPoints && req.HasThing)
		{
			return false;
		}
		if (!stat.showOnUntradeables && !DisplayTradeStats(req))
		{
			return false;
		}
		ThingDef thingDef = def as ThingDef;
		if (thingDef != null)
		{
			if (thingDef.category == ThingCategory.Pawn)
			{
				if (!stat.showOnPawns)
				{
					return false;
				}
				if (!stat.showOnHumanlikes && thingDef.race.Humanlike)
				{
					return false;
				}
				if (!stat.showOnNonWildManHumanlikes && thingDef.race.Humanlike && (!(req.Thing is Pawn p) || !p.IsWildMan()))
				{
					return false;
				}
				if (!stat.showOnAnimals && thingDef.race.Animal)
				{
					return false;
				}
				if (!stat.showOnEntities && thingDef.race.IsAnomalyEntity)
				{
					return false;
				}
				if (!stat.showOnMechanoids && thingDef.race.IsMechanoid)
				{
					return false;
				}
				if (!stat.showOnDrones && thingDef.race.IsDrone)
				{
					return false;
				}
				if (req.Thing is Pawn pawn2 && !stat.showDevelopmentalStageFilter.Has(pawn2.DevelopmentalStage))
				{
					return false;
				}
			}
			if (!stat.showOnUnhaulables && !thingDef.EverHaulable && !thingDef.Minifiable)
			{
				return false;
			}
		}
		if (stat.category == StatCategoryDefOf.BasicsPawn || stat.category == StatCategoryDefOf.BasicsPawnImportant || stat.category == StatCategoryDefOf.PawnCombat || stat.category == StatCategoryDefOf.Animals || stat.category == StatCategoryDefOf.PawnResistances || stat.category == StatCategoryDefOf.PawnHealth || stat.category == StatCategoryDefOf.PawnFood || stat.category == StatCategoryDefOf.PawnPsyfocus)
		{
			if (thingDef != null)
			{
				return thingDef.category == ThingCategory.Pawn;
			}
			return false;
		}
		if (stat.category == StatCategoryDefOf.PawnMisc || stat.category == StatCategoryDefOf.PawnSocial || stat.category == StatCategoryDefOf.PawnWork)
		{
			if (thingDef == null || thingDef.category != ThingCategory.Pawn)
			{
				return false;
			}
			if (req.Thing is Pawn pawn3)
			{
				if (pawn3.IsColonyMech && stat.showOnPlayerMechanoids)
				{
					return true;
				}
				if (stat.showOnPawnKind.NotNullAndContains(pawn3.kindDef))
				{
					return true;
				}
			}
			return thingDef.race.Humanlike;
		}
		if (stat.category == StatCategoryDefOf.Building)
		{
			if (thingDef == null)
			{
				return false;
			}
			if (stat == StatDefOf.DoorOpenSpeed)
			{
				return thingDef.IsDoor;
			}
			if (!stat.showOnNonWorkTables && !thingDef.IsWorkTable)
			{
				return false;
			}
			if (!stat.showOnNonPowerPlants && !thingDef.HasAssignableCompFrom(typeof(CompPowerPlant)))
			{
				return false;
			}
			return thingDef.category == ThingCategory.Building;
		}
		if (stat.category == StatCategoryDefOf.Apparel)
		{
			if (thingDef != null)
			{
				if (!thingDef.IsApparel)
				{
					return thingDef.category == ThingCategory.Pawn;
				}
				return true;
			}
			return false;
		}
		if (stat.category == StatCategoryDefOf.Weapon)
		{
			if (thingDef != null)
			{
				if (!thingDef.IsMeleeWeapon)
				{
					return thingDef.IsRangedWeapon;
				}
				return true;
			}
			return false;
		}
		if (stat.category == StatCategoryDefOf.Weapon_Ranged)
		{
			return thingDef?.IsRangedWeapon ?? false;
		}
		if (stat.category == StatCategoryDefOf.Weapon_Melee)
		{
			return thingDef?.IsMeleeWeapon ?? false;
		}
		if (stat.category == StatCategoryDefOf.BasicsNonPawn || stat.category == StatCategoryDefOf.BasicsNonPawnImportant)
		{
			if (thingDef == null || thingDef.category != ThingCategory.Pawn)
			{
				return !req.ForAbility;
			}
			return false;
		}
		if (stat.category == StatCategoryDefOf.Terrain)
		{
			return def is TerrainDef;
		}
		if (ModsConfig.AnomalyActive && stat.category == StatCategoryDefOf.PsychicRituals)
		{
			return false;
		}
		if (req.ForAbility)
		{
			return stat.category == StatCategoryDefOf.Ability;
		}
		if (stat.category.displayAllByDefault)
		{
			return true;
		}
		Log.Error("Unhandled case: " + stat?.ToString() + ", " + def);
		return false;
	}

	public virtual bool IsDisabledFor(Thing thing)
	{
		if (stat.neverDisabled)
		{
			return false;
		}
		if (stat.skillNeedFactors.NullOrEmpty() && stat.skillNeedOffsets.NullOrEmpty() && stat.disableIfSkillDisabled == null)
		{
			return false;
		}
		if (thing is Pawn { story: not null } pawn)
		{
			if (stat.skillNeedFactors != null)
			{
				foreach (SkillNeed skillNeedFactor in stat.skillNeedFactors)
				{
					if (skillNeedFactor.required && pawn.skills.GetSkill(skillNeedFactor.skill).TotallyDisabled)
					{
						return true;
					}
				}
			}
			if (stat.skillNeedOffsets != null)
			{
				foreach (SkillNeed skillNeedOffset in stat.skillNeedOffsets)
				{
					if (skillNeedOffset.required && pawn.skills.GetSkill(skillNeedOffset.skill).TotallyDisabled)
					{
						return true;
					}
				}
			}
			if (stat.disableIfSkillDisabled != null && pawn.skills.GetSkill(stat.disableIfSkillDisabled).TotallyDisabled)
			{
				return true;
			}
		}
		return false;
	}

	public virtual string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		return stat.ValueToString(value, numberSense, finalized);
	}

	private static string InfoTextLineFromGear(Thing gear, StatDef stat)
	{
		float f = StatOffsetFromGear(gear, stat);
		return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.finalizeEquippedStatOffset ? stat.toStringStyle : stat.ToStringStyleUnfinalized, ToStringNumberSense.Offset);
	}

	public static float StatOffsetFromGear(Thing gear, StatDef stat)
	{
		float val = gear.def.equippedStatOffsets.GetStatOffsetFromList(stat);
		CompBladelinkWeapon compBladelinkWeapon = gear.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon != null)
		{
			List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
			for (int i = 0; i < traitsListForReading.Count; i++)
			{
				val += traitsListForReading[i].equippedStatOffsets.GetStatOffsetFromList(stat);
			}
		}
		if (Math.Abs(val) > float.Epsilon && !stat.parts.NullOrEmpty())
		{
			foreach (StatPart part in stat.parts)
			{
				part.TransformValue(StatRequest.For(gear), ref val);
			}
		}
		return val;
	}

	private static IEnumerable<Thing> RelevantGear(Pawn pawn, StatDef stat)
	{
		if (pawn.apparel != null)
		{
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (GearAffectsStat(item.def, stat))
				{
					yield return item;
				}
			}
		}
		if (pawn.equipment == null)
		{
			yield break;
		}
		foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading)
		{
			if (GearAffectsStat(item2.def, stat) || GearHasCompsThatAffectStat(item2, stat))
			{
				yield return item2;
			}
		}
	}

	private static bool GearAffectsStat(ThingDef gearDef, StatDef stat)
	{
		if (gearDef.equippedStatOffsets != null)
		{
			for (int i = 0; i < gearDef.equippedStatOffsets.Count; i++)
			{
				if (gearDef.equippedStatOffsets[i].stat == stat && gearDef.equippedStatOffsets[i].value != 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool GearHasCompsThatAffectStat(Thing gear, StatDef stat)
	{
		CompBladelinkWeapon compBladelinkWeapon = gear.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon == null)
		{
			return false;
		}
		List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
		for (int i = 0; i < traitsListForReading.Count; i++)
		{
			if (traitsListForReading[i].equippedStatOffsets.NullOrEmpty())
			{
				continue;
			}
			for (int j = 0; j < traitsListForReading[i].equippedStatOffsets.Count; j++)
			{
				StatModifier statModifier = traitsListForReading[i].equippedStatOffsets[j];
				if (statModifier.stat == stat && statModifier.value != 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual float GetBaseValueFor(StatRequest request)
	{
		float result = stat.defaultBaseValue;
		if (request.StatBases != null)
		{
			for (int i = 0; i < request.StatBases.Count; i++)
			{
				if (request.StatBases[i].stat == stat)
				{
					result = request.StatBases[i].value;
					break;
				}
			}
		}
		return result;
	}

	public virtual string ValueToStringFor(Thing thing)
	{
		return ValueToString(thing.GetStatValue(stat), finalized: true);
	}

	public virtual string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
	{
		if (!finalized)
		{
			string text = val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
			if (numberSense != ToStringNumberSense.Factor && !stat.formatStringUnfinalized.NullOrEmpty())
			{
				text = string.Format(stat.formatStringUnfinalized, text);
			}
			return text;
		}
		string text2 = val.ToStringByStyle(stat.toStringStyle, numberSense);
		if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
		{
			text2 = string.Format(stat.formatString, text2);
		}
		return text2;
	}

	public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
	{
		Thing thing = statRequest.Thing;
		if (!(thing is Pawn pawn))
		{
			yield break;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			HediffStage curStage = hediffs[i].CurStage;
			if (curStage != null)
			{
				float num = curStage.statOffsets.GetStatOffsetFromList(stat);
				if (num != 0f && curStage.statOffsetEffectMultiplier != null)
				{
					num *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
				}
				float num2 = curStage.statFactors.GetStatFactorFromList(stat);
				if (Math.Abs(num2 - 1f) > float.Epsilon && curStage.statFactorEffectMultiplier != null)
				{
					num2 = ScaleFactor(num2, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
				}
				if (Mathf.Abs(num) > 0f || Math.Abs(num2 - 1f) > float.Epsilon)
				{
					yield return new Dialog_InfoCard.Hyperlink(hediffs[i].def);
				}
			}
		}
		foreach (Thing item in RelevantGear(pawn, stat))
		{
			yield return new Dialog_InfoCard.Hyperlink(item);
		}
		if (stat.parts == null)
		{
			yield break;
		}
		foreach (StatPart part in stat.parts)
		{
			foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in part.GetInfoCardHyperlinks(statRequest))
			{
				yield return infoCardHyperlink;
			}
		}
	}

	public static float ScaleFactor(float factor, float scale)
	{
		return 1f - (1f - factor) * scale;
	}

	private static bool DisplayTradeStats(StatRequest req)
	{
		if (!(req.Def is ThingDef thingDef))
		{
			return false;
		}
		if (ModsConfig.BiotechActive && req.HasThing && req.Thing is Pawn { IsColonyMech: not false })
		{
			return true;
		}
		if (req.HasThing && CompBiocodable.IsBiocoded(req.Thing))
		{
			return false;
		}
		if (thingDef.category == ThingCategory.Building && thingDef.Minifiable)
		{
			return true;
		}
		if (TradeUtility.EverPlayerSellable(thingDef))
		{
			return true;
		}
		if (thingDef.tradeability.TraderCanSell() && (thingDef.category == ThingCategory.Item || thingDef.category == ThingCategory.Pawn))
		{
			return true;
		}
		return false;
	}

	public void TryClearCache()
	{
		temporaryStatCache?.Clear();
		immutableStatCache?.Clear();
	}

	public void ClearCacheForThing(Thing thing)
	{
		temporaryStatCache?.Remove(thing);
		immutableStatCache?.Remove(thing, out var _);
	}

	public void DeleteStatCache()
	{
		temporaryStatCache = null;
		immutableStatCache = null;
	}
}
