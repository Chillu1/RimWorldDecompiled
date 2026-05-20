using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class MedicalRecipesUtility
{
	public static bool IsCleanAndDroppable(Pawn pawn, BodyPartRecord part)
	{
		if (pawn.Dead)
		{
			return false;
		}
		if (pawn.RaceProps.Animal)
		{
			return false;
		}
		if (pawn.IsMutant && !pawn.mutant.Def.partsCleanAndDroppable)
		{
			return false;
		}
		if (part.def.spawnThingOnRemoved != null)
		{
			return IsClean(pawn, part);
		}
		return false;
	}

	public static bool IsClean(Pawn pawn, BodyPartRecord part)
	{
		if (pawn.Dead)
		{
			return false;
		}
		return !pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part).Any();
	}

	public static void RestorePartAndSpawnAllPreviousParts(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
	{
		SpawnNaturalPartIfClean(pawn, part, pos, map);
		SpawnThingsFromHediffs(pawn, part, pos, map);
		pawn.health.RestorePart(part);
	}

	public static Thing SpawnNaturalPartIfClean(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
	{
		if (IsCleanAndDroppable(pawn, part))
		{
			return GenSpawn.Spawn(part.def.spawnThingOnRemoved, pos, map);
		}
		return null;
	}

	public static void SpawnThingsFromHediffs(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
	{
		if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
		{
			return;
		}
		foreach (Hediff item in pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part))
		{
			if (item.def.spawnThingOnRemoved != null)
			{
				GenSpawn.Spawn(item.def.spawnThingOnRemoved, pos, map);
			}
		}
		for (int num = 0; num < part.parts.Count; num++)
		{
			SpawnThingsFromHediffs(pawn, part.parts[num], pos, map);
		}
	}

	public static IEnumerable<BodyPartRecord> GetFixedPartsToApplyOn(RecipeDef recipe, Pawn pawn, Func<BodyPartRecord, bool> validator = null)
	{
		int i = 0;
		while (i < recipe.appliedOnFixedBodyParts.Count)
		{
			BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
			List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
			for (int j = 0; j < bpList.Count; j++)
			{
				BodyPartRecord bodyPartRecord = bpList[j];
				if (bodyPartRecord.def == part && (validator == null || validator(bodyPartRecord)))
				{
					yield return bodyPartRecord;
				}
			}
			int num = i + 1;
			i = num;
		}
		i = 0;
		while (i < recipe.appliedOnFixedBodyPartGroups.Count)
		{
			BodyPartGroupDef group = recipe.appliedOnFixedBodyPartGroups[i];
			List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
			for (int j = 0; j < bpList.Count; j++)
			{
				BodyPartRecord bodyPartRecord2 = bpList[j];
				if (bodyPartRecord2.groups != null && bodyPartRecord2.groups.Contains(group) && (validator == null || validator(bodyPartRecord2)))
				{
					yield return bodyPartRecord2;
				}
			}
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<StatDrawEntry> GetMedicalStatsFromRecipeDefs(IEnumerable<RecipeDef> recipes)
	{
		bool multiple = recipes.Count() >= 2;
		foreach (RecipeDef def in recipes)
		{
			string extraLabelPart = (multiple ? (" (" + def.addsHediff.label + ")") : "");
			HediffDef diff = def.addsHediff;
			if (diff == null)
			{
				continue;
			}
			if (!def.appliedOnFixedBodyParts.NullOrEmpty())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_InstallSites".Translate(), def.appliedOnFixedBodyParts.Select((BodyPartDef x) => x.label).ToCommaList().CapitalizeFirst(), "Stat_Thing_InstallSites_Desc".Translate(), 3990);
				if (diff.addedPartProps != null && diff.addedPartProps.solid)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_ReplacesParts".Translate(), (from x in PartAndAllPartsUnder(def.appliedOnFixedBodyParts)
						select x.def.label).Distinct().ToCommaList().CapitalizeFirst(), "Stat_Thing_ReplacesParts_Desc".Translate(), 3980);
				}
			}
			else if (!def.appliedOnFixedBodyPartGroups.NullOrEmpty())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_InstallSites".Translate(), def.appliedOnFixedBodyPartGroups.Select((BodyPartGroupDef x) => x.label).ToCommaList().CapitalizeFirst(), "Stat_Thing_InstallSites_Desc".Translate(), 3990);
			}
			if (diff.addedPartProps != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BodyPartEfficiency".Translate() + extraLabelPart, diff.addedPartProps.partEfficiency.ToStringByStyle(ToStringStyle.PercentZero), "Stat_Thing_BodyPartEfficiency_Desc".Translate(), 4000);
			}
			foreach (StatDrawEntry item in diff.SpecialDisplayStats(StatRequest.ForEmpty()))
			{
				item.category = StatCategoryDefOf.Implant;
				yield return item;
			}
			HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = diff.CompProps<HediffCompProperties_VerbGiver>();
			if (hediffCompProperties_VerbGiver != null)
			{
				if (!hediffCompProperties_VerbGiver.verbs.NullOrEmpty())
				{
					VerbProperties verb = hediffCompProperties_VerbGiver.verbs[0];
					if (!verb.IsMeleeAttack)
					{
						if (verb.defaultProjectile != null)
						{
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.AppendLine("Stat_Thing_Damage_Desc".Translate());
							stringBuilder.AppendLine();
							int damageAmount = verb.defaultProjectile.projectile.GetDamageAmount(null, stringBuilder);
							yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Damage".Translate() + extraLabelPart, damageAmount.ToString(), stringBuilder.ToString(), 5500);
							if (verb.defaultProjectile.projectile.damageDef.armorCategory != null)
							{
								float armorPenetration = verb.defaultProjectile.projectile.GetArmorPenetration();
								yield return new StatDrawEntry(StatCategoryDefOf.Basics, "ArmorPenetration".Translate() + extraLabelPart, armorPenetration.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
							}
						}
					}
					else
					{
						int meleeDamageBaseAmount = verb.meleeDamageBaseAmount;
						if (verb.meleeDamageDef.armorCategory != null)
						{
							float num = verb.meleeArmorPenetrationBase;
							if (num < 0f)
							{
								num = (float)meleeDamageBaseAmount * 0.015f;
							}
							yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Melee, "ArmorPenetration".Translate() + extraLabelPart, num.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
						}
					}
				}
				else if (!hediffCompProperties_VerbGiver.tools.NullOrEmpty())
				{
					Tool tool = hediffCompProperties_VerbGiver.tools[0];
					if (ThingUtility.PrimaryMeleeWeaponDamageType(hediffCompProperties_VerbGiver.tools).armorCategory != null)
					{
						float num2 = tool.armorPenetration;
						if (num2 < 0f)
						{
							num2 = tool.power * 0.015f;
						}
						yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Melee, "ArmorPenetration".Translate() + extraLabelPart, num2.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
					}
				}
			}
			ThoughtDef thoughtDef = DefDatabase<ThoughtDef>.AllDefs.FirstOrDefault((ThoughtDef x) => x.hediff == diff);
			if (thoughtDef != null && thoughtDef.stages != null && thoughtDef.stages.Any())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MoodChange".Translate() + extraLabelPart, thoughtDef.stages.First().baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset), "Stat_Thing_MoodChange_Desc".Translate(), 3500);
			}
		}
		static IEnumerable<BodyPartRecord> PartAndAllPartsUnder(List<BodyPartDef> bpDefs)
		{
			foreach (BodyPartDef bpDef in bpDefs)
			{
				BodyPartRecord bodyPartRecord = BodyDefOf.Human.GetPartsWithDef(bpDef).FirstOrDefault();
				if (bodyPartRecord == null)
				{
					yield break;
				}
				foreach (BodyPartRecord partAndAllChildPart in bodyPartRecord.GetPartAndAllChildParts())
				{
					yield return partAndAllChildPart;
				}
			}
		}
	}
}
