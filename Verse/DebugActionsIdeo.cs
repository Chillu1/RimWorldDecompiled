using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class DebugActionsIdeo
{
	[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void SetIdeo()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Ideo> ideosListForReading = Find.IdeoManager.IdeosListForReading;
		for (int i = 0; i < ideosListForReading.Count; i++)
		{
			Ideo ideo = ideosListForReading[i];
			list.Add(new DebugMenuOption(ideo.name, DebugMenuOptionMode.Tool, delegate
			{
				foreach (Pawn item in UI.MouseCell().GetThingList(Find.CurrentMap).OfType<Pawn>()
					.ToList())
				{
					if (!item.RaceProps.Humanlike)
					{
						break;
					}
					item.ideo.SetIdeo(ideo);
					DebugActionsUtility.DustPuffFrom(item);
				}
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, hideInSubMenu = true)]
	private static void ConvertToIdeo()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Ideo> ideosListForReading = Find.IdeoManager.IdeosListForReading;
		for (int i = 0; i < ideosListForReading.Count; i++)
		{
			Ideo ideo = ideosListForReading[i];
			list.Add(new DebugMenuOption(ideo.name, DebugMenuOptionMode.Tool, delegate
			{
				foreach (Pawn item in UI.MouseCell().GetThingList(Find.CurrentMap).OfType<Pawn>()
					.ToList())
				{
					if (!item.RaceProps.Humanlike || item.Ideo == ideo)
					{
						break;
					}
					item.ideo.IdeoConversionAttempt(1f, ideo, applyCertaintyFactor: false);
					DebugActionsUtility.DustPuffFrom(item);
				}
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Ideoligion", "Set ideo role...", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void SetIdeoRole(Pawn p)
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		if (p.Ideo != null)
		{
			Precept_Role currentRole = p.Ideo.GetRole(p);
			foreach (Precept_Role role in p.Ideo.cachedPossibleRoles)
			{
				if (role != currentRole)
				{
					list.Add(new DebugMenuOption(role.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						role.Assign(p, addThoughts: true);
					}));
				}
			}
			if (currentRole != null)
			{
				list.Add(new DebugMenuOption("None", DebugMenuOptionMode.Action, delegate
				{
					currentRole.Assign(null, addThoughts: true);
				}));
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Ideoligion", "Certainty - 20%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, requiresIdeology = true, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void OffsetCertaintyNegative20(Pawn p)
	{
		if (p.ideo != null)
		{
			p.ideo.Debug_ReduceCertainty(0.2f);
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	public static void SpawnRelic()
	{
		IntVec3 cell = UI.MouseCell();
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Precept> preceptsListForReading = Faction.OfPlayer.ideos.PrimaryIdeo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			Precept precept = preceptsListForReading[i];
			Precept_Relic relicPrecept = precept as Precept_Relic;
			if (relicPrecept != null)
			{
				list.Add(new DebugMenuOption(precept.ToString(), DebugMenuOptionMode.Action, delegate
				{
					GenSpawn.Spawn(relicPrecept.GenerateRelic(), cell, Find.CurrentMap);
				}));
			}
		}
		if (list.Any())
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	public static void SetSourcePrecept()
	{
		List<Thing> things = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
		if (things.NullOrEmpty())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		List<Precept> preceptsListForReading = Faction.OfPlayer.ideos.PrimaryIdeo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			Precept precept = preceptsListForReading[i];
			Precept_ThingStyle stylePrecept = precept as Precept_ThingStyle;
			if (stylePrecept == null)
			{
				continue;
			}
			list.Add(new DebugMenuOption(precept.ToString(), DebugMenuOptionMode.Action, delegate
			{
				foreach (Thing item in things)
				{
					item.StyleSourcePrecept = stylePrecept;
				}
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Ideoligion", "Remove ritual obligation", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void RemoveRitualObligation()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				Precept_Ritual ritual = item as Precept_Ritual;
				if (ritual == null || ritual.activeObligations.NullOrEmpty())
				{
					continue;
				}
				foreach (RitualObligation obligation in ritual.activeObligations)
				{
					string text = ritual.LabelCap;
					string text2 = ritual.obligationTargetFilter.LabelExtraPart(obligation);
					if (text2.NullOrEmpty())
					{
						text = text + " " + text2;
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						ritual.RemoveObligation(obligation);
					}));
				}
			}
		}
		if (list.Any())
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
		else
		{
			Messages.Message("No obligations to remove.", LookTargets.Invalid, MessageTypeDefOf.RejectInput, historical: false);
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = 1000)]
	private static List<DebugActionNode> AddPrecept()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (PreceptDef allDef in DefDatabase<PreceptDef>.AllDefs)
		{
			PreceptDef localDef = allDef;
			list.Add(new DebugActionNode(localDef.issue.LabelCap + ": " + localDef.LabelCap, DebugActionType.Action, delegate
			{
				Faction.OfPlayer.ideos.PrimaryIdeo.AddPrecept(PreceptMaker.MakePrecept(localDef), init: true);
			}));
		}
		return list;
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000, requiresIdeology = true)]
	private static void RemovePrecept()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Precept precept in Faction.OfPlayer.ideos.PrimaryIdeo.PreceptsListForReading)
		{
			list.Add(new DebugMenuOption(precept.def.issue.LabelCap + ": " + precept.def.LabelCap, DebugMenuOptionMode.Action, delegate
			{
				Faction.OfPlayer.ideos.PrimaryIdeo.RemovePrecept(precept);
			}));
		}
		foreach (Ideo ideo in Faction.OfPlayer.ideos.IdeosMinorListForReading)
		{
			foreach (Precept precept2 in ideo.PreceptsListForReading)
			{
				list.Add(new DebugMenuOption(precept2.def.issue.LabelCap + ": " + precept2.def.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					ideo.RemovePrecept(precept2);
				}));
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void TriggerDateRitual()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				Precept_Ritual ritual = item as Precept_Ritual;
				if (ritual != null && ritual.obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault() != null)
				{
					string text = ritual.LabelCap;
					if (!allIdeo.ObligationsActive && !item.def.allowOptionalRitualObligations)
					{
						text += "[NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						ritual.AddObligation(new RitualObligation(ritual));
					}));
				}
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Ideoligion", "Add 5 days to obligation timer", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void Add5DaysToObligationTimer()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				if (!(item is Precept_Ritual precept_Ritual) || precept_Ritual.activeObligations.NullOrEmpty())
				{
					continue;
				}
				foreach (RitualObligation obligation in precept_Ritual.activeObligations)
				{
					string text = precept_Ritual.LabelCap;
					string text2 = precept_Ritual.obligationTargetFilter.LabelExtraPart(obligation);
					if (text2.NullOrEmpty())
					{
						text = text + " " + text2;
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						obligation.DebugOffsetTriggeredTick(-300000);
					}));
				}
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Pawns", "Suppression +10%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = -1000)]
	private static void SuppressionPlus10(Pawn p)
	{
		if (p.guest != null && p.IsSlave)
		{
			p.needs.TryGetNeed(out Need_Suppression need);
			need.CurLevel += need.MaxLevel * 0.1f;
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Suppression -10%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = -1000)]
	private static void SuppressionMinus10(Pawn p)
	{
		if (p.guest != null && p.IsSlave)
		{
			p.needs.TryGetNeed(out Need_Suppression need);
			need.CurLevel -= need.MaxLevel * 0.1f;
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Clear suppression schedule", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, hideInSubMenu = true)]
	private static void ResetSuppresionSchedule(Pawn p)
	{
		if (p.guest != null && p.IsSlave)
		{
			p.mindState.lastSlaveSuppressedTick = -99999;
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Will +1", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
	private static void WillPlus1(Pawn p)
	{
		if (p.guest != null && p.IsPrisoner)
		{
			p.guest.will = Mathf.Max(p.guest.will += 1f, 0f);
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Will -1", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
	private static void WillMinus1(Pawn p)
	{
		if (p.guest != null && p.IsPrisoner)
		{
			p.guest.will = Mathf.Max(p.guest.will -= 1f, 0f);
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Start slave rebellion (random)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = -1000)]
	private static void StartSlaveRebellion(Pawn p)
	{
		if (SlaveRebellionUtility.CanParticipateInSlaveRebellion(p) && SlaveRebellionUtility.StartSlaveRebellion(p))
		{
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Start slave rebellion (aggressive)", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true, displayPriority = -1000)]
	private static void StartSlaveRebellionAggressive(Pawn p)
	{
		if (SlaveRebellionUtility.CanParticipateInSlaveRebellion(p) && SlaveRebellionUtility.StartSlaveRebellion(p, forceAggressive: true))
		{
			DebugActionsUtility.DustPuffFrom(p);
		}
	}

	[DebugAction("Pawns", "Change style", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void ChangeStyle(Pawn p)
	{
		if (p.RaceProps.Humanlike && p.story != null)
		{
			Find.WindowStack.Add(new Dialog_StylingStation(p, null));
		}
	}

	[DebugAction("Pawns", "View render tree", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void ViewRenderTree(Pawn p)
	{
		Find.WindowStack.Add(new Dialog_DebugRenderTree(p));
	}

	[DebugAction("Pawns", "Request style change", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true, requiresIdeology = true)]
	private static void RequestStyleChange(Pawn p)
	{
		if (p.style != null && p.style.CanDesireLookChange)
		{
			p.style.RequestLookChange();
		}
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -100)]
	private static void DarklightAtPosition()
	{
		Map currentMap = Find.CurrentMap;
		foreach (IntVec3 item in GenRadial.RadialCellsAround(UI.MouseCell(), 10f, useCenter: true))
		{
			if (item.InBounds(currentMap))
			{
				currentMap.debugDrawer.FlashCell(item, DarklightUtility.IsDarklightAt(item, currentMap) ? 0.5f : 0f, null, 100);
			}
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void MaxDevelopmentPoints()
	{
		if (Faction.OfPlayer.ideos.FluidIdeo != null)
		{
			Faction.OfPlayer.ideos.FluidIdeo.development.TryAddDevelopmentPoints(Faction.OfPlayer.ideos.FluidIdeo.development.NextReformationDevelopmentPoints);
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void AddDevelopmentPoint()
	{
		if (Faction.OfPlayer.ideos.FluidIdeo != null)
		{
			Faction.OfPlayer.ideos.FluidIdeo.development.TryAddDevelopmentPoints(1);
		}
	}

	[DebugAction("Ideoligion", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresIdeology = true)]
	private static void ClearDevelopmentPoints()
	{
		if (Faction.OfPlayer.ideos.FluidIdeo != null)
		{
			Faction.OfPlayer.ideos.FluidIdeo.development.ResetDevelopmentPoints();
		}
	}

	[DebugAction("Map", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> SpawnComplex()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (LayoutDef allDef in DefDatabase<LayoutDef>.AllDefs)
		{
			LayoutDef localDef = allDef;
			DebugActionNode debugActionNode = new DebugActionNode(localDef.defName);
			for (int i = 0; i < 10000; i += 100)
			{
				int localThreatPoints = i;
				debugActionNode.AddChild(new DebugActionNode(i + " threat points", DebugActionType.Action, delegate
				{
					DebugTool tool = null;
					IntVec3 firstCorner;
					tool = new DebugTool("first corner...", delegate
					{
						firstCorner = UI.MouseCell();
						DebugTools.curTool = new DebugTool("second corner...", delegate
						{
							IntVec3 second = UI.MouseCell();
							CellRect cellRect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.CurrentMap);
							StructureGenParams parms = new StructureGenParams
							{
								size = new IntVec2(cellRect.Width, cellRect.Height)
							};
							LayoutStructureSketch layoutStructureSketch = localDef.Worker.GenerateStructureSketch(parms);
							localDef.Worker.Spawn(layoutStructureSketch, Find.CurrentMap, cellRect.Min, localThreatPoints);
							DebugTools.curTool = tool;
						}, firstCorner);
					});
					DebugTools.curTool = tool;
				}));
			}
			list.Add(debugActionNode);
		}
		return list;
	}

	[DebugAction("Ideoligion", "Generate 200 ritual names", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true, requiresIdeology = true)]
	private static void Generate200RitualNames()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept item in allIdeo.PreceptsListForReading)
			{
				Precept_Ritual ritual = item as Precept_Ritual;
				if (ritual == null)
				{
					continue;
				}
				list.Add(new DebugMenuOption(ritual.def.issue.LabelCap + ": " + ritual.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 200; i++)
					{
						stringBuilder.AppendLine(ritual.GenerateNameRaw());
					}
					Log.Message(stringBuilder.ToString());
				}));
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}
}
