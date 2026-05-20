using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;

namespace Verse;

public static class DebugOutputsIncidents
{
	[DebugOutput("Incidents", false)]
	public static void IncidentChances()
	{
		List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
		for (int i = 0; i < storytellerComps.Count; i++)
		{
			if (storytellerComps[i] is StorytellerComp_CategoryMTB storytellerComp_CategoryMTB && ((StorytellerCompProperties_CategoryMTB)storytellerComp_CategoryMTB.props).category == IncidentCategoryDefOf.Misc)
			{
				storytellerComp_CategoryMTB.DebugTablesIncidentChances();
			}
		}
	}

	[DebugOutput("Incidents", false)]
	public static void IncidentChancesSampled()
	{
		Dictionary<IncidentDef, int> samples = new Dictionary<IncidentDef, int>();
		int fireCount = 0;
		List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
		for (int i = 0; i < storytellerComps.Count; i++)
		{
			StorytellerComp storytellerComp = storytellerComps[i];
			if (storytellerComp == null)
			{
				continue;
			}
			for (int j = 0; j < 50000; j++)
			{
				foreach (FiringIncident item in storytellerComp.MakeIntervalIncidents(Find.AnyPlayerHomeMap))
				{
					if (samples.TryGetValue(item.def, out var value))
					{
						samples[item.def] = value + 1;
					}
					else
					{
						samples.Add(item.def, 1);
					}
					fireCount++;
				}
			}
		}
		DebugTables.MakeTablesDialog(samples.Keys, new TableDataGetter<IncidentDef>("defName", (IncidentDef d) => d.defName), new TableDataGetter<IncidentDef>("category", (IncidentDef d) => d.category), new TableDataGetter<IncidentDef>("amount fired", (IncidentDef d) => samples[d]), new TableDataGetter<IncidentDef>("fire chance", (IncidentDef d) => ((float)samples[d] / (float)fireCount).ToString("0.0000")));
	}

	[DebugOutput("Incidents", true)]
	public static void FutureIncidents()
	{
		StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(currentMapOnly: false);
	}

	[DebugOutput("Incidents", true)]
	public static void FutureIncidentsCurrentMap()
	{
		StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(currentMapOnly: true);
	}

	[DebugOutput("Incidents", true)]
	public static void IncidentTargetsList()
	{
		StorytellerUtility.DebugLogTestIncidentTargets();
	}

	[DebugOutput("Incidents", false)]
	public static void MinThreatPoints()
	{
		int ticksGame = Find.TickManager.TicksGame;
		DebugTables.MakeTablesDialog(hourOffsets(), new TableDataGetter<int>("hours passed", (int h) => h), new TableDataGetter<int>("days passed", (int h) => h / 24), new TableDataGetter<int>("points min", (int h) => StorytellerUtility.GlobalPointsMin()), new TableDataGetter<int>("points min ceiling", (int h) => Find.Storyteller.difficulty.MinThreatPointsCeiling), new TableDataGetter<int>("points min floor", (int h) => 35f));
		Find.TickManager.DebugSetTicksGame(ticksGame);
		static IEnumerable<int> hourOffsets()
		{
			for (int i = 0; i < 1200; i += 6)
			{
				Find.TickManager.DebugSetTicksGame(2500 * i);
				yield return i;
			}
		}
	}

