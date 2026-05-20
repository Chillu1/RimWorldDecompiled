using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class DebugOutputsPawns
{
	[DebugOutput("Pawns", false)]
	public static void PawnKindsBasics()
	{
		DebugTables.MakeTablesDialog(from k in DefDatabase<PawnKindDef>.AllDefs
			where k.race != null && k.RaceProps.Humanlike
			orderby (k.defaultFactionDef == null) ? "" : k.defaultFactionDef.label, k.combatPower
			select k, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("faction", (PawnKindDef d) => (d.defaultFactionDef == null) ? "" : d.defaultFactionDef.defName), new TableDataGetter<PawnKindDef>("points", (PawnKindDef d) => d.combatPower.ToString("F0")), new TableDataGetter<PawnKindDef>("min\nage", (PawnKindDef d) => d.minGenerationAge.ToString("F0")), new TableDataGetter<PawnKindDef>("max\nage", (PawnKindDef d) => (d.maxGenerationAge >= 10000) ? "" : d.maxGenerationAge.ToString("F0")), new TableDataGetter<PawnKindDef>("recruit\nresistance", (PawnKindDef d) => d.initialResistanceRange.ToString()), new TableDataGetter<PawnKindDef>("recruit\nwill", (PawnKindDef d) => d.initialWillRange.ToString()), new TableDataGetter<PawnKindDef>("item\nquality", (PawnKindDef d) => d.itemQuality.ToString()), new TableDataGetter<PawnKindDef>("force norm\ngear qual", (PawnKindDef d) => d.forceNormalGearQuality.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("weapon\n$", (PawnKindDef d) => d.weaponMoney.ToString()), new TableDataGetter<PawnKindDef>("apparel\n$", (PawnKindDef d) => d.apparelMoney.ToString()), new TableDataGetter<PawnKindDef>("tech hediffs\nchance", (PawnKindDef d) => d.techHediffsChance.ToStringPercentEmptyZero()), new TableDataGetter<PawnKindDef>("tech hediffs\n$", (PawnKindDef d) => d.techHediffsMoney.ToString()), new TableDataGetter<PawnKindDef>("gear\nhealth", (PawnKindDef d) => d.gearHealthRange.ToString()), new TableDataGetter<PawnKindDef>("inv\nnutrition", (PawnKindDef d) => d.invNutrition.ToString()), new TableDataGetter<PawnKindDef>("addiction\nchance", (PawnKindDef d) => d.chemicalAddictionChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("combat drug\nchance", (PawnKindDef d) => (!(d.combatEnhancingDrugsChance > 0f)) ? "" : d.combatEnhancingDrugsChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("combat drug\ncount", (PawnKindDef d) => (d.combatEnhancingDrugsCount.max <= 0) ? "" : d.combatEnhancingDrugsCount.ToString()), new TableDataGetter<PawnKindDef>("backstory cryptosleep\ncommonality", (PawnKindDef d) => d.backstoryCryptosleepCommonality.ToStringPercentEmptyZero()));
	}

	[DebugOutput("Pawns", false)]
	public static void PawnKindsWeaponUsage()
	{
		List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
		list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
		list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.weaponMoney.Average.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.weaponMoney.min.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.weaponMoney.max.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
		list.AddRange(from w in DefDatabase<ThingDef>.AllDefs
			where w.IsWeapon && !w.weaponTags.NullOrEmpty()
			orderby w.IsMeleeWeapon descending, w.techLevel, w.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(w.label.Shorten() + "\n$" + w.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.weaponTags != null && w.weaponTags.Any((string z) => k.weaponTags.Contains(z)))
				{
					float num = PawnWeaponGenerator.CheapestNonDerpPriceFor(w);
					if (k.weaponMoney.max < num)
					{
						return "-";
					}
					if (k.weaponMoney.min > num)
					{
						return "✓";
					}
					return (1f - (num - k.weaponMoney.min) / (k.weaponMoney.max - k.weaponMoney.min)).ToStringPercent("F0");
				}
				return "";
			}));
		DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where (int)x.RaceProps.intelligence >= 1
			orderby (x.defaultFactionDef == null) ? int.MaxValue : ((int)x.defaultFactionDef.techLevel), x.combatPower
			select x, list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void PawnKindsApparelUsage()
	{
		List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
		list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
		list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.apparelMoney.Average.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.apparelMoney.min.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.apparelMoney.max.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
		list.AddRange(from a in DefDatabase<ThingDef>.AllDefs
			where a.IsApparel
			orderby PawnApparelGenerator.IsHeadgear(a), a.techLevel, a.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(a.label.Shorten() + "\n$" + a.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.apparelRequired != null && k.apparelRequired.Contains(a))
				{
					return "Rq";
				}
				if (k.apparelDisallowTags != null && k.apparelDisallowTags.Any((string tag) => a.apparel.tags.Contains(tag)))
				{
					return "distag";
				}
				if (k.apparelAllowHeadgearChance <= 0f && PawnApparelGenerator.IsHeadgear(a))
				{
					return "nohat";
				}
				List<SpecificApparelRequirement> specificApparelRequirements = k.specificApparelRequirements;
				if (specificApparelRequirements != null)
				{
					for (int num = 0; num < specificApparelRequirements.Count; num++)
					{
						if (PawnApparelGenerator.ApparelRequirementHandlesThing(specificApparelRequirements[num], a) && PawnApparelGenerator.ApparelRequirementTagsMatch(specificApparelRequirements[num], a))
						{
							return "SpRq";
						}
					}
				}
				if (k.apparelTags != null && a.apparel.tags.Any((string z) => k.apparelTags.Contains(z)))
				{
					float baseMarketValue = a.BaseMarketValue;
					if (k.apparelMoney.max < baseMarketValue)
					{
						return "-";
					}
					if (k.apparelMoney.min > baseMarketValue)
					{
						return "✓";
					}
					return (1f - (baseMarketValue - k.apparelMoney.min) / (k.apparelMoney.max - k.apparelMoney.min)).ToStringPercent("F0");
				}
				return "";
			}));
		DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where x.RaceProps.Humanlike
			orderby (x.defaultFactionDef == null) ? int.MaxValue : ((int)x.defaultFactionDef.techLevel), x.combatPower
			select x, list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void PawnKindsTechHediffUsage()
	{
		List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
		list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
		list.Add(new TableDataGetter<PawnKindDef>("chance", (PawnKindDef x) => x.techHediffsChance.ToStringPercent()));
		list.Add(new TableDataGetter<PawnKindDef>("$\nmin", (PawnKindDef x) => x.techHediffsMoney.min.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("$\nmax", (PawnKindDef x) => x.techHediffsMoney.max.ToString()));
		list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
		list.AddRange(from t in DefDatabase<ThingDef>.AllDefs
			where t.isTechHediff && t.techHediffsTags != null
			orderby t.techLevel descending, t.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(t.label.Shorten().Replace(" ", "\n") + "\n$" + t.BaseMarketValue.ToString("F0"), delegate(PawnKindDef k)
			{
				if (k.techHediffsTags != null && t.techHediffsTags.Any((string tag) => k.techHediffsTags.Contains(tag)))
				{
					if (k.techHediffsMoney.max < t.BaseMarketValue)
					{
						return "-";
					}
					if (k.techHediffsMoney.min >= t.BaseMarketValue)
					{
						return "✓";
					}
					return (1f - (t.BaseMarketValue - k.techHediffsMoney.min) / (k.techHediffsMoney.max - k.techHediffsMoney.min)).ToStringPercent("F0");
				}
				return "";
			}));
		DebugTables.MakeTablesDialog(from x in DefDatabase<PawnKindDef>.AllDefs
			where x.RaceProps.Humanlike
			orderby (x.defaultFactionDef == null) ? int.MaxValue : ((int)x.defaultFactionDef.techLevel), x.combatPower
			select x, list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void PawnKindGearSampled()
	{
		IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.ToolUser
			orderby k.combatPower
			select k;
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (PawnKindDef item2 in orderedEnumerable)
		{
			Faction fac = FactionUtility.DefaultFactionFrom(item2.defaultFactionDef);
			PawnKindDef kind = item2;
			FloatMenuOption item = new FloatMenuOption(kind.defName + " (" + kind.combatPower + ")", delegate
			{
				DefMap<ThingDef, int> weapons = new DefMap<ThingDef, int>();
				DefMap<ThingDef, int> apparel = new DefMap<ThingDef, int>();
				DefMap<HediffDef, int> hediffs = new DefMap<HediffDef, int>();
				for (int i = 0; i < 400; i++)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
					if (pawn.equipment.Primary != null)
					{
						weapons[pawn.equipment.Primary.def]++;
					}
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						hediffs[hediff.def]++;
					}
					foreach (Apparel item3 in pawn.apparel.WornApparel)
					{
						apparel[item3.def]++;
					}
					pawn.Destroy();
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Sampled " + 400 + "x " + kind.defName + ":");
				stringBuilder.AppendLine("Weapons");
				foreach (ThingDef item4 in DefDatabase<ThingDef>.AllDefs.OrderByDescending((ThingDef t) => weapons[t]))
				{
					int num = weapons[item4];
					if (num > 0)
					{
						stringBuilder.AppendLine("  " + item4.defName + "    " + ((float)num / 400f).ToStringPercent());
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Apparel");
				foreach (ThingDef item5 in DefDatabase<ThingDef>.AllDefs.OrderByDescending((ThingDef t) => apparel[t]))
				{
					int num2 = apparel[item5];
					if (num2 > 0)
					{
						stringBuilder.AppendLine("  " + item5.defName + "    " + ((float)num2 / 400f).ToStringPercent());
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Tech hediffs");
				foreach (HediffDef item6 in from h in DefDatabase<HediffDef>.AllDefs
					where h.spawnThingOnRemoved != null
					orderby hediffs[h] descending
					select h)
				{
					int num3 = hediffs[item6];
					if (num3 > 0)
					{
						stringBuilder.AppendLine("  " + item6.defName + "    " + ((float)num3 / 400f).ToStringPercent());
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Addiction hediffs");
				foreach (HediffDef item7 in from h in DefDatabase<HediffDef>.AllDefs
					where h.IsAddiction
					orderby hediffs[h] descending
					select h)
				{
					int num4 = hediffs[item7];
					if (num4 > 0)
					{
						stringBuilder.AppendLine("  " + item7.defName + "    " + ((float)num4 / 400f).ToStringPercent());
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Other hediffs");
				foreach (HediffDef item8 in from h in DefDatabase<HediffDef>.AllDefs
					where h.spawnThingOnRemoved == null && !h.IsAddiction
					orderby hediffs[h] descending
					select h)
				{
					int num5 = hediffs[item8];
					if (num5 > 0)
					{
						stringBuilder.AppendLine("  " + item8.defName + "    " + ((float)num5 / 400f).ToStringPercent());
					}
				}
				Log.Message(stringBuilder.ToString().TrimEndNewlines());
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Pawns", false)]
	public static void PawnWorkDisablesSampled()
	{
		IOrderedEnumerable<PawnKindDef> orderedEnumerable = from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.Humanlike
			orderby k.combatPower
			select k;
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (PawnKindDef item2 in orderedEnumerable)
		{
			PawnKindDef kind = item2;
			Faction fac = FactionUtility.DefaultFactionFrom(kind.defaultFactionDef);
			FloatMenuOption item = new FloatMenuOption(kind.defName + " (" + kind.combatPower + ")", delegate
			{
				Dictionary<WorkTags, int> dictionary = new Dictionary<WorkTags, int>();
				for (int i = 0; i < 1000; i++)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(kind, fac);
					WorkTags combinedDisabledWorkTags = pawn.CombinedDisabledWorkTags;
					foreach (WorkTags value in Enum.GetValues(typeof(WorkTags)))
					{
						if (!dictionary.ContainsKey(value))
						{
							dictionary.Add(value, 0);
						}
						if ((combinedDisabledWorkTags & value) != WorkTags.None)
						{
							dictionary[value]++;
						}
					}
					pawn.Destroy();
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Sampled " + 1000 + "x " + kind.defName + ":");
				stringBuilder.AppendLine("Worktags disabled");
				foreach (WorkTags value2 in Enum.GetValues(typeof(WorkTags)))
				{
					int num = dictionary[value2];
					stringBuilder.AppendLine("  " + value2.ToString() + "    " + num + " (" + ((float)num / 1000f).ToStringPercent() + ")");
				}
				Log.Message(stringBuilder.ToString().TrimEndNewlines());
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Pawns", false)]
	public static void LivePawnsInspirationChances()
	{
		List<TableDataGetter<Pawn>> list = new List<TableDataGetter<Pawn>>();
		list.Add(new TableDataGetter<Pawn>("name", (Pawn p) => p.Label));
		foreach (InspirationDef iDef in DefDatabase<InspirationDef>.AllDefs)
		{
			list.Add(new TableDataGetter<Pawn>(iDef.defName, (Pawn p) => iDef.Worker.InspirationCanOccur(p) ? iDef.Worker.CommonalityFor(p).ToString() : "-no-"));
		}
		DebugTables.MakeTablesDialog(Find.CurrentMap.mapPawns.FreeColonistsSpawned, list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void RacesFoodConsumption()
	{
		Func<ThingDef, int, string> lsName = (ThingDef d, int lsIndex) => (d.race.lifeStageAges.Count <= lsIndex) ? "" : d.race.lifeStageAges[lsIndex].def.defName;
		Func<ThingDef, int, string> maxFood = delegate(ThingDef d, int lsIndex)
		{
			if (d.race.lifeStageAges.Count <= lsIndex)
			{
				return "";
			}
			LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
			return (d.race.baseBodySize * def.bodySizeFactor * def.foodMaxFactor).ToString("F2");
		};
		Func<ThingDef, int, string> hungerRate = delegate(ThingDef d, int lsIndex)
		{
			if (d.race.lifeStageAges.Count <= lsIndex)
			{
				return "";
			}
			LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
			return (d.race.baseHungerRate * def.hungerRateFactor).ToString("F2");
		};
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null && d.race.EatsFood
			orderby d.race.baseHungerRate descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("Lifestage 0", (ThingDef d) => lsName(d, 0)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 0)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 0)), new TableDataGetter<ThingDef>("Lifestage 1", (ThingDef d) => lsName(d, 1)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 1)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 1)), new TableDataGetter<ThingDef>("Lifestage 2", (ThingDef d) => lsName(d, 2)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 2)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 2)), new TableDataGetter<ThingDef>("Lifestage 3", (ThingDef d) => lsName(d, 3)), new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 3)), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 3)));
	}

	[DebugOutput("Pawns", false)]
	public static void RacesButchery()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null
			orderby d.race.baseBodySize
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<ThingDef>("healthScale", (ThingDef d) => d.race.baseHealthScale.ToString("F2")), new TableDataGetter<ThingDef>("hunger rate", (ThingDef d) => d.race.baseHungerRate.ToString("F2")), new TableDataGetter<ThingDef>("wildness", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Wildness).ToStringPercent()), new TableDataGetter<ThingDef>("bodySize", (ThingDef d) => d.race.baseBodySize.ToString("F2")), new TableDataGetter<ThingDef>("meatAmount", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MeatAmount).ToString("F0")), new TableDataGetter<ThingDef>("leatherAmount", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.LeatherAmount).ToString("F0")));
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalsBasics()
	{
		DebugTables.MakeTablesDialog(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef d) => d.race != null && d.RaceProps.Animal), new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("dps", (PawnKindDef d) => MeleeDps(d).ToString("F1")), new TableDataGetter<PawnKindDef>("healthScale", (PawnKindDef d) => d.RaceProps.baseHealthScale.ToString("F2")), new TableDataGetter<PawnKindDef>("points", (PawnKindDef d) => d.combatPower.ToString("F0")), new TableDataGetter<PawnKindDef>("points guess", (PawnKindDef d) => pointsGuess(d).ToString("F0")), new TableDataGetter<PawnKindDef>("speed", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString("F2")), new TableDataGetter<PawnKindDef>("mktval", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<PawnKindDef>("mktval guess", (PawnKindDef d) => mktValGuess(d).ToString("F0")), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef d) => d.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("hunger", (PawnKindDef d) => d.RaceProps.baseHungerRate.ToString("F2")), new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.Wildness).ToStringPercent()), new TableDataGetter<PawnKindDef>("manhunter\ntame chance", (PawnKindDef d) => d.RaceProps.manhunterOnTameFailChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("manhunter\ndamage chance", (PawnKindDef d) => d.RaceProps.manhunterOnDamageChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("lifespan", (PawnKindDef d) => d.RaceProps.lifeExpectancy.ToString("F1")), new TableDataGetter<PawnKindDef>("trainability", (PawnKindDef d) => (d.RaceProps.trainability == null) ? "null" : d.RaceProps.trainability.label), new TableDataGetter<PawnKindDef>("tempMin", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin).ToString("F0")), new TableDataGetter<PawnKindDef>("tempMax", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax).ToString("F0")), new TableDataGetter<PawnKindDef>("flammability", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.Flammability).ToStringPercent()));
		static float mktValGuess(PawnKindDef d)
		{
			float num = 18f;
			num += pointsGuess(d) * 2.7f;
			if (d.RaceProps.trainability == TrainabilityDefOf.None)
			{
				num *= 0.5f;
			}
			else if (d.RaceProps.trainability == TrainabilityDefOf.Intermediate)
			{
				num += 0f;
			}
			else
			{
				if (d.RaceProps.trainability != TrainabilityDefOf.Advanced)
				{
					throw new InvalidOperationException();
				}
				num += 250f;
			}
			num += d.RaceProps.baseBodySize * 80f;
			if (d.race.HasComp(typeof(CompMilkable)))
			{
				num += 125f;
			}
			if (d.race.HasComp(typeof(CompShearable)))
			{
				num += 90f;
			}
			if (d.race.HasComp(typeof(CompEggLayer)))
			{
				num += 90f;
			}
			num *= Mathf.Lerp(0.8f, 1.2f, d.race.GetStatValueAbstract(StatDefOf.Wildness));
			return num * 0.75f;
		}
		static float pointsGuess(PawnKindDef d)
		{
			return (15f + MeleeDps(d) * 10f) * Mathf.Lerp(1f, d.race.GetStatValueAbstract(StatDefOf.MoveSpeed) / 3f, 0.25f) * d.RaceProps.baseHealthScale * GenMath.LerpDouble(0.25f, 1f, 1.65f, 1f, Mathf.Clamp(d.RaceProps.baseBodySize, 0.25f, 1f)) * 0.76f;
		}
	}

	private static float RaceMeleeDpsEstimate(ThingDef race)
	{
		return race.GetStatValueAbstract(StatDefOf.MeleeDPS);
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalPointsToHuntOrSlaughter()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.GetAnimalPointsToHuntOrSlaughter()
			select d, new TableDataGetter<PawnKindDef>("animal", (PawnKindDef a) => a.LabelCap), new TableDataGetter<PawnKindDef>("combat power", (PawnKindDef a) => a.combatPower.ToString()), new TableDataGetter<PawnKindDef>("manhunt on dmg", (PawnKindDef a) => a.RaceProps.manhunterOnDamageChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("manhunt on tame", (PawnKindDef a) => a.RaceProps.manhunterOnTameFailChance.ToStringPercent()), new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef a) => a.race.GetStatValueAbstract(StatDefOf.Wildness).ToString()), new TableDataGetter<PawnKindDef>("mkt val", (PawnKindDef a) => a.race.statBases.Find((StatModifier x) => x.stat == StatDefOf.MarketValue).value.ToString()), new TableDataGetter<PawnKindDef>("points", (PawnKindDef a) => a.GetAnimalPointsToHuntOrSlaughter().ToString()));
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalCombatBalance()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.combatPower
			select d, new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName), new TableDataGetter<PawnKindDef>("meleeDps", (PawnKindDef k) => MeleeDps(k).ToString("F1")), new TableDataGetter<PawnKindDef>("baseHealthScale", (PawnKindDef k) => k.RaceProps.baseHealthScale.ToString()), new TableDataGetter<PawnKindDef>("moveSpeed", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString()), new TableDataGetter<PawnKindDef>("averageArmor", (PawnKindDef k) => AverageArmor(k).ToStringPercent()), new TableDataGetter<PawnKindDef>("combatPowerCalculated", (PawnKindDef k) => CombatPowerCalculated(k).ToString("F0")), new TableDataGetter<PawnKindDef>("combatPower", (PawnKindDef k) => k.combatPower.ToString()), new TableDataGetter<PawnKindDef>("combatPower\ndifference", (PawnKindDef k) => (k.combatPower - CombatPowerCalculated(k)).ToString("F0")));
	}

	private static float CombatPowerCalculated(PawnKindDef pawnKind)
	{
		float num = 1f + MeleeDps(pawnKind) * 2f;
		float num2 = 1f + (pawnKind.RaceProps.baseHealthScale + AverageArmor(pawnKind) * 1.8f) * 2f;
		return num * num2 * 2.5f + 10f + pawnKind.race.GetStatValueAbstract(StatDefOf.MoveSpeed) * 2f;
	}

	[DebugOutput("Pawns", false)]
	public static void Mechanoids()
	{
		Func<PawnKindDef, RecipeDef> recipeFor = (PawnKindDef k) => DefDatabase<RecipeDef>.AllDefs.FirstOrDefault((RecipeDef r) => r.ProducedThingDef == k.race);
		Func<PawnKindDef, string> gestationCycles = delegate(PawnKindDef k)
		{
			RecipeDef recipeDef = recipeFor(k);
			return (recipeDef == null) ? "-" : recipeDef.gestationCycles.ToString();
		};
		Func<PawnKindDef, float> formingTime = delegate(PawnKindDef k)
		{
			RecipeDef recipeDef = recipeFor(k);
			return (recipeDef != null) ? ((float)(recipeDef.formingTicks * recipeDef.gestationCycles) / 60000f) : 0f;
		};
		Func<PawnKindDef, string> ingredients = delegate(PawnKindDef k)
		{
			RecipeDef recipeDef = recipeFor(k);
			return (recipeDef == null) ? "-" : recipeDef.ingredients.Select((IngredientCount ing) => ing.Summary).ToLineList();
		};
		Func<PawnKindDef, string> ingredientMarketValue = delegate(PawnKindDef k)
		{
			RecipeDef recipeDef = recipeFor(k);
			if (recipeDef == null)
			{
				return "-";
			}
			float num = 0f;
			foreach (IngredientCount ingredient in recipeDef.ingredients)
			{
				Thing thing = ThingMaker.MakeThing(ingredient.FixedIngredient);
				thing.stackCount = ingredient.CountRequiredOfFor(ingredient.FixedIngredient, recipeDef);
				num += thing.GetStatValue(StatDefOf.MarketValue);
				thing.Destroy();
			}
			return num.ToStringMoney();
		};
		Func<PawnKindDef, string> producedAt = delegate(PawnKindDef k)
		{
			RecipeDef recipe = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault((RecipeDef r) => r.ProducedThingDef == k.race);
			return (recipe == null) ? "-" : (from r in DefDatabase<ThingDef>.AllDefs
				where !r.recipes.NullOrEmpty() && r.recipes.Contains(recipe)
				select r.label).ToCommaList();
		};
		Func<PawnKindDef, string> researchRequired = delegate(PawnKindDef k)
		{
			RecipeDef recipeDef = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault((RecipeDef r) => r.ProducedThingDef == k.race);
			return (recipeDef?.researchPrerequisite == null) ? "-" : recipeDef?.researchPrerequisite?.label;
		};
		Func<PawnKindDef, ThingDef> primaryWeapon = delegate(PawnKindDef k)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k));
			ThingWithComps primary = pawn.equipment.Primary;
			ThingDef result = null;
			if (primary != null && primary.def.IsWeapon)
			{
				result = primary.def;
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			return result;
		};
		Func<PawnKindDef, string> weaponRange = delegate(PawnKindDef k)
		{
			ThingDef thingDef = primaryWeapon(k);
			return (thingDef == null) ? "-" : thingDef.Verbs.FirstOrDefault((VerbProperties v) => v.isPrimary).range.ToString();
		};
		DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.IsMechanoid
			orderby d.combatPower
			select d, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef k) => k.defName), new TableDataGetter<PawnKindDef>("weightClass", (PawnKindDef k) => (k.RaceProps.mechWeightClass != null) ? k.RaceProps.mechWeightClass.defName : "null"), new TableDataGetter<PawnKindDef>("ingredients", (PawnKindDef k) => ingredients(k)), new TableDataGetter<PawnKindDef>("ingredients\nmarket value", (PawnKindDef k) => ingredientMarketValue(k)), new TableDataGetter<PawnKindDef>("forming\ncycles", (PawnKindDef k) => gestationCycles(k)), new TableDataGetter<PawnKindDef>("total\nforming time\n(game days)", (PawnKindDef k) => formingTime(k)), new TableDataGetter<PawnKindDef>("produced\nat", (PawnKindDef k) => producedAt(k)), new TableDataGetter<PawnKindDef>("research\nrequired", (PawnKindDef k) => researchRequired(k)), new TableDataGetter<PawnKindDef>("bandwidth\ncost", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.BandwidthCost).ToString()), new TableDataGetter<PawnKindDef>("repair\nenergy\ncost", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MechEnergyLossPerHP).ToString()), new TableDataGetter<PawnKindDef>("base\nhealth\nscale", (PawnKindDef k) => k.RaceProps.baseHealthScale.ToString()), new TableDataGetter<PawnKindDef>("move\nspeed", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString()), new TableDataGetter<PawnKindDef>("average\narmor", (PawnKindDef k) => AverageArmor(k).ToStringPercent()), new TableDataGetter<PawnKindDef>("melee\nDPS", (PawnKindDef k) => MeleeDps(k).ToString("F1")), new TableDataGetter<PawnKindDef>("weapon", (PawnKindDef k) => primaryWeapon(k)?.label), new TableDataGetter<PawnKindDef>("weapon\nrange", (PawnKindDef k) => weaponRange(k)), new TableDataGetter<PawnKindDef>("weapon\nDPS", (PawnKindDef k) => weaponDps(primaryWeapon(k))), new TableDataGetter<PawnKindDef>("weapon\naccuracy\nshort", (PawnKindDef k) => weaponHitChance(k, 12f).ToStringPercent()), new TableDataGetter<PawnKindDef>("weapon\naccuracy\nmedium", (PawnKindDef k) => weaponHitChance(k, 25f).ToStringPercent()), new TableDataGetter<PawnKindDef>("weapon\naccuracy\nlong", (PawnKindDef k) => weaponHitChance(k, 40f).ToStringPercent()), new TableDataGetter<PawnKindDef>("shooter\naccuracy\nshort", (PawnKindDef k) => shooterHitChance(k, 12f).ToStringPercent()), new TableDataGetter<PawnKindDef>("shooter\naccuracy\nmedium", (PawnKindDef k) => shooterHitChance(k, 25f).ToStringPercent()), new TableDataGetter<PawnKindDef>("shooter\naccuracy\nlong", (PawnKindDef k) => shooterHitChance(k, 40f).ToStringPercent()), new TableDataGetter<PawnKindDef>("total\naccuracy\nshort", (PawnKindDef k) => combinedHitChance(k, 12f).ToStringPercent()), new TableDataGetter<PawnKindDef>("total\naccuracy\nmedium", (PawnKindDef k) => combinedHitChance(k, 25f).ToStringPercent()), new TableDataGetter<PawnKindDef>("total\naccuracy\nlong", (PawnKindDef k) => combinedHitChance(k, 40f).ToStringPercent()), new TableDataGetter<PawnKindDef>("special\nabilities", (PawnKindDef k) => specialAbilities(k).ToCommaList()), new TableDataGetter<PawnKindDef>("combat\npower", (PawnKindDef k) => k.combatPower.ToString()), new TableDataGetter<PawnKindDef>("body\nsize", (PawnKindDef k) => k.RaceProps.baseBodySize.ToString("F2")));
		static float combinedHitChance(PawnKindDef k, float distance)
		{
			return weaponHitChance(k, distance) * shooterHitChance(k, distance);
		}
		static float shooterHitChance(PawnKindDef k, float distance)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k));
			float result = ShotReport.HitFactorFromShooter(pawn, distance);
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			return result;
		}
		static IEnumerable<string> specialAbilities(PawnKindDef k)
		{
			if (k.race.HasComp(typeof(CompMechCarrier)))
			{
				yield return "produces war urchins";
			}
			if (k.race.HasComp(typeof(CompShield)))
			{
				yield return "has shield";
			}
		}
		static float weaponDps(ThingDef d)
		{
			if (d == null || !d.IsRangedWeapon)
			{
				return 0f;
			}
			int burstShotCount = d.Verbs[0].burstShotCount;
			float warmupTime = d.Verbs[0].warmupTime;
			warmupTime += (float)(burstShotCount - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
			return (float)(((d.Verbs[0].defaultProjectile != null) ? d.Verbs[0].defaultProjectile.projectile.GetDamageAmount(null) : 0) * burstShotCount) / warmupTime;
		}
		static float weaponHitChance(PawnKindDef k, float distance)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(k));
			float result = 0f;
			ThingWithComps primary = pawn.equipment.Primary;
			if (primary != null && primary.def.IsRangedWeapon)
			{
				result = primary.def.Verbs[0].GetHitChanceFactor(primary, distance);
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			return result;
		}
	}

	private static float MeleeDps(PawnKindDef pawnKind)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true));
		while (pawn.health.hediffSet.hediffs.Count > 0)
		{
			pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
		}
		float statValue = pawn.GetStatValue(StatDefOf.MeleeDPS);
		Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		return statValue;
	}

	private static float AverageArmor(PawnKindDef pawnKind)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(pawnKind);
		while (pawn.health.hediffSet.hediffs.Count > 0)
		{
			pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
		}
		float result = (pawn.GetStatValue(StatDefOf.ArmorRating_Blunt) + pawn.GetStatValue(StatDefOf.ArmorRating_Sharp)) / 2f;
		Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		return result;
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalTradeTags()
	{
		List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
		list.Add(new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName));
		foreach (string tag in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.race.tradeTags != null).SelectMany((PawnKindDef k) => k.race.tradeTags).Distinct())
		{
			list.Add(new TableDataGetter<PawnKindDef>(tag, (PawnKindDef k) => (k.race.tradeTags != null && k.race.tradeTags.Contains(tag)).ToStringCheckBlank()));
		}
		DebugTables.MakeTablesDialog(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef d) => d.race != null && d.RaceProps.Animal), list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalBehavior()
	{
		DebugTables.MakeTablesDialog(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef d) => d.race != null && d.RaceProps.Animal), new TableDataGetter<PawnKindDef>("", (PawnKindDef k) => k.defName), new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.Wildness).ToStringPercent()), new TableDataGetter<PawnKindDef>("min\nhandling\nskill", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MinimumHandlingSkill)), new TableDataGetter<PawnKindDef>("trainability", (PawnKindDef k) => k.race.race.trainability.defName), new TableDataGetter<PawnKindDef>("FenceBlocked", (PawnKindDef k) => k.race.race.FenceBlocked.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("manhunterOn\nDamage\nChance", (PawnKindDef k) => k.RaceProps.manhunterOnDamageChance.ToStringPercentEmptyZero("F1")), new TableDataGetter<PawnKindDef>("manhunterOn\nTameFail\nChance", (PawnKindDef k) => k.RaceProps.manhunterOnTameFailChance.ToStringPercentEmptyZero("F1")), new TableDataGetter<PawnKindDef>("predator", (PawnKindDef k) => k.RaceProps.predator.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef k) => k.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("max\nPreyBodySize", (PawnKindDef k) => (!k.RaceProps.predator) ? "" : k.RaceProps.maxPreyBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("canBe\nPredatorPrey", (PawnKindDef k) => k.RaceProps.canBePredatorPrey.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("petness", (PawnKindDef k) => k.RaceProps.petness.ToStringPercent()), new TableDataGetter<PawnKindDef>("nuzzle\nMtbHours", (PawnKindDef k) => (!(k.RaceProps.nuzzleMtbHours > 0f)) ? "" : k.RaceProps.nuzzleMtbHours.ToString()), new TableDataGetter<PawnKindDef>("pack\nAnimal", (PawnKindDef k) => k.RaceProps.packAnimal.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("herd\nAnimal", (PawnKindDef k) => k.RaceProps.herdAnimal.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("wildGroupSize\nMin", (PawnKindDef k) => (k.wildGroupSize.min == 1) ? "" : k.wildGroupSize.min.ToString()), new TableDataGetter<PawnKindDef>("wildGroupSize\nMax", (PawnKindDef k) => (k.wildGroupSize.max == 1) ? "" : k.wildGroupSize.max.ToString()), new TableDataGetter<PawnKindDef>("CanDo\nHerdMigration", (PawnKindDef k) => k.RaceProps.CanDoHerdMigration.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("herd\nMigration\nAllowed", (PawnKindDef k) => k.RaceProps.herdMigrationAllowed.ToStringCheckBlank()), new TableDataGetter<PawnKindDef>("mateMtb", (PawnKindDef k) => k.RaceProps.mateMtbHours.ToStringEmptyZero("F0")));
	}

	[DebugOutput("Pawns", false)]
	public static void AnimalsEcosystem()
	{
		Func<PawnKindDef, float> ecosystemWeightGuess = (PawnKindDef k) => k.RaceProps.baseBodySize * 0.2f + k.RaceProps.baseHungerRate * 0.8f;
		DebugTables.MakeTablesDialog(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.ecoSystemWeight descending
			select d, new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName), new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef d) => d.RaceProps.baseBodySize.ToString("F2")), new TableDataGetter<PawnKindDef>("hunger rate", (PawnKindDef d) => d.RaceProps.baseHungerRate.ToString("F2")), new TableDataGetter<PawnKindDef>("ecosystem weight", (PawnKindDef d) => d.ecoSystemWeight.ToString("F2")), new TableDataGetter<PawnKindDef>("ecosystem weight guess", (PawnKindDef d) => ecosystemWeightGuess(d).ToString("F2")), new TableDataGetter<PawnKindDef>("predator", (PawnKindDef d) => d.RaceProps.predator.ToStringCheckBlank()));
	}

	[DebugOutput("Pawns", false)]
	public static void MentalBreaks()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<MentalBreakDef>.AllDefs
			orderby d.intensity, d.defName
			select d, new TableDataGetter<MentalBreakDef>("defName", (MentalBreakDef d) => d.defName), new TableDataGetter<MentalBreakDef>("intensity", (MentalBreakDef d) => d.intensity.ToString()), new TableDataGetter<MentalBreakDef>("chance in intensity", (MentalBreakDef d) => (d.baseCommonality / DefDatabase<MentalBreakDef>.AllDefs.Where((MentalBreakDef x) => x.intensity == d.intensity).Sum((MentalBreakDef x) => x.baseCommonality)).ToStringPercent()), new TableDataGetter<MentalBreakDef>("min duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.minTicksBeforeRecovery / 60000f).ToString("0.##") : ""), new TableDataGetter<MentalBreakDef>("avg duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? (Mathf.Min((float)d.mentalState.minTicksBeforeRecovery + d.mentalState.recoveryMtbDays * 60000f, d.mentalState.maxTicksBeforeRecovery) / 60000f).ToString("0.##") : ""), new TableDataGetter<MentalBreakDef>("max duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.maxTicksBeforeRecovery / 60000f).ToString("0.##") : ""), new TableDataGetter<MentalBreakDef>("recoverFromSleep", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.recoverFromSleep) ? "" : "recoverFromSleep"), new TableDataGetter<MentalBreakDef>("recoveryThought", (MentalBreakDef d) => (d.mentalState != null) ? d.mentalState.moodRecoveryThought.ToStringSafe() : ""), new TableDataGetter<MentalBreakDef>("category", (MentalBreakDef d) => (d.mentalState == null) ? "" : d.mentalState.category.ToString()), new TableDataGetter<MentalBreakDef>("blockNormalThoughts", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.blockNormalThoughts) ? "" : "blockNormalThoughts"), new TableDataGetter<MentalBreakDef>("blockRandomInteraction", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.blockRandomInteraction) ? "" : "blockRandomInteraction"), new TableDataGetter<MentalBreakDef>("allowBeatfire", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.allowBeatfire) ? "" : "allowBeatfire"));
	}

	private static string ThoughtStagesText(ThoughtDef t)
	{
		string text = "";
		if (t.stages == null)
		{
			return null;
		}
		for (int i = 0; i < t.stages.Count; i++)
		{
			ThoughtStage thoughtStage = t.stages[i];
			text = text + "[" + i + "] ";
			if (thoughtStage == null)
			{
				text += "null";
			}
			else
			{
				if (thoughtStage.label != null)
				{
					text += thoughtStage.label;
				}
				if (thoughtStage.labelSocial != null)
				{
					if (thoughtStage.label != null)
					{
						text += "/";
					}
					text += thoughtStage.labelSocial;
				}
				text += " ";
				if (thoughtStage.baseMoodEffect != 0f)
				{
					text = text + "[" + thoughtStage.baseMoodEffect.ToStringWithSign() + " Mo]";
				}
				if (thoughtStage.baseOpinionOffset != 0f)
				{
					text = text + "(" + thoughtStage.baseOpinionOffset.ToStringWithSign() + " Op)";
				}
			}
			if (i < t.stages.Count - 1)
			{
				text += "\n";
			}
		}
		return text;
	}

	[DebugOutput("Pawns", false)]
	public static void Thoughts()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThoughtDef>.AllDefs, new TableDataGetter<ThoughtDef>("defName", (ThoughtDef d) => d.defName), new TableDataGetter<ThoughtDef>("type", (ThoughtDef d) => (!d.IsMemory) ? "situ" : "mem"), new TableDataGetter<ThoughtDef>("social", (ThoughtDef d) => (!d.IsSocial) ? "mood" : "soc"), new TableDataGetter<ThoughtDef>("stages", (ThoughtDef d) => ThoughtStagesText(d)), new TableDataGetter<ThoughtDef>("best\nmood", (ThoughtDef d) => d.stages.Where((ThoughtStage st) => st != null).Max((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("worst\nmood", (ThoughtDef d) => d.stages.Where((ThoughtStage st) => st != null).Min((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("stack\nlimit", (ThoughtDef d) => d.stackLimit.ToString()), new TableDataGetter<ThoughtDef>("stack\nlimit\nper o. pawn", (ThoughtDef d) => (d.stackLimitForSameOtherPawn >= 0) ? d.stackLimitForSameOtherPawn.ToString() : ""), new TableDataGetter<ThoughtDef>("stacked\neffect\nmultiplier", (ThoughtDef d) => (d.stackLimit != 1) ? d.stackedEffectMultiplier.ToStringPercent() : ""), new TableDataGetter<ThoughtDef>("duration\n(days)", (ThoughtDef d) => d.durationDays.ToString()), new TableDataGetter<ThoughtDef>("effect\nmultiplying\nstat", (ThoughtDef d) => (d.effectMultiplyingStat != null) ? d.effectMultiplyingStat.defName : ""), new TableDataGetter<ThoughtDef>("game\ncondition", (ThoughtDef d) => (d.gameCondition != null) ? d.gameCondition.defName : ""), new TableDataGetter<ThoughtDef>("hediff", (ThoughtDef d) => (d.hediff != null) ? d.hediff.defName : ""), new TableDataGetter<ThoughtDef>("lerp opinion\nto zero\nafter duration pct", (ThoughtDef d) => d.lerpOpinionToZeroAfterDurationPct.ToStringPercent()), new TableDataGetter<ThoughtDef>("max cumulated\nopinion\noffset", (ThoughtDef d) => (!(d.maxCumulatedOpinionOffset > 99999f)) ? d.maxCumulatedOpinionOffset.ToString() : ""), new TableDataGetter<ThoughtDef>("next\nthought", (ThoughtDef d) => (d.nextThought != null) ? d.nextThought.defName : ""), new TableDataGetter<ThoughtDef>("nullified\nif not colonist", (ThoughtDef d) => d.nullifiedIfNotColonist.ToStringCheckBlank()), new TableDataGetter<ThoughtDef>("show\nbubble", (ThoughtDef d) => d.showBubble.ToStringCheckBlank()));
	}

	[DebugOutput("Pawns", false)]
	public static void IdeoThoughts()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThoughtDef>.AllDefs.Where(delegate(ThoughtDef td)
		{
			foreach (PreceptDef item in DefDatabase<PreceptDef>.AllDefsListForReading)
			{
				foreach (PreceptComp comp in item.comps)
				{
					if (comp is PreceptComp_Thought preceptComp_Thought && preceptComp_Thought.thought == td)
					{
						return true;
					}
				}
			}
			return false;
		}), new TableDataGetter<ThoughtDef>("defName", (ThoughtDef d) => d.defName), new TableDataGetter<ThoughtDef>("type", (ThoughtDef d) => (!d.IsMemory) ? "situ" : "mem"), new TableDataGetter<ThoughtDef>("social", (ThoughtDef d) => (!d.IsSocial) ? "mood" : "soc"), new TableDataGetter<ThoughtDef>("stages", (ThoughtDef d) => ThoughtStagesText(d)), new TableDataGetter<ThoughtDef>("best\nmood", (ThoughtDef d) => d.stages.Where((ThoughtStage st) => st != null).Max((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("worst\nmood", (ThoughtDef d) => d.stages.Where((ThoughtStage st) => st != null).Min((ThoughtStage st) => st.baseMoodEffect)), new TableDataGetter<ThoughtDef>("stack\nlimit", (ThoughtDef d) => d.stackLimit.ToString()), new TableDataGetter<ThoughtDef>("stack\nlimit\nper o. pawn", (ThoughtDef d) => (d.stackLimitForSameOtherPawn >= 0) ? d.stackLimitForSameOtherPawn.ToString() : ""), new TableDataGetter<ThoughtDef>("stacked\neffect\nmultiplier", (ThoughtDef d) => (d.stackLimit != 1) ? d.stackedEffectMultiplier.ToStringPercent() : ""), new TableDataGetter<ThoughtDef>("duration\n(days)", (ThoughtDef d) => d.durationDays.ToString()), new TableDataGetter<ThoughtDef>("effect\nmultiplying\nstat", (ThoughtDef d) => (d.effectMultiplyingStat != null) ? d.effectMultiplyingStat.defName : ""), new TableDataGetter<ThoughtDef>("game\ncondition", (ThoughtDef d) => (d.gameCondition != null) ? d.gameCondition.defName : ""), new TableDataGetter<ThoughtDef>("hediff", (ThoughtDef d) => (d.hediff != null) ? d.hediff.defName : ""), new TableDataGetter<ThoughtDef>("lerp opinion\nto zero\nafter duration pct", (ThoughtDef d) => d.lerpOpinionToZeroAfterDurationPct.ToStringPercent()), new TableDataGetter<ThoughtDef>("max cumulated\nopinion\noffset", (ThoughtDef d) => (!(d.maxCumulatedOpinionOffset > 99999f)) ? d.maxCumulatedOpinionOffset.ToString() : ""), new TableDataGetter<ThoughtDef>("next\nthought", (ThoughtDef d) => (d.nextThought != null) ? d.nextThought.defName : ""), new TableDataGetter<ThoughtDef>("nullified\nif not colonist", (ThoughtDef d) => d.nullifiedIfNotColonist.ToStringCheckBlank()), new TableDataGetter<ThoughtDef>("show\nbubble", (ThoughtDef d) => d.showBubble.ToStringCheckBlank()));
	}

	[DebugOutput("Pawns", false)]
	public static void TraitsSampled()
	{
		List<Pawn> testColonists = new List<Pawn>();
		for (int i = 0; i < 4000; i++)
		{
			testColonists.Add(PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer));
		}
		DebugTables.MakeTablesDialog(DefDatabase<TraitDef>.AllDefs.SelectMany((TraitDef tr) => tr.degreeDatas), new TableDataGetter<TraitDegreeData>("trait", (TraitDegreeData d) => getTrait(d).defName), new TableDataGetter<TraitDegreeData>("trait commonality", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.None).ToString("F2")), new TableDataGetter<TraitDegreeData>("trait commonalityFemale", (TraitDegreeData d) => getTrait(d).GetGenderSpecificCommonality(Gender.Female).ToString("F2")), new TableDataGetter<TraitDegreeData>("degree", (TraitDegreeData d) => d.label), new TableDataGetter<TraitDegreeData>("degree num", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? "" : d.degree.ToString()), new TableDataGetter<TraitDegreeData>("degree commonality", (TraitDegreeData d) => (getTrait(d).degreeDatas.Count <= 0) ? "" : d.commonality.ToString("F2")), new TableDataGetter<TraitDegreeData>("marketValueFactorOffset", (TraitDegreeData d) => d.marketValueFactorOffset.ToString("F0")), new TableDataGetter<TraitDegreeData>("prevalence among " + 4000 + "\ngenerated Colonists", (TraitDegreeData d) => getPrevalence(d).ToStringPercent()));
		float getPrevalence(TraitDegreeData d)
		{
			float num = 0f;
			foreach (Pawn item in testColonists)
			{
				Trait trait = item.story.traits.GetTrait(getTrait(d));
				if (trait != null && trait.Degree == d.degree)
				{
					num += 1f;
				}
				Find.WorldPawns?.PassToWorld(item, PawnDiscardDecideMode.Discard);
			}
			return num / 4000f;
		}
		static TraitDef getTrait(TraitDegreeData d)
		{
			return DefDatabase<TraitDef>.AllDefs.First((TraitDef td) => td.degreeDatas.Contains(d));
		}
	}

	[DebugOutput("Pawns", false)]
	public static void BackstoryCountsPerTag()
	{
		IEnumerable<BackstoryDef> allDefs = DefDatabase<BackstoryDef>.AllDefs;
		List<string> dataSources = allDefs.SelectMany((BackstoryDef bs) => bs.spawnCategories).Distinct().ToList();
		Dictionary<string, int> countAdulthoods = new Dictionary<string, int>();
		Dictionary<string, int> countChildhoods = new Dictionary<string, int>();
		foreach (BackstoryDef item in allDefs)
		{
			Dictionary<string, int> dictionary = ((item.slot == BackstorySlot.Adulthood) ? countAdulthoods : countChildhoods);
			foreach (string spawnCategory in item.spawnCategories)
			{
				if (!dictionary.ContainsKey(spawnCategory))
				{
					dictionary.Add(spawnCategory, 0);
				}
				dictionary[spawnCategory]++;
			}
		}
		List<TableDataGetter<string>> list = new List<TableDataGetter<string>>();
		list.Add(new TableDataGetter<string>("tag", (string t) => t));
		list.Add(new TableDataGetter<string>("adulthoods", (string t) => countAdulthoods.ContainsKey(t) ? countAdulthoods[t] : 0));
		list.Add(new TableDataGetter<string>("childhoods", (string t) => countChildhoods.ContainsKey(t) ? countChildhoods[t] : 0));
		DebugTables.MakeTablesDialog(dataSources, list.ToArray());
	}

	[DebugOutput("Pawns", false)]
	public static void ListSolidBackstories()
	{
		IEnumerable<string> enumerable = SolidBioDatabase.allBios.SelectMany((PawnBio bio) => bio.adulthood.spawnCategories).Distinct();
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (string item2 in enumerable)
		{
			string catInner = item2;
			FloatMenuOption item = new FloatMenuOption(catInner, delegate
			{
				IEnumerable<PawnBio> enumerable2 = SolidBioDatabase.allBios.Where((PawnBio b) => b.adulthood.spawnCategories.Contains(catInner));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Backstories with category: " + catInner + " (" + enumerable2.Count() + ")");
				foreach (PawnBio item3 in enumerable2)
				{
					stringBuilder.AppendLine(item3.ToString());
				}
				Log.Message(stringBuilder.ToString());
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Pawns", false)]
	private static void ShowBeardFrequency()
	{
		if (Find.World == null)
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction f in Find.FactionManager.AllFactionsVisible)
		{
			list.Add(new DebugMenuOption(f.Name, DebugMenuOptionMode.Action, delegate
			{
				Faction faction = f;
				int num = 0;
				for (int i = 0; i < 100; i++)
				{
					PawnKindDef spaceRefugee = PawnKindDefOf.SpaceRefugee;
					Gender? fixedGender = Gender.Male;
					Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(spaceRefugee, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, fixedGender));
					if (pawn != null)
					{
						if (pawn.style.beardDef != BeardDefOf.NoBeard)
						{
							num++;
						}
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
				}
				Log.Message("Beard count for " + faction.Name + ": " + num + "/100 generated.");
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Pawns", false)]
	public static void Entities()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where(ValidateEntity), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("label", (ThingDef d) => d.label), new TableDataGetter<ThingDef>("knowledge gain", (ThingDef d) => (!d.HasComp<CompStudiable>()) ? "-" : d.GetCompProperties<CompProperties_Studiable>()?.anomalyKnowledge.ToString()), new TableDataGetter<ThingDef>("knowledge category", (ThingDef d) => (!d.HasComp<CompStudiable>()) ? "-" : d.GetCompProperties<CompProperties_Studiable>()?.knowledgeCategory?.ToString()), new TableDataGetter<ThingDef>("codex entry", (ThingDef d) => d.entityCodexEntry?.label ?? "-"), new TableDataGetter<ThingDef>("combat power", (ThingDef d) => d.race.AnyPawnKind.combatPower), new TableDataGetter<ThingDef>("min. containment", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.MinimumContainmentStrength).ToString("F0")), new TableDataGetter<ThingDef>("bioferrite gen.", (Func<ThingDef, string>)BioferriteGeneration), new TableDataGetter<ThingDef>("body size", (ThingDef d) => d.race.baseBodySize), new TableDataGetter<ThingDef>("health scale", (ThingDef d) => d.race.baseHealthScale), new TableDataGetter<ThingDef>("speed", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.MoveSpeed)), new TableDataGetter<ThingDef>("lifespan", (ThingDef d) => d.race.lifeExpectancy), new TableDataGetter<ThingDef>("tempMin", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin).ToString("F0")), new TableDataGetter<ThingDef>("tempMax", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax).ToString("F0")), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.Flammability).ToStringPercent()), new TableDataGetter<ThingDef>("meleeDps", (ThingDef d) => MeleeDps(d.race.AnyPawnKind).ToString("F1")), new TableDataGetter<ThingDef>("baseHealthScale", (ThingDef d) => d.race.AnyPawnKind.RaceProps.baseHealthScale.ToString()), new TableDataGetter<ThingDef>("moveSpeed", (ThingDef d) => d.race.AnyPawnKind.race.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString()), new TableDataGetter<ThingDef>("averageArmor", (ThingDef d) => AverageArmor(d.race.AnyPawnKind).ToStringPercent()), new TableDataGetter<ThingDef>("combatPowerCalculated", (ThingDef d) => CombatPowerCalculated(d.race.AnyPawnKind).ToString("F0")));
	}

	[DebugOutput("Pawns", false)]
	public static void WorldPawnRelations()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		WorldPawnSituation[] array = (WorldPawnSituation[])Enum.GetValues(typeof(WorldPawnSituation));
		for (int i = 0; i < array.Length; i++)
		{
			WorldPawnSituation worldPawnSituation = array[i];
			if (worldPawnSituation == WorldPawnSituation.None)
			{
				continue;
			}
			foreach (Pawn item in from x in Find.WorldPawns.GetPawnsBySituation(worldPawnSituation)
				orderby x.Faction?.loadID ?? (-1)
				select x)
			{
				if (item.relations == null)
				{
					continue;
				}
				Pawn local = item;
				list.Add(new DebugMenuOption(item.LabelShort + " [" + worldPawnSituation.ToString() + "]", DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Family details for: " + local.Name.ToStringFull);
					stringBuilder.AppendLine("\nDirect relations:");
					foreach (DirectPawnRelation directRelation in local.relations.DirectRelations)
					{
						stringBuilder.AppendLine($" - {directRelation.otherPawn} ({directRelation.def})");
					}
					stringBuilder.AppendLine("\nVirtual relations:");
					foreach (VirtualPawnRelation virtualRelation in local.relations.VirtualRelations)
					{
						stringBuilder.AppendLine($" - {virtualRelation.record.Name} ({virtualRelation.def}, ID: {virtualRelation.record.ID}), {virtualRelation.record.References.Count} references");
						foreach (Pawn reference in virtualRelation.record.References)
						{
							stringBuilder.AppendLine($"      - {reference}");
						}
					}
					stringBuilder.AppendLine("\nChildren:");
					foreach (Pawn child in local.relations.Children)
					{
						stringBuilder.AppendLine(" - " + child.LabelShort);
					}
					Log.Message(stringBuilder);
				}));
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Pawns", false)]
	public static void VirtualRecords()
	{
		StringBuilder stringBuilder = new StringBuilder();
		IReadOnlyDictionary<int, RelationshipRecord> records = Find.RelationshipRecords.Records;
		stringBuilder.AppendLine($"{records.Count} Records:");
		foreach (KeyValuePair<int, RelationshipRecord> item in records)
		{
			stringBuilder.AppendLine($"\n - {item.Value.Name} (ID: {item.Value.ID}), {item.Value.References.Count} references");
			foreach (Pawn reference in item.Value.References)
			{
				stringBuilder.AppendLine($"      - {reference}");
			}
		}
		Log.Message(stringBuilder);
	}

	private static bool ValidateEntity(ThingDef d)
	{
		if (d.IsCorpse)
		{
			return false;
		}
		if (d.race?.AnyPawnKind == null)
		{
			return false;
		}
		if (d.race.AnyPawnKind is CreepJoinerFormKindDef)
		{
			return false;
		}
		string categoryForPawnKind = DebugToolsSpawning.GetCategoryForPawnKind(d.race.AnyPawnKind);
		if (!(categoryForPawnKind == "Other"))
		{
			if (d.HasComp<CompStudiable>() && d.category == ThingCategory.Pawn && categoryForPawnKind != "Animal")
			{
				return categoryForPawnKind != "Insect";
			}
			return false;
		}
		return true;
	}

	private static string BioferriteGeneration(ThingDef def)
	{
		float num = 0f;
		if (def.HasComp<CompProducesBioferrite>())
		{
			num = def.GetCompProperties<CompProperties_ProducesBioferrite>().bioferriteDensity;
		}
		else if (def.race.AnyPawnKind.mutant != null)
		{
			num = 1f;
		}
		return (def.race.baseBodySize * num).ToString("F2");
	}

	[DebugOutput]
	public static void AnimalSpecialTrainables()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (TrainableDef item in DefDatabase<TrainableDef>.AllDefs.Where((TrainableDef t) => t.specialTrainable))
		{
			stringBuilder.AppendLine(item.label);
			foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where(delegate(ThingDef d)
			{
				if (!d.IsCorpse)
				{
					RaceProperties race = d.race;
					if (race != null && race.Animal)
					{
						return race.specialTrainables != null;
					}
					return false;
				}
				return false;
			}))
			{
				if (item2.race.specialTrainables.Contains(item))
				{
					stringBuilder.AppendLine(" - " + item2.label);
				}
			}
		}
		Log.Message(stringBuilder.ToString());
	}
}
