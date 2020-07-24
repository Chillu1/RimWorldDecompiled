using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class DebugOutputsGeneral
	{
		private static float damage(ThingDef d)
		{
			return (d.Verbs[0].defaultProjectile != null) ? d.Verbs[0].defaultProjectile.projectile.GetDamageAmount(null) : 0;
		}

		private static float armorPenetration(ThingDef d)
		{
			if (d.Verbs[0].defaultProjectile == null)
			{
				return 0f;
			}
			return d.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(null);
		}

		private static float stoppingPower(ThingDef d)
		{
			if (d.Verbs[0].defaultProjectile == null)
			{
				return 0f;
			}
			return d.Verbs[0].defaultProjectile.projectile.stoppingPower;
		}

		private static float warmup(ThingDef d)
		{
			return d.Verbs[0].warmupTime;
		}

		private static float cooldown(ThingDef d)
		{
			return d.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown);
		}

		private static int burstShots(ThingDef d)
		{
			return d.Verbs[0].burstShotCount;
		}

		private static float fullcycle(ThingDef d)
		{
			return warmup(d) + cooldown(d) + ((d.Verbs[0].burstShotCount - 1) * d.Verbs[0].ticksBetweenBurstShots).TicksToSeconds();
		}

		private static float accTouch(ThingDef d)
		{
			return d.GetStatValueAbstract(StatDefOf.AccuracyTouch);
		}

		private static float accShort(ThingDef d)
		{
			return d.GetStatValueAbstract(StatDefOf.AccuracyShort);
		}

		private static float accMed(ThingDef d)
		{
			return d.GetStatValueAbstract(StatDefOf.AccuracyMedium);
		}

		private static float accLong(ThingDef d)
		{
			return d.GetStatValueAbstract(StatDefOf.AccuracyLong);
		}

		private static float accAvg(ThingDef d)
		{
			return (accTouch(d) + accShort(d) + accMed(d) + accLong(d)) / 4f;
		}

		private static float dpsAvg(ThingDef d)
		{
			return dpsMissless(d) * accAvg(d);
		}

		private static float dpsMissless(ThingDef d)
		{
			int num = burstShots(d);
			float num2 = warmup(d) + cooldown(d);
			num2 += (float)(num - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
			return damage(d) * (float)num / num2;
		}

		[DebugOutput]
		public static void WeaponsRanged()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsRangedWeapon).OrderByDescending(dpsAvg), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("damage", (ThingDef d) => damage(d).ToString()), new TableDataGetter<ThingDef>("AP", (ThingDef d) => armorPenetration(d).ToStringPercent()), new TableDataGetter<ThingDef>("stop\npower", (ThingDef d) => (!(stoppingPower(d) > 0f)) ? "" : stoppingPower(d).ToString("F1")), new TableDataGetter<ThingDef>("warmup", (ThingDef d) => warmup(d).ToString("F2")), new TableDataGetter<ThingDef>("burst\nshots", (ThingDef d) => burstShots(d).ToString()), new TableDataGetter<ThingDef>("cooldown", (ThingDef d) => cooldown(d).ToString("F2")), new TableDataGetter<ThingDef>("full\ncycle", (ThingDef d) => fullcycle(d).ToString("F2")), new TableDataGetter<ThingDef>("range", (ThingDef d) => d.Verbs[0].range.ToString("F1")), new TableDataGetter<ThingDef>("projectile\nspeed", (ThingDef d) => (d.projectile == null) ? "" : d.projectile.speed.ToString("F0")), new TableDataGetter<ThingDef>("dps\nmissless", (ThingDef d) => dpsMissless(d).ToString("F2")), new TableDataGetter<ThingDef>("accuracy\ntouch (" + 3f + ")", (ThingDef d) => accTouch(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nshort (" + 12f + ")", (ThingDef d) => accShort(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nmed (" + 25f + ")", (ThingDef d) => accMed(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nlong (" + 40f + ")", (ThingDef d) => accLong(d).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\navg", (ThingDef d) => accAvg(d).ToString("F2")), new TableDataGetter<ThingDef>("forced\nmiss\nradius", (ThingDef d) => (!(d.Verbs[0].forcedMissRadius > 0f)) ? "" : d.Verbs[0].forcedMissRadius.ToString()), new TableDataGetter<ThingDef>("dps\ntouch", (ThingDef d) => (dpsMissless(d) * accTouch(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nshort", (ThingDef d) => (dpsMissless(d) * accShort(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nmed", (ThingDef d) => (dpsMissless(d) * accMed(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nlong", (ThingDef d) => (dpsMissless(d) * accLong(d)).ToString("F2")), new TableDataGetter<ThingDef>("dps\navg", (ThingDef d) => dpsAvg(d).ToString("F2")), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake).ToString("F0")), new TableDataGetter<ThingDef>("dpsAvg*100 / market value", (ThingDef d) => (dpsAvg(d) * 100f / d.GetStatValueAbstract(StatDefOf.MarketValue)).ToString("F3")));
		}

		[DebugOutput]
		public static void Turrets()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Building && d.building.IsTurret), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F0")), new TableDataGetter<ThingDef>("cost\nlist", (ThingDef d) => (!d.costList.NullOrEmpty()) ? d.costList.Select((ThingDefCountClass x) => x.Summary).ToCommaList() : ""), new TableDataGetter<ThingDef>("cost\nstuff\ncount", (ThingDef d) => (!d.MadeFromStuff) ? "" : d.costStuffCount.ToString()), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild).ToString("F0")), new TableDataGetter<ThingDef>("hp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MaxHitPoints).ToString("F0")), new TableDataGetter<ThingDef>("damage", (ThingDef d) => damage(d.building.turretGunDef).ToString()), new TableDataGetter<ThingDef>("AP", (ThingDef d) => armorPenetration(d.building.turretGunDef).ToStringPercent()), new TableDataGetter<ThingDef>("stop\npower", (ThingDef d) => (!(stoppingPower(d.building.turretGunDef) > 0f)) ? "" : stoppingPower(d.building.turretGunDef).ToString("F1")), new TableDataGetter<ThingDef>("warmup", (ThingDef d) => warmup(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("burst\nshots", (ThingDef d) => burstShots(d.building.turretGunDef).ToString()), new TableDataGetter<ThingDef>("cooldown", (ThingDef d) => cooldown(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("full\ncycle", (ThingDef d) => fullcycle(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("range", (ThingDef d) => d.building.turretGunDef.Verbs[0].range.ToString("F1")), new TableDataGetter<ThingDef>("projectile\nspeed", (ThingDef d) => (d.building.turretGunDef.projectile == null) ? "" : d.building.turretGunDef.projectile.speed.ToString("F0")), new TableDataGetter<ThingDef>("dps\nmissless", (ThingDef d) => dpsMissless(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("accuracy\ntouch (" + 3f + ")", (ThingDef d) => accTouch(d.building.turretGunDef).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nshort (" + 12f + ")", (ThingDef d) => accShort(d.building.turretGunDef).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nmed (" + 25f + ")", (ThingDef d) => accMed(d.building.turretGunDef).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\nlong (" + 40f + ")", (ThingDef d) => accLong(d.building.turretGunDef).ToStringPercent()), new TableDataGetter<ThingDef>("accuracy\navg", (ThingDef d) => accAvg(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("forced\nmiss\nradius", (ThingDef d) => (!(d.building.turretGunDef.Verbs[0].forcedMissRadius > 0f)) ? "" : d.building.turretGunDef.Verbs[0].forcedMissRadius.ToString()), new TableDataGetter<ThingDef>("dps\ntouch", (ThingDef d) => (dpsMissless(d.building.turretGunDef) * accTouch(d.building.turretGunDef)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nshort", (ThingDef d) => (dpsMissless(d.building.turretGunDef) * accShort(d.building.turretGunDef)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nmed", (ThingDef d) => (dpsMissless(d.building.turretGunDef) * accMed(d.building.turretGunDef)).ToString("F2")), new TableDataGetter<ThingDef>("dps\nlong", (ThingDef d) => (dpsMissless(d.building.turretGunDef) * accLong(d.building.turretGunDef)).ToString("F2")), new TableDataGetter<ThingDef>("dps\navg", (ThingDef d) => dpsAvg(d.building.turretGunDef).ToString("F2")), new TableDataGetter<ThingDef>("dpsAvg / $100", (ThingDef d) => (dpsAvg(d.building.turretGunDef) / (d.GetStatValueAbstract(StatDefOf.MarketValue) / 100f)).ToString("F3")), new TableDataGetter<ThingDef>("fuel\nshot capacity", (ThingDef d) => fuelCapacity(d).ToString()), new TableDataGetter<ThingDef>("fuel\ntype", (ThingDef d) => fuelType(d)), new TableDataGetter<ThingDef>("fuel to\nreload", (ThingDef d) => fuelToReload(d).ToString()));
			static string fuelCapacity(ThingDef d)
			{
				if (!d.HasComp(typeof(CompRefuelable)))
				{
					return "";
				}
				return d.GetCompProperties<CompProperties_Refuelable>().fuelCapacity.ToString();
			}
			static string fuelToReload(ThingDef d)
			{
				if (!d.HasComp(typeof(CompRefuelable)))
				{
					return "";
				}
				return (d.GetCompProperties<CompProperties_Refuelable>().fuelCapacity / d.GetCompProperties<CompProperties_Refuelable>().FuelMultiplierCurrentDifficulty).ToString();
			}
			static string fuelType(ThingDef d)
			{
				if (!d.HasComp(typeof(CompRefuelable)))
				{
					return "";
				}
				return d.GetCompProperties<CompProperties_Refuelable>().fuelFilter.Summary;
			}
		}

		[DebugOutput]
		public static void WeaponsMelee()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Stuffless", delegate
			{
				DoTablesInternalMelee(null);
			}));
			foreach (ThingDef item in from st in DefDatabase<ThingDef>.AllDefs
				where st.IsStuff
				where DefDatabase<ThingDef>.AllDefs.Any((ThingDef wd) => wd.IsMeleeWeapon && st.stuffProps.CanMake(wd))
				select st into td
				orderby td.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier) descending
				select td)
			{
				ThingDef localStuff = item;
				float statValueAbstract = localStuff.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier);
				float statValueAbstract2 = localStuff.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier);
				float statFactorFromList = localStuff.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				list.Add(new FloatMenuOption(localStuff.defName + " (sharp " + statValueAbstract + ", blunt " + statValueAbstract2 + ", cooldown " + statFactorFromList + ")", delegate
				{
					DoTablesInternalMelee(localStuff);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		private static void DoTablesInternalMelee(ThingDef stuff, bool doRaces = false)
		{
			IEnumerable<Def> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsWeapon).Cast<Def>().Concat(DefDatabase<HediffDef>.AllDefs.Where((HediffDef h) => h.CompProps<HediffCompProperties_VerbGiver>() != null).Cast<Def>());
			if (doRaces)
			{
				enumerable = enumerable.Concat(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.race != null).Cast<Def>());
			}
			enumerable = enumerable.OrderByDescending((Def h) => meleeDpsGetter(h));
			DebugTables.MakeTablesDialog(enumerable, new TableDataGetter<Def>("defName", (Def d) => d.defName), new TableDataGetter<Def>("melee\nDPS", (Def d) => meleeDpsGetter(d).ToString("F2")), new TableDataGetter<Def>("melee\ndamage\naverage", (Def d) => meleeDamageGetter(d).ToString("F2")), new TableDataGetter<Def>("melee\ncooldown\naverage", (Def d) => meleeCooldownGetter(d).ToString("F2")), new TableDataGetter<Def>("melee\nAP", (Def d) => meleeAPGetter(d).ToString("F2")), new TableDataGetter<Def>("ranged\ndamage", (Def d) => rangedDamageGetter(d).ToString()), new TableDataGetter<Def>("ranged\nwarmup", (Def d) => rangedWarmupGetter(d).ToString("F2")), new TableDataGetter<Def>("ranged\ncooldown", (Def d) => rangedCooldownGetter(d).ToString("F2")), new TableDataGetter<Def>("market value", (Def d) => marketValueGetter(d).ToStringMoney()), new TableDataGetter<Def>("work to make", delegate(Def d)
			{
				ThingDef thingDef3 = d as ThingDef;
				return (thingDef3 == null) ? "-" : thingDef3.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0");
			}), new TableDataGetter<Def>((stuff != null) ? (stuff.defName + " CanMake") : "CanMake", delegate(Def d)
			{
				if (stuff == null)
				{
					return "n/a";
				}
				ThingDef thingDef2 = d as ThingDef;
				return (thingDef2 == null) ? "-" : stuff.stuffProps.CanMake(thingDef2).ToStringCheckBlank();
			}), new TableDataGetter<Def>("assumed\nmelee\nhit chance", (Def d) => 0.82f.ToStringPercent()));
			float marketValueGetter(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					return thingDef.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
				}
				HediffDef hediffDef = d as HediffDef;
				if (hediffDef != null)
				{
					if (hediffDef.spawnThingOnRemoved == null)
					{
						return 0f;
					}
					return hediffDef.spawnThingOnRemoved.GetStatValueAbstract(StatDefOf.MarketValue);
				}
				return -1f;
			}
			float meleeAPGetter(Def d)
			{
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				if (concreteExampleVerbs.OfType<Verb_MeleeAttack>().Any())
				{
					return concreteExampleVerbs.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedArmorPenetration(v, null), (Verb_MeleeAttack v) => v.verbProps.AdjustedArmorPenetration(v, null));
				}
				return -1f;
			}
			float meleeCooldownGetter(Def d)
			{
				List<Verb> concreteExampleVerbs2 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				if (concreteExampleVerbs2.OfType<Verb_MeleeAttack>().Any())
				{
					return concreteExampleVerbs2.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null), (Verb_MeleeAttack v) => v.verbProps.AdjustedCooldown(v, null));
				}
				return -1f;
			}
			float meleeDamageGetter(Def d)
			{
				List<Verb> concreteExampleVerbs3 = VerbUtility.GetConcreteExampleVerbs(d, stuff);
				if (concreteExampleVerbs3.OfType<Verb_MeleeAttack>().Any())
				{
					return concreteExampleVerbs3.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null), (Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeDamageAmount(v, null));
				}
				return -1f;
			}
			float meleeDpsGetter(Def d)
			{
				return meleeDamageGetter(d) * 0.82f / meleeCooldownGetter(d);
			}
			float rangedCooldownGetter(Def d)
			{
				return VerbUtility.GetConcreteExampleVerbs(d, stuff).OfType<Verb_LaunchProjectile>().FirstOrDefault()?.verbProps.defaultCooldownTime ?? (-1f);
			}
			float rangedDamageGetter(Def d)
			{
				Verb_LaunchProjectile verb_LaunchProjectile = VerbUtility.GetConcreteExampleVerbs(d, stuff).OfType<Verb_LaunchProjectile>().FirstOrDefault();
				if (verb_LaunchProjectile != null && verb_LaunchProjectile.GetProjectile() != null)
				{
					return verb_LaunchProjectile.GetProjectile().projectile.GetDamageAmount(null);
				}
				return -1f;
			}
			float rangedWarmupGetter(Def d)
			{
				return VerbUtility.GetConcreteExampleVerbs(d, stuff).OfType<Verb_LaunchProjectile>().FirstOrDefault()?.verbProps.warmupTime ?? (-1f);
			}
		}

		[DebugOutput]
		public static void Tools()
		{
			var tools = (from x in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => !x.tools.NullOrEmpty()).SelectMany((ThingDef x) => x.tools.Select((Tool y) => new
				{
					Parent = (Def)x,
					Tool = y
				})).Concat(DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => !x.tools.NullOrEmpty()).SelectMany((TerrainDef x) => x.tools.Select((Tool y) => new
				{
					Parent = (Def)x,
					Tool = y
				})))
					.Concat(DefDatabase<HediffDef>.AllDefs.Where((HediffDef x) => x.HasComp(typeof(HediffComp_VerbGiver)) && !x.CompProps<HediffCompProperties_VerbGiver>().tools.NullOrEmpty()).SelectMany((HediffDef x) => x.CompProps<HediffCompProperties_VerbGiver>().tools.Select((Tool y) => new
					{
						Parent = (Def)x,
						Tool = y
					})))
				orderby x.Parent.defName, x.Tool.power descending
				select x).ToList();
			Dictionary<Tool, float> selWeight = tools.ToDictionary(x => x.Tool, x => x.Tool.VerbsProperties.Average((VerbProperties y) => y.AdjustedMeleeSelectionWeight(x.Tool, null, null, null, x.Parent is ThingDef && ((ThingDef)x.Parent).category == ThingCategory.Pawn)));
			Dictionary<Def, float> selWeightSumInGroup = tools.Select(x => x.Parent).Distinct().ToDictionary((Def x) => x, (Def x) => tools.Where(y => y.Parent == x).Sum(y => selWeight[y.Tool]));
			DebugTables.MakeTablesDialog(tools.Select((x, int index) => index), new TableDataGetter<int>("label", (int x) => tools[x].Tool.label), new TableDataGetter<int>("source", (int x) => tools[x].Parent.defName), new TableDataGetter<int>("power", (int x) => tools[x].Tool.power.ToString("0.##")), new TableDataGetter<int>("AP", delegate(int x)
			{
				float num = tools[x].Tool.armorPenetration;
				if (num < 0f)
				{
					num = tools[x].Tool.power * 0.015f;
				}
				return num.ToStringPercent();
			}), new TableDataGetter<int>("cooldown", (int x) => tools[x].Tool.cooldownTime.ToString("0.##")), new TableDataGetter<int>("selection weight", (int x) => selWeight[tools[x].Tool].ToString("0.##")), new TableDataGetter<int>("selection weight\nwithin def", (int x) => (selWeight[tools[x].Tool] / selWeightSumInGroup[tools[x].Parent]).ToStringPercent()), new TableDataGetter<int>("chance\nfactor", (int x) => (tools[x].Tool.chanceFactor != 1f) ? tools[x].Tool.chanceFactor.ToString("0.##") : ""), new TableDataGetter<int>("adds hediff", (int x) => (tools[x].Tool.hediff == null) ? "" : tools[x].Tool.hediff.defName), new TableDataGetter<int>("linked body parts", (int x) => (tools[x].Tool.linkedBodyPartsGroup == null) ? "" : tools[x].Tool.linkedBodyPartsGroup.defName), new TableDataGetter<int>("surprise attack", (int x) => (tools[x].Tool.surpriseAttack == null || tools[x].Tool.surpriseAttack.extraMeleeDamages.NullOrEmpty()) ? "" : (tools[x].Tool.surpriseAttack.extraMeleeDamages[0].amount.ToString("0.##") + " (" + tools[x].Tool.surpriseAttack.extraMeleeDamages[0].def.defName + ")")), new TableDataGetter<int>("capacities", (int x) => tools[x].Tool.capacities.ToStringSafeEnumerable()), new TableDataGetter<int>("maneuvers", (int x) => tools[x].Tool.Maneuvers.ToStringSafeEnumerable()), new TableDataGetter<int>("always weapon", (int x) => (!tools[x].Tool.alwaysTreatAsWeapon) ? "" : "always wep"), new TableDataGetter<int>("id", (int x) => tools[x].Tool.id));
		}

		[DebugOutput]
		public static void ApparelByStuff()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Stuffless", delegate
			{
				DoTableInternalApparel(null);
			}));
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => td.IsStuff))
			{
				ThingDef localStuff = item;
				list.Add(new FloatMenuOption(localStuff.defName, delegate
				{
					DoTableInternalApparel(localStuff);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ApparelArmor()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
			list.Add(new TableDataGetter<ThingDef>("stuff", (ThingDef x) => x.MadeFromStuff.ToStringCheckBlank()));
			list.Add(new TableDataGetter<ThingDef>("mass", (ThingDef x) => x.BaseMass));
			list.Add(new TableDataGetter<ThingDef>("mrkt\nvalue", (ThingDef x) => x.BaseMarketValue.ToString("F0")));
			list.Add(new TableDataGetter<ThingDef>("hp", (ThingDef x) => x.BaseMaxHitPoints));
			list.Add(new TableDataGetter<ThingDef>("flama\nbility", (ThingDef x) => x.BaseFlammability));
			list.Add(new TableDataGetter<ThingDef>("recipe\nmin\nskill", (ThingDef x) => (x.recipeMaker == null || x.recipeMaker.skillRequirements.NullOrEmpty()) ? "" : (x.recipeMaker.skillRequirements[0].skill.defName + " " + x.recipeMaker.skillRequirements[0].minLevel)));
			list.Add(new TableDataGetter<ThingDef>("equip\ndelay", (ThingDef x) => x.GetStatValueAbstract(StatDefOf.EquipDelay)));
			list.Add(new TableDataGetter<ThingDef>("none", (ThingDef x) => x.MadeFromStuff ? "" : (x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat).ToStringPercent())));
			foreach (ThingDef item in new List<ThingDef>
			{
				ThingDefOf.Steel,
				ThingDefOf.Plasteel,
				ThingDefOf.Cloth,
				ThingDef.Named("Leather_Patch"),
				ThingDefOf.Leather_Plain,
				ThingDef.Named("Leather_Heavy"),
				ThingDef.Named("Leather_Thrumbo"),
				ThingDef.Named("Synthread"),
				ThingDef.Named("Hyperweave"),
				ThingDef.Named("DevilstrandCloth"),
				ThingDef.Named("WoolSheep"),
				ThingDef.Named("WoolMegasloth"),
				ThingDefOf.BlocksGranite,
				ThingDefOf.Silver,
				ThingDefOf.Gold
			})
			{
				ThingDef stuffLocal = item;
				if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
				{
					list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), (ThingDef x) => (!stuffLocal.stuffProps.CanMake(x)) ? "" : (x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat, stuffLocal).ToStringPercent())));
				}
			}
			DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
				where x.IsApparel
				orderby x.BaseMarketValue
				select x, list.ToArray());
		}

		[DebugOutput]
		public static void ApparelInsulation()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
			list.Add(new TableDataGetter<ThingDef>("none", (ThingDef x) => x.MadeFromStuff ? "" : (x.GetStatValueAbstract(StatDefOf.Insulation_Heat).ToStringTemperature() + " / " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold).ToStringTemperature())));
			foreach (ThingDef item in from x in DefDatabase<ThingDef>.AllDefs
				where x.IsStuff
				orderby x.BaseMarketValue
				select x)
			{
				ThingDef stuffLocal = item;
				if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
				{
					list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), (ThingDef x) => (!stuffLocal.stuffProps.CanMake(x)) ? "" : (x.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuffLocal).ToString("F1") + ", " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuffLocal).ToString("F1"))));
				}
			}
			DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
				where x.IsApparel
				orderby x.BaseMarketValue
				select x, list.ToArray());
		}

		private static void DoTableInternalApparel(ThingDef stuff)
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsApparel && (stuff == null || (d.MadeFromStuff && stuff.stuffProps.CanMake(d)))), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("bodyParts", (ThingDef d) => GenText.ToSpaceList(d.apparel.bodyPartGroups.Select((BodyPartGroupDef bp) => bp.defName))), new TableDataGetter<ThingDef>("layers", (ThingDef d) => GenText.ToSpaceList(d.apparel.layers.Select((ApparelLayerDef l) => l.ToString()))), new TableDataGetter<ThingDef>("tags", (ThingDef d) => GenText.ToSpaceList(d.apparel.tags.Select((string t) => t.ToString()))), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0")), new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, stuff).ToString("F0")), new TableDataGetter<ThingDef>("insCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff).ToString("F1")), new TableDataGetter<ThingDef>("insHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff).ToString("F1")), new TableDataGetter<ThingDef>("blunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuff).ToString("F2")), new TableDataGetter<ThingDef>("sharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultArmor", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierArmor, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultInsuCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Cold, stuff).ToString("F2")), new TableDataGetter<ThingDef>("SEMultInsuHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Heat, stuff).ToString("F2")), new TableDataGetter<ThingDef>("equipTime", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.EquipDelay, stuff).ToString("F1")));
		}

		[DebugOutput]
		public static void ResearchProjects()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ResearchProjectDef>.AllDefs, new TableDataGetter<ResearchProjectDef>("defName", (ResearchProjectDef d) => d.defName), new TableDataGetter<ResearchProjectDef>("label", (ResearchProjectDef d) => d.label), new TableDataGetter<ResearchProjectDef>("baseCost", (ResearchProjectDef d) => d.baseCost), new TableDataGetter<ResearchProjectDef>("techLevel", (ResearchProjectDef d) => d.techLevel.ToString()), new TableDataGetter<ResearchProjectDef>("prerequisites", (ResearchProjectDef d) => (d.prerequisites != null && d.prerequisites.Count != 0) ? string.Join(",", d.prerequisites.Select((ResearchProjectDef p) => d.defName).ToArray()) : "NONE"), new TableDataGetter<ResearchProjectDef>("hiddenPrerequisites", (ResearchProjectDef d) => (d.hiddenPrerequisites != null && d.hiddenPrerequisites.Count != 0) ? string.Join(",", d.hiddenPrerequisites.Select((ResearchProjectDef p) => d.defName).ToArray()) : "NONE"), new TableDataGetter<ResearchProjectDef>("requiredResearchBuilding", (ResearchProjectDef d) => d.requiredResearchBuilding), new TableDataGetter<ResearchProjectDef>("techprintCount", (ResearchProjectDef d) => d.techprintCount), new TableDataGetter<ResearchProjectDef>("heldByFactionCategoryTags", (ResearchProjectDef d) => (d.heldByFactionCategoryTags != null) ? string.Join(",", d.heldByFactionCategoryTags.Select((string fc) => fc).ToArray()) : "NONE"));
		}

		[DebugOutput]
		public static void ThingsExistingList()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingRequestGroup value in Enum.GetValues(typeof(ThingRequestGroup)))
			{
				ThingRequestGroup localRg = value;
				FloatMenuOption item = new FloatMenuOption(localRg.ToString(), delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					List<Thing> list2 = Find.CurrentMap.listerThings.ThingsInGroup(localRg);
					stringBuilder.AppendLine(string.Concat("Global things in group ", localRg, " (count ", list2.Count, ")"));
					Log.Message(DebugLogsUtility.ThingListToUniqueCountString(list2));
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void ThingFillageAndPassability()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.passability != 0 || allDef.fillPercent > 0f)
				{
					stringBuilder.Append(allDef.defName + " - pass=" + allDef.passability.ToString() + ", fill=" + allDef.fillPercent.ToStringPercent());
					if (allDef.passability == Traversability.Impassable && allDef.fillPercent < 0.1f)
					{
						stringBuilder.Append("   ALERT, impassable with low fill");
					}
					if (allDef.passability != Traversability.Impassable && allDef.fillPercent > 0.8f)
					{
						stringBuilder.Append("    ALERT, passabile with very high fill");
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void ThingDamageData()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.useHitPoints
				orderby d.category, d.defName
				select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("hp", (ThingDef d) => d.BaseMaxHitPoints.ToString()), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => (!(d.BaseFlammability > 0f)) ? "" : d.BaseFlammability.ToString()), new TableDataGetter<ThingDef>("uses stuff", (ThingDef d) => d.MadeFromStuff.ToStringCheckBlank()), new TableDataGetter<ThingDef>("deterioration rate", (ThingDef d) => (!(d.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0f)) ? "" : d.GetStatValueAbstract(StatDefOf.DeteriorationRate).ToString()), new TableDataGetter<ThingDef>("days to deterioriate", (ThingDef d) => (!(d.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0f)) ? "" : ((float)d.BaseMaxHitPoints / d.GetStatValueAbstract(StatDefOf.DeteriorationRate)).ToString()));
		}

		[DebugOutput]
		public static void UnfinishedThings()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.isUnfinishedThing
				orderby d.category, d.defName
				select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("billGivers", (ThingDef d) => string.Join(", ", (from td in DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.unfinishedThingDef == d).SelectMany((RecipeDef r) => DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.AllRecipes != null && td.AllRecipes.Contains(r)))
				select td.defName).Distinct())));
		}

		[DebugOutput]
		public static void ThingMasses()
		{
			IOrderedEnumerable<ThingDef> dataSources = from x in DefDatabase<ThingDef>.AllDefsListForReading
				where x.category == ThingCategory.Item || x.Minifiable
				where x.thingClass != typeof(MinifiedThing) && x.thingClass != typeof(UnfinishedThing)
				orderby x.GetStatValueAbstract(StatDefOf.Mass), x.GetStatValueAbstract(StatDefOf.MarketValue)
				select x;
			Func<ThingDef, float, string> perPawn = (ThingDef d, float bodySize) => (bodySize * 35f / d.GetStatValueAbstract(StatDefOf.Mass)).ToString("F0");
			DebugTables.MakeTablesDialog(dataSources, new TableDataGetter<ThingDef>("defName", delegate(ThingDef d)
			{
				if (d.Minifiable)
				{
					return d.defName + " (minified)";
				}
				string text = d.defName;
				if (!d.EverHaulable)
				{
					text += " (not haulable)";
				}
				return text;
			}), new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Mass).ToString()), new TableDataGetter<ThingDef>("per human", (ThingDef d) => perPawn(d, ThingDefOf.Human.race.baseBodySize)), new TableDataGetter<ThingDef>("per muffalo", (ThingDef d) => perPawn(d, ThingDefOf.Muffalo.race.baseBodySize)), new TableDataGetter<ThingDef>("per dromedary", (ThingDef d) => perPawn(d, ThingDefOf.Dromedary.race.baseBodySize)), new TableDataGetter<ThingDef>("per nutrition", (ThingDef d) => perNutrition(d)), new TableDataGetter<ThingDef>("small volume", (ThingDef d) => (!d.smallVolume) ? "" : "small"));
			static string perNutrition(ThingDef d)
			{
				if (d.ingestible == null || d.GetStatValueAbstract(StatDefOf.Nutrition) == 0f)
				{
					return "";
				}
				return (d.GetStatValueAbstract(StatDefOf.Mass) / d.GetStatValueAbstract(StatDefOf.Nutrition)).ToString("F2");
			}
		}

		[DebugOutput]
		public static void ThingFillPercents()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.fillPercent > 0f
				orderby d.fillPercent descending
				select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("fillPercent", (ThingDef d) => d.fillPercent.ToStringPercent()), new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()));
		}

		[DebugOutput]
		public static void ThingNutritions()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.ingestible != null), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F1")), new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Nutrition).ToString("F2")), new TableDataGetter<ThingDef>("nutrition per value", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.Nutrition) / d.BaseMarketValue).ToString("F3")));
		}

		public static void MakeTablePairsByThing(List<ThingStuffPair> pairList)
		{
			DefMap<ThingDef, float> totalCommMult = new DefMap<ThingDef, float>();
			DefMap<ThingDef, float> totalComm = new DefMap<ThingDef, float>();
			DefMap<ThingDef, int> pairCount = new DefMap<ThingDef, int>();
			foreach (ThingStuffPair pair in pairList)
			{
				totalCommMult[pair.thing] += pair.commonalityMultiplier;
				totalComm[pair.thing] += pair.Commonality;
				pairCount[pair.thing]++;
			}
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => pairList.Any((ThingStuffPair pa) => pa.thing == d)), new TableDataGetter<ThingDef>("thing", (ThingDef t) => t.defName), new TableDataGetter<ThingDef>("pair count", (ThingDef t) => pairCount[t].ToString()), new TableDataGetter<ThingDef>("total commonality multiplier ", (ThingDef t) => totalCommMult[t].ToString("F4")), new TableDataGetter<ThingDef>("total commonality", (ThingDef t) => totalComm[t].ToString("F4")), new TableDataGetter<ThingDef>("generateCommonality", (ThingDef t) => t.generateCommonality.ToString("F4")));
		}

		public static string ToStringEmptyZero(this float f, string format)
		{
			if (f <= 0f)
			{
				return "";
			}
			return f.ToString(format);
		}

		public static string ToStringPercentEmptyZero(this float f, string format = "F0")
		{
			if (f <= 0f)
			{
				return "";
			}
			return f.ToStringPercent(format);
		}

		public static string ToStringCheckBlank(this bool b)
		{
			if (!b)
			{
				return "";
			}
			return "✓";
		}
	}
}