	[DebugOutput("Incidents", false)]
	public static void CurrentThreatPoints()
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap != null)
		{
			currentMap.wealthWatcher.ForceRecount();
			Log.Message(StorytellerUtility.DefaultThreatPointsNow(currentMap).ToString("F2"));
		}
	}

	[DebugOutput("Incidents", false)]
	public static void PawnArrivalCandidates()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(IncidentDefOf.RaidEnemy.defName);
		stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidEnemy.Worker).DebugListingOfGroupSources());
		stringBuilder.AppendLine(IncidentDefOf.RaidFriendly.defName);
		stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidFriendly.Worker).DebugListingOfGroupSources());
		stringBuilder.AppendLine(IncidentDefOf.VisitorGroup.defName);
		stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.VisitorGroup.Worker).DebugListingOfGroupSources());
		stringBuilder.AppendLine(IncidentDefOf.TravelerGroup.defName);
		stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TravelerGroup.Worker).DebugListingOfGroupSources());
		stringBuilder.AppendLine(IncidentDefOf.TraderCaravanArrival.defName);
		stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TraderCaravanArrival.Worker).DebugListingOfGroupSources());
		Log.Message(stringBuilder.ToString());
	}

	[DebugOutput("Incidents", false)]
	public static void TraderKinds()
	{
		DebugTables.MakeTablesDialog(DefDatabase<TraderKindDef>.AllDefs, new TableDataGetter<TraderKindDef>("defName", (TraderKindDef d) => d.defName), new TableDataGetter<TraderKindDef>("orbital", (TraderKindDef d) => d.orbital.ToStringCheckBlank()), new TableDataGetter<TraderKindDef>("requestable", (TraderKindDef d) => d.requestable.ToStringCheckBlank()), new TableDataGetter<TraderKindDef>("commonality\nbase", (TraderKindDef d) => d.commonality.ToString("F2")), new TableDataGetter<TraderKindDef>("commonality\nnow", (TraderKindDef d) => d.CalculatedCommonality.ToString("F2")), new TableDataGetter<TraderKindDef>("faction", (TraderKindDef d) => (d.faction == null) ? "" : d.faction.defName), new TableDataGetter<TraderKindDef>("permit\nrequired", (TraderKindDef d) => (d.permitRequiredForTrading == null) ? "" : d.permitRequiredForTrading.defName), new TableDataGetter<TraderKindDef>("average\nvalue", (TraderKindDef d) => ((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).DebugAverageTotalStockValue(d).ToString("F0")));
	}

	[DebugOutput("Incidents", false)]
	public static void TraderKindThings()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
		foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
		{
			TraderKindDef localTk = allDef;
			list.Add(new TableDataGetter<ThingDef>(localTk.defName.Replace("_", "\n").Shorten(), (ThingDef td) => TraderTradeability(localTk, td)));
		}
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.001f && !d.isUnfinishedThing && !d.IsCorpse && d.PlayerAcquirable && d != ThingDefOf.Silver && !d.thingCategories.NullOrEmpty()) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
			orderby d.thingCategories.NullOrEmpty() ? "zzzzzzz" : d.thingCategories[0].defName, d.BaseMarketValue
			select d, list.ToArray());
		static string TraderTradeability(TraderKindDef tk, ThingDef td)
		{
			Tradeability tradeability = Tradeability.None;
			foreach (StockGenerator stockGenerator in tk.stockGenerators)
			{
				Tradeability tradeability2 = stockGenerator.TradeabilityFor(td);
				if (tradeability == Tradeability.None)
				{
					tradeability = tradeability2;
				}
				else if ((tradeability == Tradeability.Buyable && tradeability2 == Tradeability.Sellable) || (tradeability == Tradeability.Sellable && tradeability2 == Tradeability.Buyable))
				{
					tradeability = Tradeability.All;
					break;
				}
			}
			return tradeability switch
			{
				Tradeability.None => "", 
				Tradeability.All => "✓", 
				Tradeability.Buyable => "sell", 
				Tradeability.Sellable => "buy", 
				_ => td.tradeability.ToString(), 
			};
		}
	}

	[DebugOutput("Incidents", false)]
	public static void TraderStockMarketValues()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
		{
			stringBuilder.AppendLine(allDef.defName + " : " + ((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).DebugAverageTotalStockValue(allDef).ToString("F0"));
		}
		Log.Message(stringBuilder.ToString());
	}

	[DebugOutput("Incidents", false)]
	public static void TraderStockGeneration()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
		{
			TraderKindDef localDef = allDef;
			FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
			{
				Log.Message(((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).DebugGenerationDataFor(localDef));
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Incidents", false)]
	public static void TraderStockGeneratorsDefs()
	{
		if (Find.CurrentMap == null)
		{
			Log.Error("Requires visible map.");
			return;
		}
		StringBuilder sb = new StringBuilder();
		Action<StockGenerator> obj = delegate(StockGenerator gen)
		{
			if (gen is StockGenerator_Tag stockGenerator_Tag && !stockGenerator_Tag.tradeTag.NullOrEmpty())
			{
				sb.AppendLine(gen.GetType()?.ToString() + " (" + stockGenerator_Tag.tradeTag + ")");
			}
			else
			{
				sb.AppendLine(gen.GetType().ToString());
			}
			sb.AppendLine("ALLOWED DEFS:");
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where(gen.HandlesThingDef))
			{
				sb.AppendLine(item.defName + " [" + item.BaseMarketValue + "]");
			}
			sb.AppendLine();
			sb.AppendLine("GENERATION TEST:");
			gen.countRange = IntRange.One;
			for (int i = 0; i < 30; i++)
			{
				foreach (Thing item2 in gen.GenerateThings(Find.CurrentMap.Tile))
				{
					sb.AppendLine(item2.Label + " [" + item2.MarketValue + "]");
				}
			}
			sb.AppendLine("---------------------------------------------------------");
		};
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "Armor"
		});
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "WeaponRanged"
		});
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "WeaponMelee"
		});
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "BasicClothing"
		});
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "Clothing"
		});
		obj(new StockGenerator_MarketValue
		{
			tradeTag = "Art"
		});
		Log.Message(sb.ToString());
	}

	[DebugOutput("Incidents", false)]
	public static void TraderAnimalTags()
	{
		TableDataGetter<TraderKindDef>[] getters = (from tag in DefDatabase<TraderKindDef>.AllDefs.SelectMany((TraderKindDef k) => k.stockGenerators.Where((StockGenerator g) => g is StockGenerator_Animals).Cast<StockGenerator_Animals>().SelectMany((StockGenerator_Animals g) => g.AllRelevantTradeTags)).Distinct()
			select new TableDataGetter<TraderKindDef>(tag, (TraderKindDef def) => def.stockGenerators.Where((StockGenerator g) => g is StockGenerator_Animals).Cast<StockGenerator_Animals>().FirstOrDefault()?.GetInterestInTradeTag(tag) ?? "-")).Prepend(new TableDataGetter<TraderKindDef>("name", (TraderKindDef def) => def.defName)).ToArray();
		DebugTables.MakeTablesDialog(DefDatabase<TraderKindDef>.AllDefs, getters);
	}

	[DebugOutput("Incidents", false)]
	public static void PawnGroupGenSampled()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (allFaction.def.pawnGroupMakers == null || !allFaction.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat))
			{
				continue;
			}
			Faction localFac = allFaction;
			list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
				{
					float localP = item;
					float maxPawnCost = PawnGroupMakerUtility.MaxPawnCost(localFac, localP, null, PawnGroupKindDefOf.Combat);
					string defName = (from op in localFac.def.pawnGroupMakers.SelectMany((PawnGroupMaker gm) => gm.options)
						where op.Cost <= maxPawnCost
						select op).MaxBy((PawnGenOption op) => op.Cost).kind.defName;
					string label = localP + ", max " + maxPawnCost.ToString("F0") + " " + defName;
					list2.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
					{
						Dictionary<ThingDef, int>[] weaponsCount = new Dictionary<ThingDef, int>[20];
						string[] pawnKinds = new string[20];
						for (int i = 0; i < 20; i++)
						{
							weaponsCount[i] = new Dictionary<ThingDef, int>();
							List<Pawn> list3 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
							{
								groupKind = PawnGroupKindDefOf.Combat,
								tile = Find.CurrentMap.Tile,
								points = localP,
								faction = localFac
							}, warnOnZeroResults: false).ToList();
							pawnKinds[i] = PawnUtility.PawnKindsToCommaList(list3, useAnd: true);
							foreach (Pawn item2 in list3)
							{
								if (item2.equipment.Primary != null)
								{
									if (!weaponsCount[i].ContainsKey(item2.equipment.Primary.def))
									{
										weaponsCount[i].Add(item2.equipment.Primary.def, 0);
									}
									weaponsCount[i][item2.equipment.Primary.def]++;
								}
								item2.Destroy();
							}
						}
						int totalPawns = weaponsCount.Sum((Dictionary<ThingDef, int> x) => x.Sum((KeyValuePair<ThingDef, int> y) => y.Value));
						List<TableDataGetter<int>> list4 = new List<TableDataGetter<int>>();
						list4.Add(new TableDataGetter<int>("", (int x) => (x != 20) ? (x + 1).ToString() : "avg"));
						list4.Add(new TableDataGetter<int>("pawns", (int x) => " " + ((x == 20) ? ((float)totalPawns / 20f).ToString("0.#") : weaponsCount[x].Sum((KeyValuePair<ThingDef, int> y) => y.Value).ToString())));
						list4.Add(new TableDataGetter<int>("kinds", (int x) => (x != 20) ? pawnKinds[x] : ""));
						list4.AddRange(from x in DefDatabase<ThingDef>.AllDefs
							where x.IsWeapon && !x.weaponTags.NullOrEmpty() && weaponsCount.Any((Dictionary<ThingDef, int> wc) => wc.ContainsKey(x))
							orderby x.IsMeleeWeapon descending, x.techLevel, x.BaseMarketValue
							select new TableDataGetter<int>(x.label.Shorten(), delegate(int y)
							{
								if (y == 20)
								{
									return " " + ((float)weaponsCount.Sum((Dictionary<ThingDef, int> z) => z.ContainsKey(x) ? z[x] : 0) / 20f).ToString("0.#");
								}
								return (!weaponsCount[y].ContainsKey(x)) ? "" : (" " + weaponsCount[y][x] + " (" + ((float)weaponsCount[y][x] / (float)weaponsCount[y].Sum((KeyValuePair<ThingDef, int> z) => z.Value)).ToStringPercent("F0") + ")");
							}));
						DebugTables.MakeTablesDialog(Enumerable.Range(0, 21), list4.ToArray());
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Incidents", false)]
	public static void RaidFactionSampled()
	{
		((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidFactionSampled();
	}

	[DebugOutput("Incidents", false)]
	public static void RaidStrategySampled()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
		{
			((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(null);
		}));
		foreach (Faction f in Find.FactionManager.AllFactions)
		{
			Faction faction = f;
			list.Add(new FloatMenuOption(faction.Name + " (" + faction.def.defName + ")", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(f);
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Incidents", false)]
	public static void RaidArrivemodeSampled()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
		{
			((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(null);
		}));
		foreach (Faction f in Find.FactionManager.AllFactions)
		{
			Faction faction = f;
			list.Add(new FloatMenuOption(faction.Name + " (" + faction.def.defName + ")", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(f);
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Incidents", false)]
	public static void ThreatsGenerator()
	{
		StorytellerUtility.DebugLogTestFutureIncidents(new ThreatsGeneratorParams
		{
			allowedThreats = AllowedThreatsGeneratorThreats.All,
			randSeed = Rand.Int,
			onDays = 1f,
			offDays = 0.5f,
			minSpacingDays = 0.04f,
			numIncidentsRange = new FloatRange(1f, 2f)
		});
	}

	[DebugOutput("Incidents", false)]
	private static void RaidsInfoSampled()
	{
		if (Find.CurrentMap == null)
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (float item2 in DebugActionsUtility.PointsOptions(extended: true))
		{
			float localP = item2;
			list.Add(new DebugMenuOption(localP + " points", DebugMenuOptionMode.Action, delegate
			{
				int ticksGame = Find.TickManager.TicksGame;
				Find.TickManager.DebugSetTicksGame(36000000);
				Faction lastRaidFaction = Find.CurrentMap.StoryState.lastRaidFaction;
				List<Tuple<IncidentParms, List<Pawn>>> list2 = new List<Tuple<IncidentParms, List<Pawn>>>();
				for (int i = 0; i < 100; i++)
				{
					IncidentParms incidentParms = new IncidentParms
					{
						target = Find.CurrentMap,
						points = localP
					};
					if (((IncidentWorker_RaidEnemy)IncidentDefOf.RaidEnemy.Worker).TryGenerateRaidInfo(incidentParms, out var pawns, debugTest: true))
					{
						list2.Add(new Tuple<IncidentParms, List<Pawn>>(incidentParms, pawns));
					}
				}
				DebugTables.MakeTablesDialog(list2, new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("faction def", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.faction.def.defName), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("faction name", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.faction.Name), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("arrival mode", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.raidArrivalMode.defName), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("strategy", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.raidStrategy.defName), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("age restriction", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.raidAgeRestriction?.defName), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("points", (Tuple<IncidentParms, List<Pawn>> t) => t.Item1.points), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("pawn points\ntotal", (Tuple<IncidentParms, List<Pawn>> t) => t.Item2.Sum((Pawn x) => x.kindDef.combatPower)), new TableDataGetter<Tuple<IncidentParms, List<Pawn>>>("pawns", (Tuple<IncidentParms, List<Pawn>> t) => PawnUtility.PawnKindsToCommaList(t.Item2)));
				foreach (Tuple<IncidentParms, List<Pawn>> item3 in list2)
				{
					List<Pawn> item = item3.Item2;
					for (int num = 0; num < item.Count; num++)
					{
						item[num].DestroyOrPassToWorld();
					}
				}
				Find.TickManager.DebugSetTicksGame(ticksGame);
				Find.CurrentMap.storyState.lastRaidFaction = lastRaidFaction;
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}
}
