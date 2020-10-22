using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class MeditationUtility
	{
		public static float FocusObjectSearchRadius = 3.9f;

		private static float WanderRadius = 10f;

		public static readonly Color ArtificialBuildingRingColor = new Color(0.8f, 0.49f, 0.43f);

		private static Dictionary<MeditationFocusDef, List<Dialog_InfoCard.Hyperlink>> focusObjectHyperlinksPerTypeCache = new Dictionary<MeditationFocusDef, List<Dialog_InfoCard.Hyperlink>>();

		private static Dictionary<MeditationFocusDef, string> focusObjectsPerTypeCache = new Dictionary<MeditationFocusDef, string>();

		public static Job GetMeditationJob(Pawn pawn, bool forJoy = false)
		{
			MeditationSpotAndFocus meditationSpotAndFocus = FindMeditationSpot(pawn);
			if (meditationSpotAndFocus.IsValid)
			{
				Building_Throne t;
				Job job = (((t = meditationSpotAndFocus.focus.Thing as Building_Throne) == null) ? JobMaker.MakeJob(JobDefOf.Meditate, meditationSpotAndFocus.spot, null, meditationSpotAndFocus.focus) : JobMaker.MakeJob(JobDefOf.Reign, t, null, t));
				job.ignoreJoyTimeAssignment = !forJoy;
				return job;
			}
			return null;
		}

		public static MeditationSpotAndFocus FindMeditationSpot(Pawn pawn)
		{
			float num = float.MinValue;
			LocalTargetInfo spot = LocalTargetInfo.Invalid;
			LocalTargetInfo focus = LocalTargetInfo.Invalid;
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Psyfocus meditation is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657324);
				return new MeditationSpotAndFocus(spot, focus);
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			foreach (LocalTargetInfo item in AllMeditationSpotCandidates(pawn))
			{
				if (SafeEnvironmentalConditions(pawn, item.Cell, pawn.Map))
				{
					LocalTargetInfo localTargetInfo = ((item.Thing is Building_Throne) ? ((LocalTargetInfo)item.Thing) : BestFocusAt(item, pawn));
					float num2 = 1f / Mathf.Max(item.Cell.DistanceToSquared(pawn.Position), 0.1f);
					if (pawn.HasPsylink && localTargetInfo.IsValid)
					{
						num2 += localTargetInfo.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) * 100f;
					}
					Room room = item.Cell.GetRoom(pawn.Map);
					if (room != null && ownedRoom == room)
					{
						num2 += 1f;
					}
					Building building;
					if (item.Thing != null && (building = item.Thing as Building) != null && building.GetAssignedPawn() == pawn)
					{
						num2 += (float)((building.def == ThingDefOf.MeditationSpot) ? 200 : 100);
					}
					if (!item.Cell.Standable(pawn.Map))
					{
						num2 = float.NegativeInfinity;
					}
					if (num2 > num)
					{
						spot = item;
						focus = localTargetInfo;
						num = num2;
					}
				}
			}
			return new MeditationSpotAndFocus(spot, focus);
		}

		public static IEnumerable<LocalTargetInfo> AllMeditationSpotCandidates(Pawn pawn, bool allowFallbackSpots = true)
		{
			bool flag = false;
			if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0 && !pawn.IsPrisonerOfColony)
			{
				Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone(pawn);
				if (building_Throne != null)
				{
					yield return building_Throne;
					flag = true;
				}
			}
			if (!pawn.IsPrisonerOfColony)
			{
				foreach (Building item in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot).Where(delegate(Building s)
				{
					if (s.IsForbidden(pawn) || !s.Position.Standable(s.Map))
					{
						return false;
					}
					if (s.GetAssignedPawn() != null && s.GetAssignedPawn() != pawn)
					{
						return false;
					}
					Room room4 = s.GetRoom();
					return (room4 == null || CanUseRoomToMeditate(room4, pawn)) && pawn.CanReserveAndReach(s, PathEndMode.OnCell, pawn.NormalMaxDanger());
				}))
				{
					yield return item;
					flag = true;
				}
			}
			if (flag || !allowFallbackSpots)
			{
				yield break;
			}
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.MeditationFocus);
			foreach (Thing item2 in list)
			{
				if (item2.def == ThingDefOf.Wall)
				{
					continue;
				}
				Room room = item2.GetRoom();
				if ((room == null || CanUseRoomToMeditate(room, pawn)) && item2.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) > 0f)
				{
					LocalTargetInfo localTargetInfo = MeditationSpotForFocus(item2, pawn);
					if (localTargetInfo.IsValid)
					{
						yield return localTargetInfo;
					}
				}
			}
			Building_Bed bed = pawn.ownership.OwnedBed;
			Room room2 = bed?.GetRoom();
			IntVec3 c2;
			if (room2 != null && !room2.PsychologicallyOutdoors && pawn.CanReserveAndReach(bed, PathEndMode.OnCell, pawn.NormalMaxDanger()))
			{
				foreach (LocalTargetInfo item3 in FocusSpotsInTheRoom(pawn, room2))
				{
					yield return item3;
				}
				c2 = RCellFinder.RandomWanderDestFor(pawn, bed.Position, WanderRadius, (Pawn p, IntVec3 c, IntVec3 r) => c.Standable(p.Map) && c.GetDoor(p.Map) == null && WanderRoomUtility.IsValidWanderDest(p, c, r), pawn.NormalMaxDanger());
				if (c2.IsValid)
				{
					yield return c2;
				}
			}
			if (pawn.IsPrisonerOfColony)
			{
				yield break;
			}
			IntVec3 colonyWanderRoot = WanderUtility.GetColonyWanderRoot(pawn);
			c2 = RCellFinder.RandomWanderDestFor(pawn, colonyWanderRoot, WanderRadius, delegate(Pawn p, IntVec3 c, IntVec3 r)
			{
				if (!c.Standable(p.Map) || c.GetDoor(p.Map) != null || !p.CanReserveAndReach(c, PathEndMode.OnCell, p.NormalMaxDanger()))
				{
					return false;
				}
				Room room3 = c.GetRoom(p.Map);
				return (room3 == null || CanUseRoomToMeditate(room3, pawn)) ? true : false;
			}, pawn.NormalMaxDanger());
			if (c2.IsValid)
			{
				yield return c2;
			}
		}

		public static bool SafeEnvironmentalConditions(Pawn pawn, IntVec3 cell, Map map)
		{
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && !cell.Roofed(map))
			{
				return false;
			}
			if (cell.GetDangerFor(pawn, map) != Danger.None)
			{
				return false;
			}
			return true;
		}

		public static bool CanMeditateNow(Pawn pawn)
		{
			if (pawn.needs.rest != null && (int)pawn.needs.rest.CurCategory >= 2)
			{
				return false;
			}
			if (pawn.needs.food.Starving)
			{
				return false;
			}
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0f || (HealthAIUtility.ShouldSeekMedicalRest(pawn) && pawn.timetable?.CurrentAssignment != TimeAssignmentDefOf.Meditate) || HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
			{
				return false;
			}
			return true;
		}

		public static bool CanOnlyMeditateInBed(Pawn pawn)
		{
			return pawn.Downed;
		}

		public static bool ShouldMeditateInBed(Pawn pawn)
		{
			if (pawn.Downed)
			{
				return true;
			}
			if (pawn.health.hediffSet.AnyHediffMakesSickThought)
			{
				return true;
			}
			return false;
		}

		public static LocalTargetInfo BestFocusAt(LocalTargetInfo spot, Pawn pawn)
		{
			float num = 0f;
			LocalTargetInfo result = LocalTargetInfo.Invalid;
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(spot.Cell, pawn.MapHeld, FocusObjectSearchRadius, useCenter: false))
			{
				if (!GenSight.LineOfSightToThing(spot.Cell, item, pawn.Map) || item is Building_Throne)
				{
					continue;
				}
				CompMeditationFocus compMeditationFocus = item.TryGetComp<CompMeditationFocus>();
				if (compMeditationFocus != null && compMeditationFocus.CanPawnUse(pawn))
				{
					float statValueForPawn = item.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn);
					if (statValueForPawn > num)
					{
						result = item;
						num = statValueForPawn;
					}
				}
			}
			return result;
		}

		public static IEnumerable<LocalTargetInfo> FocusSpotsInTheRoom(Pawn pawn, Room r)
		{
			foreach (Thing containedAndAdjacentThing in r.ContainedAndAdjacentThings)
			{
				CompMeditationFocus compMeditationFocus = containedAndAdjacentThing.TryGetComp<CompMeditationFocus>();
				if (compMeditationFocus != null && compMeditationFocus.CanPawnUse(pawn) && !(containedAndAdjacentThing is Building_Throne) && containedAndAdjacentThing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) > 0f)
				{
					LocalTargetInfo localTargetInfo = MeditationSpotForFocus(containedAndAdjacentThing, pawn, r.ContainsCell);
					if (localTargetInfo.IsValid)
					{
						yield return localTargetInfo;
					}
				}
			}
		}

		public static LocalTargetInfo MeditationSpotForFocus(Thing t, Pawn p, Func<IntVec3, bool> validator = null)
		{
			return t.OccupiedRect().ExpandedBy(2).AdjacentCellsCardinal.Where((IntVec3 cell) => (validator == null || validator(cell)) && !cell.IsForbidden(p) && p.CanReserveAndReach(cell, PathEndMode.OnCell, p.NormalMaxDanger()) && cell.Standable(p.Map)).RandomElementWithFallback(IntVec3.Invalid);
		}

		public static IEnumerable<MeditationFocusDef> FocusTypesAvailableForPawn(Pawn pawn)
		{
			for (int i = 0; i < DefDatabase<MeditationFocusDef>.AllDefsListForReading.Count; i++)
			{
				MeditationFocusDef meditationFocusDef = DefDatabase<MeditationFocusDef>.AllDefsListForReading[i];
				if (meditationFocusDef.CanPawnUse(pawn))
				{
					yield return meditationFocusDef;
				}
			}
		}

		public static string FocusTypesAvailableForPawnString(Pawn pawn)
		{
			return (from f in FocusTypesAvailableForPawn(pawn)
				select f.label).ToCommaList();
		}

		public static IEnumerable<Dialog_InfoCard.Hyperlink> FocusObjectsForPawnHyperlinks(Pawn pawn)
		{
			for (int i = 0; i < DefDatabase<MeditationFocusDef>.AllDefsListForReading.Count; i++)
			{
				MeditationFocusDef meditationFocusDef = DefDatabase<MeditationFocusDef>.AllDefsListForReading[i];
				if (!meditationFocusDef.CanPawnUse(pawn))
				{
					continue;
				}
				if (!focusObjectHyperlinksPerTypeCache.ContainsKey(meditationFocusDef))
				{
					List<Dialog_InfoCard.Hyperlink> list2 = new List<Dialog_InfoCard.Hyperlink>();
					foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
					{
						CompProperties_MeditationFocus compProperties = item.GetCompProperties<CompProperties_MeditationFocus>();
						if (compProperties != null && compProperties.focusTypes.Contains(meditationFocusDef))
						{
							list2.Add(new Dialog_InfoCard.Hyperlink(item));
						}
					}
					focusObjectHyperlinksPerTypeCache[meditationFocusDef] = list2;
				}
				List<Dialog_InfoCard.Hyperlink> list = focusObjectHyperlinksPerTypeCache[meditationFocusDef];
				for (int j = 0; j < list.Count; j++)
				{
					yield return list[j];
				}
			}
		}

		public static string FocusTypeAvailableExplanation(Pawn pawn)
		{
			string text = "";
			for (int i = 0; i < DefDatabase<MeditationFocusDef>.AllDefsListForReading.Count; i++)
			{
				MeditationFocusDef meditationFocusDef = DefDatabase<MeditationFocusDef>.AllDefsListForReading[i];
				if (!meditationFocusDef.CanPawnUse(pawn))
				{
					continue;
				}
				text = text + "MeditationFocusCanUse".Translate(meditationFocusDef.label).RawText + ":\n" + meditationFocusDef.EnablingThingsExplanation(pawn) + "\n\n";
				if (!focusObjectsPerTypeCache.ContainsKey(meditationFocusDef))
				{
					List<string> list = new List<string>();
					foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
					{
						CompProperties_MeditationFocus compProperties = item.GetCompProperties<CompProperties_MeditationFocus>();
						if (compProperties != null && compProperties.focusTypes.Contains(meditationFocusDef))
						{
							list.AddDistinct(item.label);
						}
					}
					focusObjectsPerTypeCache[meditationFocusDef] = list.ToLineList("  - ", capitalizeItems: true);
				}
				text += "MeditationFocusObjects".Translate(meditationFocusDef.label).CapitalizeFirst() + ":\n" + focusObjectsPerTypeCache[meditationFocusDef] + "\n\n";
			}
			return text;
		}

		public static void DrawMeditationSpotOverlay(IntVec3 center, Map map)
		{
			GenDraw.DrawRadiusRing(center, FocusObjectSearchRadius);
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(center, map, FocusObjectSearchRadius, useCenter: false))
			{
				if (!(item is Building_Throne) && item.def != ThingDefOf.Wall && item.TryGetComp<CompMeditationFocus>() != null && GenSight.LineOfSight(center, item.Position, map))
				{
					GenDraw.DrawLineBetween(center.ToVector3() + new Vector3(0.5f, 0f, 0.5f), item.TrueCenter(), SimpleColor.White);
				}
			}
		}

		public static bool CanUseRoomToMeditate(Room r, Pawn p)
		{
			if (!r.Owners.EnumerableNullOrEmpty() && !r.Owners.Contains(p))
			{
				return false;
			}
			if (r.isPrisonCell && !p.IsPrisoner)
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<Thing> GetMeditationFociAffectedByBuilding(Map map, ThingDef def, Faction faction, IntVec3 pos, Rot4 rotation)
		{
			if (!CountsAsArtificialBuilding(def, faction))
			{
				yield break;
			}
			foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.MeditationFocus)))
			{
				CompMeditationFocus compMeditationFocus = item.TryGetComp<CompMeditationFocus>();
				if (compMeditationFocus != null && compMeditationFocus.WillBeAffectedBy(def, faction, pos, rotation))
				{
					yield return item;
				}
			}
		}

		public static void DrawMeditationFociAffectedByBuildingOverlay(Map map, ThingDef def, Faction faction, IntVec3 pos, Rot4 rotation)
		{
			int num = 0;
			foreach (Thing item in GetMeditationFociAffectedByBuilding(map, def, faction, pos, rotation))
			{
				if (num++ > 10)
				{
					break;
				}
				CompToggleDrawAffectedMeditationFoci compToggleDrawAffectedMeditationFoci = item.TryGetComp<CompToggleDrawAffectedMeditationFoci>();
				if (compToggleDrawAffectedMeditationFoci == null || compToggleDrawAffectedMeditationFoci.Enabled)
				{
					GenAdj.OccupiedRect(pos, rotation, def.size);
					GenDraw.DrawLineBetween(GenThing.TrueCenter(pos, rotation, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Red);
				}
			}
		}

		public static bool CountsAsArtificialBuilding(ThingDef def, Faction faction)
		{
			if (def.category == ThingCategory.Building && faction != null)
			{
				return def.building.artificialForMeditationPurposes;
			}
			return false;
		}

		public static bool CountsAsArtificialBuilding(Thing t)
		{
			return CountsAsArtificialBuilding(t.def, t.Faction);
		}

		public static void DrawArtificialBuildingOverlay(IntVec3 pos, ThingDef def, Map map, float radius)
		{
			GenDraw.DrawRadiusRing(pos, radius, ArtificialBuildingRingColor);
			int num = 0;
			foreach (Thing item in map.listerArtificialBuildingsForMeditation.GetForCell(pos, radius))
			{
				if (num++ > 10)
				{
					break;
				}
				GenDraw.DrawLineBetween(GenThing.TrueCenter(pos, Rot4.North, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Red);
			}
		}

		public static float PsyfocusGainPerTick(Pawn pawn, Thing focus = null)
		{
			float num = pawn.GetStatValue(StatDefOf.MeditationFocusGain);
			if (focus != null && !focus.Destroyed)
			{
				num += focus.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn);
			}
			return num / 60000f;
		}

		public static void CheckMeditationScheduleTeachOpportunity(Pawn pawn)
		{
			if (!pawn.Dead && pawn.Spawned && pawn.HasPsylink && pawn.Faction == Faction.OfPlayer && !pawn.IsQuestLodger())
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.MeditationSchedule, pawn, OpportunityType.GoodToKnow);
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.MeditationDesiredPsyfocus, pawn, OpportunityType.GoodToKnow);
			}
		}
	}
}
