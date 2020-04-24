using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugActionsIncidents
	{
		[DebugActionYielder]
		private static IEnumerable<Dialog_DebugActionsMenu.DebugActionOption> IncidentsYielder()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				IIncidentTarget incidentTarget = WorldRendererUtility.WorldRenderedNow ? (Find.WorldSelector.SingleSelectedObject as IIncidentTarget) : null;
				if (incidentTarget == null)
				{
					incidentTarget = Find.CurrentMap;
				}
				if (incidentTarget != null)
				{
					yield return GetIncidentDebugAction(Find.CurrentMap);
					yield return GetIncidentWithPointsDebugAction(Find.CurrentMap);
				}
				if (WorldRendererUtility.WorldRenderedNow)
				{
					yield return GetIncidentDebugAction(Find.World);
					yield return GetIncidentWithPointsDebugAction(Find.World);
				}
			}
		}

		[DebugAction("Incidents", "Execute raid with points...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExecuteRaidWithPoints()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
			{
				float localP = item;
				list.Add(new FloatMenuOption(localP.ToString() + " points", delegate
				{
					IncidentParms parms = new IncidentParms
					{
						target = Find.CurrentMap,
						points = localP
					};
					IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugAction("Incidents", "Execute raid with faction...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExecuteRaidWithFaction()
		{
			StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
			IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				Faction localFac = allFaction;
				float localPoints = default(float);
				list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
				{
					parms.faction = localFac;
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
					{
						localPoints = item;
						list2.Add(new DebugMenuOption(item + " points", DebugMenuOptionMode.Action, delegate
						{
							parms.points = localPoints;
							List<RaidStrategyDef> source = DefDatabase<RaidStrategyDef>.AllDefs.Where((RaidStrategyDef s) => s.Worker.CanUseWith(parms, PawnGroupKindDefOf.Combat)).ToList();
							Log.Message("Available strategies: " + string.Join(", ", source.Select((RaidStrategyDef s) => s.defName).ToArray()));
							parms.raidStrategy = source.RandomElement();
							Log.Message("Strategy: " + parms.raidStrategy.defName);
							List<PawnsArrivalModeDef> source2 = DefDatabase<PawnsArrivalModeDef>.AllDefs.Where((PawnsArrivalModeDef a) => a.Worker.CanUseWith(parms) && parms.raidStrategy.arriveModes.Contains(a)).ToList();
							Log.Message("Available arrival modes: " + string.Join(", ", source2.Select((PawnsArrivalModeDef s) => s.defName).ToArray()));
							parms.raidArrivalMode = source2.RandomElement();
							Log.Message("Arrival mode: " + parms.raidArrivalMode.defName);
							DoRaid(parms);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugAction("Incidents", "Execute raid with specifics...", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ExecuteRaidWithSpecifics()
		{
			StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
			IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				Faction localFac = allFaction;
				float localPoints = default(float);
				RaidStrategyDef localStrat = default(RaidStrategyDef);
				PawnsArrivalModeDef localArrival = default(PawnsArrivalModeDef);
				list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
				{
					parms.faction = localFac;
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (float item in DebugActionsUtility.PointsOptions(extended: true))
					{
						localPoints = item;
						list2.Add(new DebugMenuOption(item + " points", DebugMenuOptionMode.Action, delegate
						{
							parms.points = localPoints;
							List<DebugMenuOption> list3 = new List<DebugMenuOption>();
							foreach (RaidStrategyDef allDef in DefDatabase<RaidStrategyDef>.AllDefs)
							{
								localStrat = allDef;
								string text = localStrat.defName;
								if (!localStrat.Worker.CanUseWith(parms, PawnGroupKindDefOf.Combat))
								{
									text += " [NO]";
								}
								list3.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
								{
									parms.raidStrategy = localStrat;
									List<DebugMenuOption> list4 = new List<DebugMenuOption>
									{
										new DebugMenuOption("-Random-", DebugMenuOptionMode.Action, delegate
										{
											DoRaid(parms);
										})
									};
									foreach (PawnsArrivalModeDef allDef2 in DefDatabase<PawnsArrivalModeDef>.AllDefs)
									{
										localArrival = allDef2;
										string text2 = localArrival.defName;
										if (!localArrival.Worker.CanUseWith(parms) || !localStrat.arriveModes.Contains(localArrival))
										{
											text2 += " [NO]";
										}
										list4.Add(new DebugMenuOption(text2, DebugMenuOptionMode.Action, delegate
										{
											parms.raidArrivalMode = localArrival;
											DoRaid(parms);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static string GetIncidentTargetLabel(IIncidentTarget target)
		{
			if (target == null)
			{
				return "null target";
			}
			if (target is Map)
			{
				return "Map";
			}
			if (target is World)
			{
				return "World";
			}
			if (target is Caravan)
			{
				return ((Caravan)target).LabelCap;
			}
			return target.ToString();
		}

		private static Dialog_DebugActionsMenu.DebugActionOption GetIncidentDebugAction(IIncidentTarget target)
		{
			Dialog_DebugActionsMenu.DebugActionOption result = default(Dialog_DebugActionsMenu.DebugActionOption);
			result.action = delegate
			{
				DoIncidentDebugAction(target);
			};
			result.actionType = DebugActionType.Action;
			result.category = "Incidents";
			result.label = "Do incident (" + GetIncidentTargetLabel(target) + ")...";
			return result;
		}

		private static void DoIncidentDebugAction(IIncidentTarget target)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (IncidentDef item in from d in DefDatabase<IncidentDef>.AllDefs
				where d.TargetAllowed(target)
				orderby d.defName
				select d)
			{
				IncidentDef localDef = item;
				string text = localDef.defName;
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(localDef.category, target);
				if (!localDef.Worker.CanFireNow(parms))
				{
					text += " [NO]";
				}
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
				{
					if (localDef.pointsScaleable)
					{
						StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
						parms = storytellerComp.GenerateParms(localDef.category, parms.target);
					}
					localDef.Worker.TryExecute(parms);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static Dialog_DebugActionsMenu.DebugActionOption GetIncidentWithPointsDebugAction(IIncidentTarget target)
		{
			Dialog_DebugActionsMenu.DebugActionOption result = default(Dialog_DebugActionsMenu.DebugActionOption);
			result.action = delegate
			{
				DoIncidentWithPointsAction(target);
			};
			result.actionType = DebugActionType.Action;
			result.category = "Incidents";
			result.label = "Do incident w/ points (" + GetIncidentTargetLabel(target) + ")...";
			return result;
		}

		private static void DoIncidentWithPointsAction(IIncidentTarget target)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (IncidentDef item in from d in DefDatabase<IncidentDef>.AllDefs
				where d.TargetAllowed(target) && d.pointsScaleable
				orderby d.defName
				select d)
			{
				IncidentDef localDef = item;
				string text = localDef.defName;
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(localDef.category, target);
				if (!localDef.Worker.CanFireNow(parms))
				{
					text += " [NO]";
				}
				float localPoints = default(float);
				list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (float item2 in DebugActionsUtility.PointsOptions(extended: true))
					{
						localPoints = item2;
						list2.Add(new DebugMenuOption(item2 + " points", DebugMenuOptionMode.Action, delegate
						{
							parms.points = localPoints;
							localDef.Worker.TryExecute(parms);
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static void DoRaid(IncidentParms parms)
		{
			IncidentDef incidentDef = (!parms.faction.HostileTo(Faction.OfPlayer)) ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;
			incidentDef.Worker.TryExecute(parms);
		}
	}
}
