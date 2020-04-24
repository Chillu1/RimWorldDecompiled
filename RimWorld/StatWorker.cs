using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker
	{
		protected StatDef stat;

		public void InitSetStat(StatDef newStat)
		{
			stat = newStat;
		}

		public float GetValue(Thing thing, bool applyPostProcess = true)
		{
			return GetValue(StatRequest.For(thing));
		}

		public float GetValue(StatRequest req, bool applyPostProcess = true)
		{
			if (stat.minifiedThingInherits)
			{
				MinifiedThing minifiedThing = req.Thing as MinifiedThing;
				if (minifiedThing != null)
				{
					if (minifiedThing.InnerThing == null)
					{
						Log.Error("MinifiedThing's inner thing is null.");
					}
					return minifiedThing.InnerThing.GetStatValue(stat, applyPostProcess);
				}
			}
			float val = GetValueUnfinalized(req, applyPostProcess);
			FinalizeValue(req, ref val, applyPostProcess);
			return val;
		}

		public float GetValueAbstract(BuildableDef def, ThingDef stuffDef = null)
		{
			return GetValue(StatRequest.For(def, stuffDef));
		}

		public float GetValueAbstract(AbilityDef def)
		{
			return GetValue(StatRequest.For(def));
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
						num += pawn.story.traits.allTraits[k].OffsetOfStat(stat);
					}
				}
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int l = 0; l < hediffs.Count; l++)
				{
					HediffStage curStage = hediffs[l].CurStage;
					if (curStage != null)
					{
						float num2 = curStage.statOffsets.GetStatOffsetFromList(stat);
						if (num2 != 0f && curStage.statOffsetEffectMultiplier != null)
						{
							num2 *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
						}
						num += num2;
					}
				}
				if (pawn.apparel != null)
				{
					for (int m = 0; m < pawn.apparel.WornApparel.Count; m++)
					{
						num += StatOffsetFromGear(pawn.apparel.WornApparel[m], stat);
					}
				}
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					num += StatOffsetFromGear(pawn.equipment.Primary, stat);
				}
				if (pawn.story != null)
				{
					for (int n = 0; n < pawn.story.traits.allTraits.Count; n++)
					{
						num *= pawn.story.traits.allTraits[n].MultiplierOfStat(stat);
					}
				}
				for (int num3 = 0; num3 < hediffs.Count; num3++)
				{
					HediffStage curStage2 = hediffs[num3].CurStage;
					if (curStage2 != null)
					{
						float num4 = curStage2.statFactors.GetStatFactorFromList(stat);
						if (Math.Abs(num4 - 1f) > float.Epsilon && curStage2.statFactorEffectMultiplier != null)
						{
							num4 = ScaleFactor(num4, pawn.GetStatValue(curStage2.statFactorEffectMultiplier));
						}
						num *= num4;
					}
				}
				num *= pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
			}
			if (req.StuffDef != null)
			{
				if (num > 0f || stat.applyFactorsIfNegative)
				{
					num *= req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
				}
				num += req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
			}
			if (req.ForAbility && stat.statFactors != null)
			{
				for (int num5 = 0; num5 < stat.statFactors.Count; num5++)
				{
					num *= req.AbilityDef.statBases.GetStatValueFromList(stat.statFactors[num5], 1f);
				}
			}
			if (req.HasThing)
			{
				CompAffectedByFacilities compAffectedByFacilities = req.Thing.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null)
				{
					num += compAffectedByFacilities.GetStatOffset(stat);
				}
				if (stat.statFactors != null)
				{
					for (int num6 = 0; num6 < stat.statFactors.Count; num6++)
					{
						num *= req.Thing.GetStatValue(stat.statFactors[num6]);
					}
				}
				if (pawn != null)
				{
					if (pawn.skills != null)
					{
						if (stat.skillNeedFactors != null)
						{
							for (int num7 = 0; num7 < stat.skillNeedFactors.Count; num7++)
							{
								num *= stat.skillNeedFactors[num7].ValueFor(pawn);
							}
						}
					}
					else
					{
						num *= stat.noSkillFactor;
					}
					if (stat.capacityFactors != null)
					{
						for (int num8 = 0; num8 < stat.capacityFactors.Count; num8++)
						{
							PawnCapacityFactor pawnCapacityFactor = stat.capacityFactors[num8];
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
			if (baseValueFor != 0f)
			{
				stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(baseValueFor, numberSense));
			}
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedOffsets != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
						{
							SkillNeed skillNeed = stat.skillNeedOffsets[i];
							int level = pawn.skills.GetSkill(skillNeed.skill).Level;
							float val = skillNeed.ValueFor(pawn);
							stringBuilder.AppendLine((string)("    " + skillNeed.skill.LabelCap + " (") + level + "): " + val.ToStringSign() + ValueToString(val, finalized: false));
						}
					}
				}
				else if (stat.noSkillOffset != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : " + stat.noSkillOffset.ToStringSign() + ValueToString(stat.noSkillOffset, finalized: false));
				}
				if (stat.capacityOffsets != null)
				{
					stringBuilder.AppendLine("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate());
					foreach (PawnCapacityOffset item in stat.capacityOffsets.OrderBy((PawnCapacityOffset hfa) => hfa.capacity.listOrder))
					{
						string text = item.capacity.GetLabelFor(pawn).CapitalizeFirst();
						float level2 = pawn.health.capacities.GetLevel(item.capacity);
						float offset = item.GetOffset(pawn.health.capacities.GetLevel(item.capacity));
						string text2 = ValueToString(offset, finalized: false);
						string text3 = Mathf.Min(level2, item.max).ToStringPercent() + ", " + "HealthOffsetScale".Translate(item.scale.ToString() + "x");
						if (item.max < 999f)
						{
							text3 += ", " + "HealthFactorMaxImpact".Translate(item.max.ToStringPercent());
						}
						stringBuilder.AppendLine("    " + text + ": " + offset.ToStringSign() + text2 + " (" + text3 + ")");
					}
				}
				if ((int)pawn.RaceProps.intelligence >= 1)
				{
					if (pawn.story != null && pawn.story.traits != null)
					{
						List<Trait> list = pawn.story.traits.allTraits.Where((Trait tr) => tr.CurrentData.statOffsets != null && tr.CurrentData.statOffsets.Any((StatModifier se) => se.stat == stat)).ToList();
						List<Trait> list2 = pawn.story.traits.allTraits.Where((Trait tr) => tr.CurrentData.statFactors != null && tr.CurrentData.statFactors.Any((StatModifier se) => se.stat == stat)).ToList();
						if (list.Count > 0 || list2.Count > 0)
						{
							stringBuilder.AppendLine("StatsReport_RelevantTraits".Translate());
							for (int j = 0; j < list.Count; j++)
							{
								Trait trait = list[j];
								string valueToStringAsOffset = trait.CurrentData.statOffsets.First((StatModifier se) => se.stat == stat).ValueToStringAsOffset;
								stringBuilder.AppendLine("    " + trait.LabelCap + ": " + valueToStringAsOffset);
							}
							for (int k = 0; k < list2.Count; k++)
							{
								Trait trait2 = list2[k];
								string toStringAsFactor = trait2.CurrentData.statFactors.First((StatModifier se) => se.stat == stat).ToStringAsFactor;
								stringBuilder.AppendLine("    " + trait2.LabelCap + ": " + toStringAsFactor);
							}
						}
					}
					if (RelevantGear(pawn, stat).Any())
					{
						stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
						if (pawn.apparel != null)
						{
							for (int l = 0; l < pawn.apparel.WornApparel.Count; l++)
							{
								Apparel apparel = pawn.apparel.WornApparel[l];
								if (GearAffectsStat(apparel.def, stat))
								{
									stringBuilder.AppendLine(InfoTextLineFromGear(apparel, stat));
								}
							}
						}
						if (pawn.equipment != null && pawn.equipment.Primary != null && GearAffectsStat(pawn.equipment.Primary.def, stat))
						{
							stringBuilder.AppendLine(InfoTextLineFromGear(pawn.equipment.Primary, stat));
						}
					}
				}
				bool flag = false;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int m = 0; m < hediffs.Count; m++)
				{
					HediffStage curStage = hediffs[m].CurStage;
					if (curStage == null)
					{
						continue;
					}
					float num = curStage.statOffsets.GetStatOffsetFromList(stat);
					if (num != 0f)
					{
						float val2 = num;
						if (curStage.statOffsetEffectMultiplier != null)
						{
							num *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
						}
						if (!flag)
						{
							stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate());
							flag = true;
						}
						stringBuilder.Append("    " + hediffs[m].LabelBaseCap + ": " + ValueToString(num, finalized: false, ToStringNumberSense.Offset));
						if (curStage.statOffsetEffectMultiplier != null)
						{
							stringBuilder.Append(" (" + ValueToString(val2, finalized: false, ToStringNumberSense.Offset) + " x " + ValueToString(pawn.GetStatValue(curStage.statOffsetEffectMultiplier), finalized: true, curStage.statOffsetEffectMultiplier.toStringNumberSense) + " " + curStage.statOffsetEffectMultiplier.LabelCap + ")");
						}
						stringBuilder.AppendLine();
					}
					float num2 = curStage.statFactors.GetStatFactorFromList(stat);
					if (Math.Abs(num2 - 1f) > float.Epsilon)
					{
						float val3 = num2;
						if (curStage.statFactorEffectMultiplier != null)
						{
							num2 = ScaleFactor(num2, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
						}
						if (!flag)
						{
							stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate());
							flag = true;
						}
						stringBuilder.Append("    " + hediffs[m].LabelBaseCap + ": " + ValueToString(num2, finalized: false, ToStringNumberSense.Factor));
						if (curStage.statFactorEffectMultiplier != null)
						{
							stringBuilder.Append(" (" + ValueToString(val3, finalized: false, ToStringNumberSense.Factor) + " x " + ValueToString(pawn.GetStatValue(curStage.statFactorEffectMultiplier), finalized: false) + " " + curStage.statFactorEffectMultiplier.LabelCap + ")");
						}
						stringBuilder.AppendLine();
					}
				}
				float statFactorFromList = pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
				if (statFactorFromList != 1f)
				{
					stringBuilder.AppendLine("StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statFactorFromList.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
				}
			}
			if (req.StuffDef != null)
			{
				if (baseValueFor > 0f || stat.applyFactorsIfNegative)
				{
					float statFactorFromList2 = req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList2 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statFactorFromList2.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
					}
				}
				float statOffsetFromList = req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
				if (statOffsetFromList != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statOffsetFromList.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
				}
			}
			req.Thing.TryGetComp<CompAffectedByFacilities>()?.GetStatsExplanation(stat, stringBuilder);
			if (stat.statFactors != null)
			{
				stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
				for (int n = 0; n < stat.statFactors.Count; n++)
				{
					StatDef statDef = stat.statFactors[n];
					stringBuilder.AppendLine("    " + statDef.LabelCap + ": x" + statDef.Worker.GetValue(req).ToStringPercent());
				}
			}
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedFactors != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int num3 = 0; num3 < stat.skillNeedFactors.Count; num3++)
						{
							SkillNeed skillNeed2 = stat.skillNeedFactors[num3];
							int level3 = pawn.skills.GetSkill(skillNeed2.skill).Level;
							stringBuilder.AppendLine((string)("    " + skillNeed2.skill.LabelCap + " (") + level3 + "): x" + skillNeed2.ValueFor(pawn).ToStringPercent());
						}
					}
				}
				else if (stat.noSkillFactor != 1f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : x" + stat.noSkillFactor.ToStringPercent());
				}
				if (stat.capacityFactors != null)
				{
					stringBuilder.AppendLine("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate());
					if (stat.capacityFactors != null)
					{
						foreach (PawnCapacityFactor item2 in stat.capacityFactors.OrderBy((PawnCapacityFactor hfa) => hfa.capacity.listOrder))
						{
							string text4 = item2.capacity.GetLabelFor(pawn).CapitalizeFirst();
							string text5 = item2.GetFactor(pawn.health.capacities.GetLevel(item2.capacity)).ToStringPercent();
							string text6 = "HealthFactorPercentImpact".Translate(item2.weight.ToStringPercent());
							if (item2.max < 999f)
							{
								text6 += ", " + "HealthFactorMaxImpact".Translate(item2.max.ToStringPercent());
							}
							if (item2.allowedDefect != 0f)
							{
								text6 += ", " + "HealthFactorAllowedDefect".Translate((1f - item2.allowedDefect).ToStringPercent());
							}
							stringBuilder.AppendLine("    " + text4 + ": x" + text5 + " (" + text6 + ")");
						}
					}
				}
				if (pawn.Inspired)
				{
					float statOffsetFromList2 = pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
					if (statOffsetFromList2 != 0f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + ValueToString(statOffsetFromList2, finalized: false, ToStringNumberSense.Offset));
					}
					float statFactorFromList3 = pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList3 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + statFactorFromList3.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
					}
				}
			}
			return stringBuilder.ToString();
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
			if (applyPostProcess && stat.postProcessStatFactors != null)
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
			if (stat.parts != null)
			{
				for (int i = 0; i < stat.parts.Count; i++)
				{
					string text = stat.parts[i].ExplanationPart(req);
					if (!text.NullOrEmpty())
					{
						stringBuilder.AppendLine(text);
					}
				}
			}
			if (stat.postProcessCurve != null)
			{
				float value = GetValue(req, applyPostProcess: false);
				float num = stat.postProcessCurve.Evaluate(value);
				if (!Mathf.Approximately(value, num))
				{
					string t = ValueToString(value, finalized: false);
					string t2 = stat.ValueToString(num, numberSense);
					stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + t + " => " + t2);
				}
			}
			if (stat.postProcessStatFactors != null)
			{
				stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
				for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
				{
					StatDef statDef = stat.postProcessStatFactors[j];
					stringBuilder.AppendLine($"    {statDef.LabelCap}: x{statDef.Worker.GetValue(req).ToStringPercent()}");
				}
			}
			float statFactor = Find.Scenario.GetStatFactor(stat);
			if (statFactor != 1f)
			{
				stringBuilder.AppendLine("StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
			}
			stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(finalVal, stat.toStringNumberSense));
			return stringBuilder.ToString();
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
			if (!stat.showIfModsLoaded.NullOrEmpty())
			{
				for (int i = 0; i < stat.showIfModsLoaded.Count; i++)
				{
					if (!ModsConfig.IsActive(stat.showIfModsLoaded[i]))
					{
						return false;
					}
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
					if (!stat.showOnNonWildManHumanlikes && thingDef.race.Humanlike && !((req.Thing as Pawn)?.IsWildMan() ?? false))
					{
						return false;
					}
					if (!stat.showOnAnimals && thingDef.race.Animal)
					{
						return false;
					}
					if (!stat.showOnMechanoids && thingDef.race.IsMechanoid)
					{
						return false;
					}
				}
				if (!stat.showOnUnhaulables && !thingDef.EverHaulable && !thingDef.Minifiable)
				{
					return false;
				}
			}
			if (stat.category == StatCategoryDefOf.BasicsPawn || stat.category == StatCategoryDefOf.BasicsPawnImportant || stat.category == StatCategoryDefOf.PawnCombat)
			{
				if (thingDef != null)
				{
					return thingDef.category == ThingCategory.Pawn;
				}
				return false;
			}
			if (stat.category == StatCategoryDefOf.PawnMisc || stat.category == StatCategoryDefOf.PawnSocial || stat.category == StatCategoryDefOf.PawnWork)
			{
				if (thingDef != null)
				{
					if (thingDef.category == ThingCategory.Pawn)
					{
						return thingDef.race.Humanlike;
					}
					return false;
				}
				return false;
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
			if (stat.category == StatCategoryDefOf.BasicsNonPawn || stat.category == StatCategoryDefOf.BasicsNonPawnImportant)
			{
				if (thingDef == null || thingDef.category != ThingCategory.Pawn)
				{
					return !req.ForAbility;
				}
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
			Log.Error("Unhandled case: " + stat + ", " + def);
			return false;
		}

		public virtual bool IsDisabledFor(Thing thing)
		{
			if (stat.neverDisabled || (stat.skillNeedFactors.NullOrEmpty() && stat.skillNeedOffsets.NullOrEmpty()))
			{
				return false;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.story != null)
			{
				if (stat.skillNeedFactors != null)
				{
					for (int i = 0; i < stat.skillNeedFactors.Count; i++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedFactors[i].skill).TotallyDisabled)
						{
							return true;
						}
					}
				}
				if (stat.skillNeedOffsets != null)
				{
					for (int j = 0; j < stat.skillNeedOffsets.Count; j++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedOffsets[j].skill).TotallyDisabled)
						{
							return true;
						}
					}
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
			return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
		}

		private static float StatOffsetFromGear(Thing gear, StatDef stat)
		{
			return gear.def.equippedStatOffsets.GetStatOffsetFromList(stat);
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
			if (pawn.equipment != null)
			{
				foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading)
				{
					if (GearAffectsStat(item2.def, stat))
					{
						yield return item2;
					}
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

		protected float GetBaseValueFor(StatRequest request)
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

		public string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			if (!finalized)
			{
				return val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
			}
			string text = val.ToStringByStyle(stat.toStringStyle, numberSense);
			if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
			{
				text = string.Format(stat.formatString, text);
			}
			return text;
		}

		public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
		{
			Pawn pawn = statRequest.Thing as Pawn;
			if (pawn == null)
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
			if (stat.parts != null)
			{
				foreach (StatPart part in stat.parts)
				{
					foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in part.GetInfoCardHyperlinks(statRequest))
					{
						yield return infoCardHyperlink;
					}
				}
			}
		}

		public static float ScaleFactor(float factor, float scale)
		{
			return 1f - (1f - factor) * scale;
		}

		private static bool DisplayTradeStats(StatRequest req)
		{
			ThingDef thingDef;
			if ((thingDef = (req.Def as ThingDef)) == null)
			{
				return false;
			}
			if (req.HasThing && EquipmentUtility.IsBiocoded(req.Thing))
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
	}
}
