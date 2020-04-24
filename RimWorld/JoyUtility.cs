using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class JoyUtility
	{
		private static List<JoyKindDef> tempKindList = new List<JoyKindDef>();

		private static List<JoyKindDef> listedJoyKinds = new List<JoyKindDef>();

		public static bool EnjoyableOutsideNow(Map map, StringBuilder outFailReason = null)
		{
			if (map.weatherManager.RainRate >= 0.25f)
			{
				outFailReason?.Append(map.weatherManager.curWeather.label);
				return false;
			}
			if (!map.gameConditionManager.AllowEnjoyableOutsideNow(map, out GameConditionDef reason))
			{
				outFailReason?.Append(reason.label);
				return false;
			}
			return true;
		}

		public static bool EnjoyableOutsideNow(Pawn pawn, StringBuilder outFailReason = null)
		{
			Map mapHeld = pawn.MapHeld;
			if (mapHeld == null)
			{
				return true;
			}
			if (!EnjoyableOutsideNow(mapHeld, outFailReason))
			{
				return false;
			}
			if (!pawn.ComfortableTemperatureRange().Includes(mapHeld.mapTemperature.OutdoorTemp))
			{
				outFailReason?.Append("NotEnjoyableOutsideTemperature".Translate());
				return false;
			}
			return true;
		}

		public static void JoyTickCheckEnd(Pawn pawn, JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f, Building joySource = null)
		{
			Job curJob = pawn.CurJob;
			if (curJob.def.joyKind == null)
			{
				Log.Warning("This method can only be called for jobs with joyKind.");
				return;
			}
			if (joySource != null)
			{
				if (joySource.def.building.joyKind != null && pawn.CurJob.def.joyKind != joySource.def.building.joyKind)
				{
					Log.ErrorOnce("Joy source joyKind and jobDef.joyKind are not the same. building=" + joySource.ToStringSafe() + ", jobDef=" + pawn.CurJob.def.ToStringSafe(), joySource.thingIDNumber ^ 0x343FD5CC);
				}
				extraJoyGainFactor *= joySource.GetStatValue(StatDefOf.JoyGainFactor);
			}
			if (pawn.needs.joy == null)
			{
				pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
				return;
			}
			pawn.needs.joy.GainJoy(extraJoyGainFactor * curJob.def.joyGainRate * 0.36f / 2500f, curJob.def.joyKind);
			if (curJob.def.joySkill != null)
			{
				pawn.skills.GetSkill(curJob.def.joySkill).Learn(curJob.def.joyXpPerTick);
			}
			if (!curJob.ignoreJoyTimeAssignment && !pawn.GetTimeAssignment().allowJoy)
			{
				pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
			}
			if (pawn.needs.joy.CurLevel > 0.9999f)
			{
				switch (fullJoyAction)
				{
				case JoyTickFullJoyAction.EndJob:
					pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
					break;
				case JoyTickFullJoyAction.GoToNextToil:
					pawn.jobs.curDriver.ReadyForNextToil();
					break;
				}
			}
		}

		public static void TryGainRecRoomThought(Pawn pawn)
		{
			Room room = pawn.GetRoom();
			if (room != null)
			{
				int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
				if (pawn.needs.mood != null && ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.JoyActivityInImpressiveRecRoom, scoreStageIndex));
				}
			}
		}

		public static bool LordPreventsGettingJoy(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
			{
				return true;
			}
			return false;
		}

		public static bool TimetablePreventsGettingJoy(Pawn pawn)
		{
			if (!((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment).allowJoy)
			{
				return true;
			}
			return false;
		}

		public static int JoyKindsOnMapCount(Map map)
		{
			List<JoyKindDef> list = JoyKindsOnMapTempList(map);
			int count = list.Count;
			list.Clear();
			return count;
		}

		public static List<JoyKindDef> JoyKindsOnMapTempList(Map map)
		{
			for (int i = 0; i < DefDatabase<JoyKindDef>.AllDefsListForReading.Count; i++)
			{
				JoyKindDef joyKindDef = DefDatabase<JoyKindDef>.AllDefsListForReading[i];
				if (!joyKindDef.needsThing)
				{
					tempKindList.Add(joyKindDef);
				}
			}
			foreach (Building item in map.listerBuildings.allBuildingsColonist)
			{
				if (item.def.building.joyKind != null && !tempKindList.Contains(item.def.building.joyKind))
				{
					tempKindList.Add(item.def.building.joyKind);
				}
			}
			foreach (Thing item2 in map.listerThings.ThingsInGroup(ThingRequestGroup.Drug))
			{
				if (item2.def.IsIngestible && item2.def.ingestible.joyKind != null && !tempKindList.Contains(item2.def.ingestible.joyKind) && !item2.Position.Fogged(map))
				{
					tempKindList.Add(item2.def.ingestible.joyKind);
				}
			}
			foreach (Thing item3 in map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree))
			{
				if (item3.def.IsIngestible && item3.def.ingestible.joyKind != null && !tempKindList.Contains(item3.def.ingestible.joyKind) && !item3.Position.Fogged(map))
				{
					tempKindList.Add(item3.def.ingestible.joyKind);
				}
			}
			return tempKindList;
		}

		public static string JoyKindsOnMapString(Map map)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < DefDatabase<JoyKindDef>.AllDefsListForReading.Count; i++)
			{
				JoyKindDef joyKindDef = DefDatabase<JoyKindDef>.AllDefsListForReading[i];
				if (!joyKindDef.needsThing)
				{
					CheckAppendJoyKind(stringBuilder, null, joyKindDef, map);
				}
			}
			foreach (Building item in map.listerBuildings.allBuildingsColonist)
			{
				if (item.def.building.joyKind != null)
				{
					CheckAppendJoyKind(stringBuilder, item, item.def.building.joyKind, map);
				}
			}
			foreach (Thing item2 in map.listerThings.ThingsInGroup(ThingRequestGroup.Drug))
			{
				if (item2.def.IsIngestible && item2.def.ingestible.joyKind != null)
				{
					CheckAppendJoyKind(stringBuilder, item2, item2.def.ingestible.joyKind, map);
				}
			}
			foreach (Thing item3 in map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree))
			{
				if (item3.def.IsIngestible && item3.def.ingestible.joyKind != null)
				{
					CheckAppendJoyKind(stringBuilder, item3, item3.def.ingestible.joyKind, map);
				}
			}
			listedJoyKinds.Clear();
			return stringBuilder.ToString().TrimEndNewlines();
		}

		private static void CheckAppendJoyKind(StringBuilder sb, Thing t, JoyKindDef kind, Map map)
		{
			if (listedJoyKinds.Contains(kind))
			{
				return;
			}
			if (t == null)
			{
				sb.AppendLine("   " + kind.LabelCap);
			}
			else
			{
				if (t.def.category == ThingCategory.Item && t.Position.Fogged(map))
				{
					return;
				}
				sb.AppendLine("   " + kind.LabelCap + " (" + t.def.label + ")");
			}
			listedJoyKinds.Add(kind);
		}

		public static string JoyKindsNotOnMapString(Map map)
		{
			List<JoyKindDef> allDefsListForReading = DefDatabase<JoyKindDef>.AllDefsListForReading;
			List<JoyKindDef> list = JoyKindsOnMapTempList(map);
			if (allDefsListForReading.Count == list.Count)
			{
				return "(" + "None".Translate() + ")";
			}
			string text = "";
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				JoyKindDef joyKindDef = allDefsListForReading[i];
				if (!list.Contains(joyKindDef))
				{
					text += "   " + joyKindDef.LabelCap + "\n";
				}
			}
			list.Clear();
			return text.TrimEndNewlines();
		}
	}
}
