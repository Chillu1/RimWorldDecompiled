using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class DebugOutputsHealth
{
	[DebugOutput]
	public static void Bodies()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (BodyDef allDef in DefDatabase<BodyDef>.AllDefs)
		{
			BodyDef localBd = allDef;
			list.Add(new FloatMenuOption(localBd.defName, delegate
			{
				DebugTables.MakeTablesDialog(localBd.AllParts.OrderByDescending((BodyPartRecord d) => d.height), new TableDataGetter<BodyPartRecord>("defName", (BodyPartRecord d) => d.def.defName), new TableDataGetter<BodyPartRecord>("hitPoints\n(non-adjusted)", (BodyPartRecord d) => d.def.hitPoints), new TableDataGetter<BodyPartRecord>("coverage", (BodyPartRecord d) => d.coverage.ToStringPercent()), new TableDataGetter<BodyPartRecord>("coverageAbsWithChildren", (BodyPartRecord d) => d.coverageAbsWithChildren.ToStringPercent()), new TableDataGetter<BodyPartRecord>("coverageAbs", (BodyPartRecord d) => d.coverageAbs.ToStringPercent()), new TableDataGetter<BodyPartRecord>("depth", (BodyPartRecord d) => d.depth.ToString()), new TableDataGetter<BodyPartRecord>("height", (BodyPartRecord d) => d.height.ToString()));
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput]
	public static void InstallableBodyParts()
	{
		Func<RecipeDef, ThingDef> getThingDef = (RecipeDef r) => r.fixedIngredientFilter.AllowedThingDefs.FirstOrDefault();
		Func<ThingDef, RecipeDef> recipeToMakeThing = (ThingDef t) => DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.ProducedThingDef == t).FirstOrDefault();
		Func<RecipeDef, bool> installsBodyPart = (RecipeDef r) => r.addsHediff != null && getThingDef(r) != null;
		IEnumerable<string> enumerable = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef r) => installsBodyPart(r) && !getThingDef(r).tradeTags.NullOrEmpty()).SelectMany((RecipeDef x) => getThingDef(x).tradeTags).Distinct();
		IEnumerable<string> enumerable2 = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef r) => installsBodyPart(r) && !getThingDef(r).techHediffsTags.NullOrEmpty()).SelectMany((RecipeDef x) => getThingDef(x).techHediffsTags).Distinct();
		Func<ThingDef, string> getMinCraftingSkill = delegate(ThingDef t)
		{
			if (recipeToMakeThing(t) == null || recipeToMakeThing(t).skillRequirements.NullOrEmpty())
			{
				return string.Empty;
			}
			SkillRequirement skillRequirement = recipeToMakeThing(t).skillRequirements.Where((SkillRequirement x) => x.skill == SkillDefOf.Crafting).FirstOrDefault();
			return (skillRequirement != null) ? skillRequirement.minLevel.ToString() : string.Empty;
		};
		List<TableDataGetter<RecipeDef>> list = new List<TableDataGetter<RecipeDef>>
		{
			new TableDataGetter<RecipeDef>("thingDef", (RecipeDef r) => getThingDef(r).defName),
			new TableDataGetter<RecipeDef>("hediffDef", (RecipeDef r) => (r.addsHediff != null) ? r.addsHediff.defName : ""),
			new TableDataGetter<RecipeDef>("mkt val", (RecipeDef r) => getThingDef(r).BaseMarketValue.ToStringMoney()),
			new TableDataGetter<RecipeDef>("tech lvl", (RecipeDef r) => getThingDef(r).techLevel.ToString()),
			new TableDataGetter<RecipeDef>("mass", (RecipeDef r) => getThingDef(r).BaseMass),
			new TableDataGetter<RecipeDef>("work to\nmake", (RecipeDef r) => r.workAmount.ToString()),
			new TableDataGetter<RecipeDef>("min skill\ncrft", (RecipeDef r) => getMinCraftingSkill(getThingDef(r))),
			new TableDataGetter<RecipeDef>("stuff costs", (RecipeDef r) => (!getThingDef(r).CostList.NullOrEmpty()) ? getThingDef(r).CostList.Select((ThingDefCountClass x) => x.Summary).ToCommaList() : ""),
			new TableDataGetter<RecipeDef>("tradeable", (RecipeDef r) => getThingDef(r).tradeability.ToString()),
			new TableDataGetter<RecipeDef>("recipeDef", (RecipeDef r) => r.defName),
			new TableDataGetter<RecipeDef>("death on\nfail %", (RecipeDef r) => r.deathOnFailedSurgeryChance.ToStringPercent()),
			new TableDataGetter<RecipeDef>("surg sccss\nfctr", (RecipeDef r) => r.surgerySuccessChanceFactor.ToString()),
			new TableDataGetter<RecipeDef>("min skill", (RecipeDef r) => r.MinSkillString.TrimEndNewlines().TrimStart(' ')),
			new TableDataGetter<RecipeDef>("research\nprereq", (RecipeDef r) => (recipeToMakeThing(getThingDef(r)) != null) ? ((recipeToMakeThing(getThingDef(r)).researchPrerequisite != null) ? recipeToMakeThing(getThingDef(r)).researchPrerequisite.defName : "") : ""),
			new TableDataGetter<RecipeDef>("research\nprereqs", (RecipeDef r) => (recipeToMakeThing(getThingDef(r)) != null) ? ((!recipeToMakeThing(getThingDef(r)).researchPrerequisites.NullOrEmpty()) ? recipeToMakeThing(getThingDef(r)).researchPrerequisites.Select((ResearchProjectDef x) => x.defName).ToCommaList() : "") : ""),
			new TableDataGetter<RecipeDef>("recipe\nusers", (RecipeDef r) => (recipeToMakeThing(getThingDef(r)) != null) ? recipeToMakeThing(getThingDef(r)).AllRecipeUsers.Select((ThingDef x) => x.defName).ToCommaList() : "")
		};
		foreach (string c in enumerable2)
		{
			TableDataGetter<RecipeDef> item = new TableDataGetter<RecipeDef>("techHediff\n" + c.Shorten(), (RecipeDef r) => (!getThingDef(r).techHediffsTags.NullOrEmpty() && getThingDef(r).techHediffsTags.Contains(c)).ToStringCheckBlank());
			list.Add(item);
		}
		foreach (string c2 in enumerable)
		{
			TableDataGetter<RecipeDef> item2 = new TableDataGetter<RecipeDef>("trade\n" + c2.Shorten(), (RecipeDef r) => (!getThingDef(r).tradeTags.NullOrEmpty() && getThingDef(r).tradeTags.Contains(c2)).ToStringCheckBlank());
			list.Add(item2);
		}
		DebugTables.MakeTablesDialog(DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef r) => getThingDef(r) != null && installsBodyPart(r)), list.ToArray());
	}

	[DebugOutput]
	public static void BodyParts()
	{
		DebugTables.MakeTablesDialog(DefDatabase<BodyPartDef>.AllDefs, new TableDataGetter<BodyPartDef>("defName", (BodyPartDef d) => d.defName), new TableDataGetter<BodyPartDef>("hit\npoints", (BodyPartDef d) => d.hitPoints), new TableDataGetter<BodyPartDef>("bleeding\nate\nmultiplier", (BodyPartDef d) => d.bleedRate.ToStringPercent()), new TableDataGetter<BodyPartDef>("perm injury\nchance factor", (BodyPartDef d) => d.permanentInjuryChanceFactor.ToStringPercent()), new TableDataGetter<BodyPartDef>("frostbite\nvulnerability", (BodyPartDef d) => d.frostbiteVulnerability), new TableDataGetter<BodyPartDef>("solid", (BodyPartDef d) => (!d.IsSolidInDefinition_Debug) ? "" : "S"), new TableDataGetter<BodyPartDef>("beauty\nrelated", (BodyPartDef d) => (!d.beautyRelated) ? "" : "B"), new TableDataGetter<BodyPartDef>("alive", (BodyPartDef d) => (!d.alive) ? "" : "A"), new TableDataGetter<BodyPartDef>("conceptual", (BodyPartDef d) => (!d.conceptual) ? "" : "C"), new TableDataGetter<BodyPartDef>("can\nsuggest\namputation", (BodyPartDef d) => (!d.canSuggestAmputation) ? "no A" : ""), new TableDataGetter<BodyPartDef>("socketed", (BodyPartDef d) => (!d.socketed) ? "" : "DoL"), new TableDataGetter<BodyPartDef>("skin covered", (BodyPartDef d) => (!d.IsSkinCoveredInDefinition_Debug) ? "" : "skin"), new TableDataGetter<BodyPartDef>("pawn generator\ncan amputate", (BodyPartDef d) => (!d.pawnGeneratorCanAmputate) ? "" : "amp"), new TableDataGetter<BodyPartDef>("spawn thing\non removed", (BodyPartDef d) => d.spawnThingOnRemoved), new TableDataGetter<BodyPartDef>("hitChanceFactors", (BodyPartDef d) => (d.hitChanceFactors != null) ? d.hitChanceFactors.Select((KeyValuePair<DamageDef, float> kvp) => kvp.ToString()).ToCommaList() : ""), new TableDataGetter<BodyPartDef>("tags", (BodyPartDef d) => (d.tags != null) ? d.tags.Select((BodyPartTagDef t) => t.defName).ToCommaList() : ""));
	}

	[DebugOutput]
	public static void Surgeries()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<RecipeDef>.AllDefs
			where d.IsSurgery
			orderby d.WorkAmountTotal(null) descending
			select d, new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName), new TableDataGetter<RecipeDef>("work", (RecipeDef d) => d.WorkAmountTotal(null).ToString("F0")), new TableDataGetter<RecipeDef>("ingredients", (RecipeDef d) => d.ingredients.Select((IngredientCount ing) => ing.ToString()).ToCommaList()), new TableDataGetter<RecipeDef>("skillRequirements", (RecipeDef d) => (d.skillRequirements != null) ? d.skillRequirements.Select((SkillRequirement ing) => ing.ToString()).ToCommaList() : "-"), new TableDataGetter<RecipeDef>("surgerySuccessChanceFactor", (RecipeDef d) => d.surgerySuccessChanceFactor.ToStringPercent()), new TableDataGetter<RecipeDef>("deathOnFailChance", (RecipeDef d) => d.deathOnFailedSurgeryChance.ToStringPercent()));
	}

	[DebugOutput]
	public static void HitsToKill()
	{
		var data = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.race != null).Select(delegate(ThingDef x)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 15; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(x.race.AnyPawnKind, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true));
				for (int j = 0; j < 1000; j++)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Crush, 10f);
					dinfo.SetIgnoreInstantKillProtection(ignore: true);
					pawn.TakeDamage(dinfo);
					if (pawn.Destroyed)
					{
						num += j + 1;
						break;
					}
				}
				if (!pawn.Destroyed)
				{
					Log.Error("Could not kill pawn " + pawn.ToStringSafe());
				}
				if (pawn.health.ShouldBeDeadFromLethalDamageThreshold())
				{
					num2++;
				}
				if (Find.WorldPawns.Contains(pawn))
				{
					Find.WorldPawns.RemovePawn(pawn);
				}
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			}
			float hits = (float)num / 15f;
			return new
			{
				Race = x,
				Hits = hits,
				DiedDueToDamageThreshold = num2
			};
		}).ToDictionary(x => x.Race);
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			orderby d.race.baseHealthScale descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("10 damage hits", (ThingDef d) => data[d].Hits.ToString("F0")), new TableDataGetter<ThingDef>("died due to\ndam. thresh.", (ThingDef d) => data[d].DiedDueToDamageThreshold + "/" + 15), new TableDataGetter<ThingDef>("mech", (ThingDef d) => (!d.race.IsMechanoid) ? "" : "mech"));
	}

	[DebugOutput]
	public static void Prosthetics()
	{
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, (Pawn p) => p.health.hediffSet.hediffs.Count == 0);
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		Action refreshPawn = delegate
		{
			while (pawn.health.hediffSet.hediffs.Count > 0)
			{
				pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
			}
		};
		Func<RecipeDef, BodyPartRecord> getApplicationPoint = (RecipeDef recipe) => recipe.appliedOnFixedBodyParts.SelectMany((BodyPartDef bpd) => pawn.def.race.body.GetPartsWithDef(bpd)).Concat(recipe.appliedOnFixedBodyPartGroups.SelectMany((BodyPartGroupDef g) => pawn.def.race.body.AllParts.Where((BodyPartRecord r) => r.groups != null && r.groups.Contains(g)))).FirstOrDefault();
		Func<RecipeDef, ThingDef> getProstheticItem = (RecipeDef recipe) => recipe.ingredients.Select((IngredientCount ic) => ic.filter.AnyAllowedDef).FirstOrDefault((ThingDef td) => !td.IsMedicine);
		List<TableDataGetter<RecipeDef>> list = new List<TableDataGetter<RecipeDef>>();
		list.Add(new TableDataGetter<RecipeDef>("defName", (RecipeDef r) => r.defName));
		list.Add(new TableDataGetter<RecipeDef>("price", (RecipeDef r) => getProstheticItem(r)?.BaseMarketValue ?? 0f));
		list.Add(new TableDataGetter<RecipeDef>("install time", (RecipeDef r) => r.workAmount));
		list.Add(new TableDataGetter<RecipeDef>("install total cost", delegate(RecipeDef r)
		{
			float num = r.ingredients.Sum((IngredientCount ic) => ic.filter.AnyAllowedDef.BaseMarketValue * ic.GetBaseCount());
			float num2 = r.workAmount * 0.0036f;
			return num + num2;
		}));
		list.Add(new TableDataGetter<RecipeDef>("install skill", (RecipeDef r) => r.skillRequirements.Select((SkillRequirement sr) => sr.minLevel).Max()));
		foreach (PawnCapacityDef cap in DefDatabase<PawnCapacityDef>.AllDefs.OrderBy((PawnCapacityDef pc) => pc.listOrder))
		{
			list.Add(new TableDataGetter<RecipeDef>(cap.defName, delegate(RecipeDef r)
			{
				refreshPawn();
				r.Worker.ApplyOnPawn(pawn, getApplicationPoint(r), null, null, null);
				float num = pawn.health.capacities.GetLevel(cap) - 1f;
				if ((double)Math.Abs(num) > 0.001)
				{
					return num.ToStringPercent();
				}
				refreshPawn();
				BodyPartRecord bodyPartRecord = getApplicationPoint(r);
				pawn.TakeDamage(new DamageInfo(DamageDefOf.ExecutionCut, pawn.health.hediffSet.GetPartHealth(bodyPartRecord) / 2f, 999f, -1f, null, bodyPartRecord));
				List<PawnCapacityUtility.CapacityImpactor> list2 = new List<PawnCapacityUtility.CapacityImpactor>();
				PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, cap, list2);
				return list2.Any((PawnCapacityUtility.CapacityImpactor imp) => imp.IsDirect) ? 0f.ToStringPercent() : "";
			}));
		}
		list.Add(new TableDataGetter<RecipeDef>("tech level", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techLevel.ToStringHuman() : ""));
		list.Add(new TableDataGetter<RecipeDef>("thingSetMakerTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).thingSetMakerTags.ToCommaList() : ""));
		list.Add(new TableDataGetter<RecipeDef>("techHediffsTags", (RecipeDef r) => (getProstheticItem(r) != null) ? getProstheticItem(r).techHediffsTags.ToCommaList() : ""));
		DebugTables.MakeTablesDialog(ThingDefOf.Human.AllRecipes.Where((RecipeDef r) => r.workerClass == typeof(Recipe_InstallArtificialBodyPart) || r.workerClass == typeof(Recipe_InstallNaturalBodyPart)), list.ToArray());
		Messages.Clear();
	}

	[DebugOutput]
	public static void TranshumanistBodyParts()
	{
		DebugTables.MakeTablesDialog(DefDatabase<HediffDef>.AllDefs, new List<TableDataGetter<HediffDef>>
		{
			new TableDataGetter<HediffDef>("defName", (HediffDef h) => h.defName),
			new TableDataGetter<HediffDef>("cares", (HediffDef h) => h.countsAsAddedPartOrImplant.ToStringCheckBlank())
		}.ToArray());
	}
}
