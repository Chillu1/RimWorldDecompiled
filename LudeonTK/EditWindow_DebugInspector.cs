using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Steam;

namespace LudeonTK;

public class EditWindow_DebugInspector : EditWindow
{
	private const float CopyTextHeight = 20f;

	private StringBuilder debugStringBuilder = new StringBuilder();

	public bool fullMode;

	private float columnWidth = 360f;

	public override Vector2 InitialSize => new Vector2(400f, 600f);

	public override bool IsDebug => true;

	public EditWindow_DebugInspector()
	{
		optionalTitle = "Debug inspector";
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		if (Current.ProgramState == ProgramState.Playing)
		{
			GenUI.RenderMouseoverBracket();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (KeyBindingDefOf.Dev_ToggleDebugInspector.KeyDownEvent)
		{
			Event.current.Use();
			Close();
		}
		if (Input.GetMouseButtonDown(1))
		{
			GUIUtility.systemCopyBuffer = debugStringBuilder.ToString();
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
		}
		Text.Font = GameFont.Tiny;
		DevGUI.Label(new Rect(inRect.x, inRect.y, 200f, 20f), "Right click to copy to clipboard");
		Text.Font = GameFont.Small;
		inRect.yMin += 20f;
		float x = inRect.x;
		float y = inRect.y;
		DoImageToggle(ref x, y, DevGUI.InspectMode, "Toggle deep inspection mode for things on the map.", ref fullMode);
		DoImageToggle(ref x, y, DevGUI.InspectMode, "Toggle shallow inspection for things on the map.", ref DebugViewSettings.writeCellContents);
		DoRowButton(ref x, y, "Visbility", "Toggle what information should be reported by the inspector.", delegate
		{
			Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Settings));
		});
		DoRowButton(ref x, y, "Column Width +", "Make the columns wider.", delegate
		{
			columnWidth += 20f;
			columnWidth = Mathf.Clamp(columnWidth, 200f, 1600f);
		});
		DoRowButton(ref x, y, "Column Width -", "Make the columns narrower.", delegate
		{
			columnWidth -= 20f;
			columnWidth = Mathf.Clamp(columnWidth, 200f, 1600f);
		});
		inRect.yMin += 30f;
		float num = Mathf.Min(columnWidth, inRect.width);
		string[] array = debugStringBuilder.ToString().Split('\n');
		float num2 = inRect.y;
		float num3 = inRect.x;
		Text.Font = GameFont.Tiny;
		for (int num4 = 0; num4 < array.Length; num4++)
		{
			float num5 = Text.CalcHeight(array[num4], num);
			if (num2 + num5 > inRect.yMax)
			{
				num3 += num;
				num2 = inRect.y;
			}
			DevGUI.Label(new Rect(num3, num2, num, num5), array[num4]);
			num2 += num5 - 8f;
		}
		Text.Font = GameFont.Small;
		if (Event.current.type == EventType.Repaint)
		{
			debugStringBuilder = new StringBuilder();
			debugStringBuilder.Append(CurrentDebugString());
		}
	}

	public void AppendDebugString(string str)
	{
		debugStringBuilder.AppendLine(str);
	}

	private string CurrentDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (DebugViewSettings.writeGame)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine((Current.Game == null) ? "Current.Game = null" : Current.Game.DebugString());
		}
		if (DebugViewSettings.writeMusicManagerPlay)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine(Find.MusicManagerPlay.DebugString());
		}
		if (DebugViewSettings.writePlayingSounds)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine("Sustainers:");
			foreach (Sustainer allSustainer in Find.SoundRoot.sustainerManager.AllSustainers)
			{
				stringBuilder.AppendLine(allSustainer.DebugString());
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("OneShots:");
			foreach (SampleOneShot playingOneShot in Find.SoundRoot.oneShotManager.PlayingOneShots)
			{
				stringBuilder.AppendLine(playingOneShot.ToString());
			}
		}
		if (DebugViewSettings.writeSoundEventsRecord)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine("Recent sound events:\n       ...");
			stringBuilder.AppendLine(DebugSoundEventsLog.EventsListingDebugString);
		}
		if (DebugViewSettings.writeSteamItems)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine(WorkshopItems.DebugOutput());
		}
		if (DebugViewSettings.writeConcepts)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine(LessonAutoActivator.DebugString());
		}
		if (DebugViewSettings.writeReservations && Find.CurrentMap != null)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine(string.Join("\r\n", Find.CurrentMap.reservationManager.ReservationsReadOnly.Select((ReservationManager.Reservation r) => r.ToString()).ToArray()));
		}
		if (DebugViewSettings.writeMemoryUsage)
		{
			stringBuilder.AppendLine("---");
			stringBuilder.AppendLine("Total allocated: " + Profiler.GetTotalAllocatedMemoryLong().ToStringBytes());
			stringBuilder.AppendLine("Total reserved: " + Profiler.GetTotalReservedMemoryLong().ToStringBytes());
			stringBuilder.AppendLine("Total reserved unused: " + Profiler.GetTotalUnusedReservedMemoryLong().ToStringBytes());
			stringBuilder.AppendLine("Mono heap size: " + Profiler.GetMonoHeapSizeLong().ToStringBytes());
			stringBuilder.AppendLine("Mono used size: " + Profiler.GetMonoUsedSizeLong().ToStringBytes());
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			stringBuilder.AppendLine("Tick " + Find.TickManager.TicksGame);
			if (DebugViewSettings.writeStoryteller)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.Storyteller.DebugString());
			}
		}
		if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null)
		{
			if (DebugViewSettings.writeMapGameConditions)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.gameConditionManager.DebugString());
			}
			if (DebugViewSettings.drawPawnDebug)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.reservationManager.DebugString());
			}
			if (DebugViewSettings.writeMoteSaturation)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Mote count: " + Find.CurrentMap.moteCounter.MoteCount);
				stringBuilder.AppendLine("Mote saturation: " + Find.CurrentMap.moteCounter.Saturation);
			}
			if (DebugViewSettings.writeEcosystem)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.wildAnimalSpawner.DebugString());
			}
			if (DebugViewSettings.writeTotalSnowDepth)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine("Total snow depth: " + Find.CurrentMap.snowGrid.TotalDepth);
			}
			if (DebugViewSettings.writeWind)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.windManager.DebugString());
			}
			if (DebugViewSettings.writeRecentStrikes)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.mineStrikeManager.DebugStrikeRecords());
			}
			if (DebugViewSettings.writeListRepairableBldgs)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.listerBuildingsRepairable.DebugString());
			}
			if (DebugViewSettings.writeListFilthInHomeArea)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.listerFilthInHomeArea.DebugString());
			}
			if (DebugViewSettings.writeListHaulables)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.listerHaulables.DebugString());
			}
			if (DebugViewSettings.writeListMergeables)
			{
				stringBuilder.AppendLine("---");
				stringBuilder.AppendLine(Find.CurrentMap.listerMergeables.DebugString());
			}
			if (DebugViewSettings.drawLords)
			{
				foreach (Lord lord in Find.CurrentMap.lordManager.lords)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(lord.DebugString());
				}
			}
			IntVec3 intVec = UI.MouseCell();
			if (intVec.InBounds(Find.CurrentMap))
			{
				stringBuilder.AppendLine($"Inspecting {intVec} (index: {Find.CurrentMap.cellIndices[intVec]})");
				if (DebugViewSettings.writeTerrain)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.terrainGrid.DebugStringAt(intVec));
				}
				if (DebugViewSettings.writeAttackTargets)
				{
					foreach (Pawn item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>())
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine("Potential attack targets for " + item.LabelShort + ":");
						List<IAttackTarget> potentialTargetsFor = Find.CurrentMap.attackTargetsCache.GetPotentialTargetsFor(item);
						for (int num = 0; num < potentialTargetsFor.Count; num++)
						{
							Thing thing = (Thing)potentialTargetsFor[num];
							stringBuilder.AppendLine(thing.LabelShort + ", " + thing.Faction?.ToString() + (potentialTargetsFor[num].ThreatDisabled(null) ? " (threat disabled)" : ""));
						}
					}
				}
				if (DebugViewSettings.writeSnowDepth)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Snow depth: " + Find.CurrentMap.snowGrid.GetDepth(intVec));
				}
				if (DebugViewSettings.writeSandDepth)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Sand depth: " + Find.CurrentMap.sandGrid.GetDepth(intVec));
				}
				if (DebugViewSettings.drawDeepResources)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Deep resource def: " + Find.CurrentMap.deepResourceGrid.ThingDefAt(intVec));
					stringBuilder.AppendLine("Deep resource count: " + Find.CurrentMap.deepResourceGrid.CountAt(intVec));
				}
				if (DebugViewSettings.writeCanReachColony)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("CanReachColony: " + Find.CurrentMap.reachability.CanReachColony(UI.MouseCell()));
				}
				if (DebugViewSettings.writeMentalStateCalcs)
				{
					stringBuilder.AppendLine("---");
					foreach (Pawn item2 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						stringBuilder.AppendLine(item2.mindState.mentalBreaker.DebugString());
					}
				}
				if (DebugViewSettings.writeWorkSettings)
				{
					foreach (Pawn item3 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
					{
						if (item3.workSettings != null)
						{
							stringBuilder.AppendLine("---");
							stringBuilder.AppendLine(item3.workSettings.DebugString());
						}
					}
				}
				if (DebugViewSettings.writeApparelScore)
				{
					stringBuilder.AppendLine("---");
					if (intVec.InBounds(Find.CurrentMap))
					{
						foreach (Thing thing3 in intVec.GetThingList(Find.CurrentMap))
						{
							if (thing3 is Apparel apparel)
							{
								stringBuilder.AppendLine(apparel.Label + ": " + JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel).ToString("F2"));
							}
						}
					}
				}
				if (DebugViewSettings.writeCellContents || fullMode)
				{
					stringBuilder.AppendLine("---");
					if (intVec.InBounds(Find.CurrentMap))
					{
						foreach (Designation item4 in Find.CurrentMap.designationManager.AllDesignationsAt(intVec))
						{
							stringBuilder.AppendLine(item4.ToString());
						}
						foreach (Thing item5 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
						{
							if (!fullMode)
							{
								stringBuilder.AppendLine(item5.LabelCap + " - " + item5.ToString());
								continue;
							}
							stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(item5));
							stringBuilder.AppendLine();
						}
					}
				}
				if (DebugViewSettings.debugApparelOptimize)
				{
					stringBuilder.AppendLine("---");
					foreach (Thing item6 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
					{
						if (!(item6 is Apparel apparel2))
						{
							continue;
						}
						stringBuilder.AppendLine(apparel2.LabelCap);
						stringBuilder.AppendLine("   raw: " + JobGiver_OptimizeApparel.ApparelScoreRaw(null, apparel2).ToString("F2"));
						if (Find.Selector.SingleSelectedThing is Pawn pawn)
						{
							List<float> list = new List<float>();
							for (int num2 = 0; num2 < pawn.apparel.WornApparel.Count; num2++)
							{
								list.Add(JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, pawn.apparel.WornApparel[num2]));
							}
							stringBuilder.AppendLine("  Pawn: " + pawn);
							stringBuilder.AppendLine("  gain: " + JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel2, list).ToString("F2"));
						}
					}
				}
				if (DebugViewSettings.drawRegions)
				{
					stringBuilder.AppendLine("---");
					Region regionAt_NoRebuild_InvalidAllowed = Find.CurrentMap.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(intVec);
					stringBuilder.AppendLine("Region:\n" + ((regionAt_NoRebuild_InvalidAllowed != null) ? regionAt_NoRebuild_InvalidAllowed.DebugString : "null"));
				}
				if (DebugViewSettings.drawDistricts)
				{
					stringBuilder.AppendLine("---");
					District district = intVec.GetDistrict(Find.CurrentMap);
					if (district != null)
					{
						stringBuilder.AppendLine(district.DebugString());
					}
					else
					{
						stringBuilder.AppendLine("(no district)");
					}
				}
				if (DebugViewSettings.drawRooms)
				{
					stringBuilder.AppendLine("---");
					Room room = intVec.GetRoom(Find.CurrentMap);
					if (room != null)
					{
						stringBuilder.AppendLine(room.DebugString());
					}
					else
					{
						stringBuilder.AppendLine("(no room)");
					}
				}
				if (DebugViewSettings.drawGlow)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("Game glow: " + Find.CurrentMap.glowGrid.GroundGlowAt(intVec));
					stringBuilder.AppendLine("Psych glow: " + Find.CurrentMap.glowGrid.PsychGlowAt(intVec));
					stringBuilder.AppendLine("Visual Glow: " + Find.CurrentMap.glowGrid.VisualGlowAt(intVec).ToString());
					stringBuilder.AppendLine("GlowReport:\n" + ((SectionLayer_LightingOverlay)Find.CurrentMap.mapDrawer.SectionAt(intVec).GetLayer(typeof(SectionLayer_LightingOverlay))).GlowReportAt(intVec));
					stringBuilder.AppendLine("SkyManager.CurSkyGlow: " + Find.CurrentMap.skyManager.CurSkyGlow);
				}
				if (DebugViewSettings.writePathCosts)
				{
					stringBuilder.AppendLine("---");
					int num3 = Find.CurrentMap.pathing.Normal.pathGrid.Cost(intVec);
					int num4 = Find.CurrentMap.pathing.Normal.pathGrid.CalculatedCostAt(intVec, perceivedStatic: false, IntVec3.Invalid);
					stringBuilder.AppendLine("Perceived path cost: " + num3);
					stringBuilder.AppendLine("Real path cost: " + num4);
					int num5 = Find.CurrentMap.pathing.FenceBlocked.pathGrid.Cost(intVec);
					int num6 = Find.CurrentMap.pathing.FenceBlocked.pathGrid.CalculatedCostAt(intVec, perceivedStatic: false, IntVec3.Invalid);
					if (num5 != num3 || num6 != num4)
					{
						stringBuilder.AppendLine("Perceived path cost (for fenceblocked): " + num5);
						stringBuilder.AppendLine("Real path cost (for fenceblocked): " + num6);
					}
				}
				if (DebugViewSettings.writeFertility)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("\nFertility: " + Find.CurrentMap.fertilityGrid.FertilityAt(intVec).ToString("##0.00"));
				}
				if (DebugViewSettings.writeLinkFlags)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine("\nLinkFlags: ");
					foreach (object value in Enum.GetValues(typeof(LinkFlags)))
					{
						if ((Find.CurrentMap.linkGrid.LinkFlagsAt(intVec) & (LinkFlags)value) != LinkFlags.None)
						{
							stringBuilder.Append(" " + value);
						}
					}
				}
				if (DebugViewSettings.writeSkyManager)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.AppendLine(Find.CurrentMap.skyManager.DebugString());
				}
				if (DebugViewSettings.writeCover)
				{
					stringBuilder.AppendLine("---");
					stringBuilder.Append("Cover: ");
					Thing thing2 = Find.CurrentMap.coverGrid[intVec];
					if (thing2 == null)
					{
						stringBuilder.AppendLine("null");
					}
					else
					{
						stringBuilder.AppendLine(thing2.ToString());
					}
				}
				if (DebugViewSettings.drawPower)
				{
					stringBuilder.AppendLine("---");
					foreach (Thing item7 in Find.CurrentMap.thingGrid.ThingsAt(intVec))
					{
						if (item7 is ThingWithComps thingWithComps && thingWithComps.GetComp<CompPowerTrader>() != null)
						{
							stringBuilder.AppendLine(" " + thingWithComps.GetComp<CompPowerTrader>().DebugString);
						}
					}
					PowerNet powerNet = Find.CurrentMap.powerNetGrid.TransmittedPowerNetAt(intVec);
					if (powerNet != null)
					{
						stringBuilder.AppendLine(powerNet.DebugString() ?? "");
					}
					else
					{
						stringBuilder.AppendLine("(no PowerNet here)");
					}
				}
				if (DebugViewSettings.drawPreyInfo && Find.Selector.SingleSelectedThing is Pawn predator)
				{
					List<Thing> thingList = intVec.GetThingList(Find.CurrentMap);
					for (int num7 = 0; num7 < thingList.Count; num7++)
					{
						if (thingList[num7] is Pawn prey)
						{
							stringBuilder.AppendLine("---");
							if (FoodUtility.IsAcceptablePreyFor(predator, prey))
							{
								stringBuilder.AppendLine("Prey score: " + FoodUtility.GetPreyScoreFor(predator, prey));
							}
							else
							{
								stringBuilder.AppendLine("Prey score: None");
							}
							break;
						}
					}
				}
				if (DebugViewSettings.writeRopesAndPens)
				{
					CompAnimalPenMarker compAnimalPenMarker = intVec.GetEdifice(Find.CurrentMap)?.TryGetComp<CompAnimalPenMarker>();
					if (compAnimalPenMarker != null)
					{
						stringBuilder.AppendLine("---");
						stringBuilder.AppendLine($"Pen marker {compAnimalPenMarker.parent} - {compAnimalPenMarker.label}");
						if (compAnimalPenMarker.PenState.Enclosed)
						{
							District district2 = compAnimalPenMarker.parent.GetDistrict();
							float num8 = new AnimalPenBalanceCalculator(Find.CurrentMap, considerInProgressMovement: false).TotalBodySizeIn(district2) / (float)district2.CellCount;
							float num9 = new AnimalPenBalanceCalculator(Find.CurrentMap, considerInProgressMovement: true).TotalBodySizeIn(district2) / (float)district2.CellCount;
							stringBuilder.AppendLine($" animal density: {num8} (upcoming: {num9})");
						}
					}
					foreach (Pawn item8 in intVec.GetThingList(Find.CurrentMap).OfType<Pawn>())
					{
						if (item8.roping != null && item8.roping.HasAnyRope)
						{
							stringBuilder.AppendLine("---");
							stringBuilder.Append(item8.roping.DebugString());
						}
					}
				}
			}
		}
		return stringBuilder.ToString();
	}
}
