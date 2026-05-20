using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompAffectedByFacilities : ThingComp
{
	private List<Thing> linkedFacilities = new List<Thing>();

	public static Material InactiveFacilityLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

	private static readonly Dictionary<ThingDef, int> alreadyReturnedCount = new Dictionary<ThingDef, int>();

	private List<ThingDef> alreadyUsed = new List<ThingDef>();

	public List<Thing> LinkedFacilitiesListForReading => linkedFacilities;

	private IEnumerable<Thing> ThingsICanLinkTo
	{
		get
		{
			if (!parent.Spawned)
			{
				yield break;
			}
			IEnumerable<Thing> enumerable = PotentialThingsToLinkTo(parent.def, parent.Position, parent.Rotation, parent.Map);
			foreach (Thing item in enumerable)
			{
				if (CanLinkTo(item))
				{
					yield return item;
				}
			}
		}
	}

	public bool CanLinkTo(Thing facility)
	{
		if (!facility.TryGetComp(out CompFacility comp))
		{
			return false;
		}
		if (!comp.CanLink())
		{
			return false;
		}
		if (!CanPotentiallyLinkTo(facility.def, facility.Position, facility.Rotation))
		{
			return false;
		}
		if (!IsValidFacilityForMe(facility))
		{
			return false;
		}
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (linkedFacilities[i] == facility)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanPotentiallyLinkTo_Static(Thing facility, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map myMap)
	{
		if (!CanPotentiallyLinkTo_Static(facility.def, facility.Position, facility.Rotation, myDef, myPos, myRot, myMap))
		{
			return false;
		}
		if (!IsPotentiallyValidFacilityForMe_Static(facility, myDef, myPos, myRot))
		{
			return false;
		}
		return true;
	}

	public bool CanPotentiallyLinkTo(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot)
	{
		if (!CanPotentiallyLinkTo_Static(facilityDef, facilityPos, facilityRot, parent.def, parent.Position, parent.Rotation, parent.Map))
		{
			return false;
		}
		if (!IsPotentiallyValidFacilityForMe(facilityDef, facilityPos, facilityRot))
		{
			return false;
		}
		int num = 0;
		bool flag = false;
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (linkedFacilities[i].def == facilityDef)
			{
				num++;
				if (IsBetter(facilityDef, facilityPos, facilityRot, linkedFacilities[i]))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			return true;
		}
		CompProperties_Facility compProperties = facilityDef.GetCompProperties<CompProperties_Facility>();
		if (num + 1 > compProperties.maxSimultaneous)
		{
			return false;
		}
		return true;
	}

	public static bool CanPotentiallyLinkTo_Static(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map myMap)
	{
		CompProperties_Facility compProperties = facilityDef.GetCompProperties<CompProperties_Facility>();
		if (compProperties.mustBePlacedAdjacent)
		{
			CellRect rect = GenAdj.OccupiedRect(myPos, myRot, myDef.size);
			CellRect rect2 = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.size);
			if (!GenAdj.AdjacentTo8WayOrInside(rect, rect2))
			{
				return false;
			}
		}
		if (compProperties.mustBePlacedFacingThingLinear)
		{
			if (ContainmentUtility.IsLinearBuildingBlocked(facilityDef, facilityPos, facilityRot, myMap))
			{
				return false;
			}
			CellRect cellRect = GenAdj.OccupiedRect(myPos, myRot, myDef.size);
			foreach (IntVec3 inhibitorAffectedCell in ContainmentUtility.GetInhibitorAffectedCells(facilityDef, facilityPos, facilityRot, myMap))
			{
				if (cellRect.Cells.Contains(inhibitorAffectedCell))
				{
					return true;
				}
			}
			return false;
		}
		if (compProperties.mustBePlacedAdjacentCardinalToBedHead || compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead)
		{
			if (!myDef.IsBed)
			{
				return false;
			}
			CellRect other = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.size);
			bool flag = false;
			int sleepingSlotsCount = BedUtility.GetSleepingSlotsCount(myDef.size);
			for (int i = 0; i < sleepingSlotsCount; i++)
			{
				IntVec3 sleepingSlotPos = BedUtility.GetSleepingSlotPos(i, myPos, myRot, myDef.size);
				if (!sleepingSlotPos.IsAdjacentToCardinalOrInside(other))
				{
					continue;
				}
				if (compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead)
				{
					if (other.MovedBy(facilityRot.FacingCell).Contains(sleepingSlotPos))
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (!compProperties.mustBePlacedAdjacent && !compProperties.mustBePlacedAdjacentCardinalToBedHead && !compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead)
		{
			Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
			Vector3 b = GenThing.TrueCenter(facilityPos, facilityRot, facilityDef.size, facilityDef.Altitude);
			float num = Vector3.Distance(a, b);
			if (num > compProperties.maxDistance || (compProperties.minDistance > 0f && num < compProperties.minDistance))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsValidFacilityForMe(Thing facility)
	{
		if (!IsPotentiallyValidFacilityForMe_Static(facility, parent.def, parent.Position, parent.Rotation))
		{
			return false;
		}
		return true;
	}

	private bool IsPotentiallyValidFacilityForMe(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot)
	{
		if (!IsPotentiallyValidFacilityForMe_Static(facilityDef, facilityPos, facilityRot, parent.def, parent.Position, parent.Rotation, parent.Map))
		{
			return false;
		}
		if (facilityDef.GetCompProperties<CompProperties_Facility>().canLinkToMedBedsOnly && !(parent is Building_Bed { Medical: not false }))
		{
			return false;
		}
		return true;
	}

	private static bool IsPotentiallyValidFacilityForMe_Static(Thing facility, ThingDef myDef, IntVec3 myPos, Rot4 myRot)
	{
		return IsPotentiallyValidFacilityForMe_Static(facility.def, facility.Position, facility.Rotation, myDef, myPos, myRot, facility.Map);
	}

	private static bool IsPotentiallyValidFacilityForMe_Static(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
	{
		if (!facilityDef.GetCompProperties<CompProperties_Facility>().requiresLOS)
		{
			return true;
		}
		CellRect startRect = GenAdj.OccupiedRect(myPos, myRot, myDef.size);
		CellRect endRect = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.size);
		bool flag = false;
		for (int i = startRect.minZ; i <= startRect.maxZ; i++)
		{
			for (int j = startRect.minX; j <= startRect.maxX; j++)
			{
				for (int k = endRect.minZ; k <= endRect.maxZ; k++)
				{
					int num = endRect.minX;
					while (num <= endRect.maxX)
					{
						IntVec3 start = new IntVec3(j, 0, i);
						IntVec3 end = new IntVec3(num, 0, k);
						if (!GenSight.LineOfSight(start, end, map, startRect, endRect))
						{
							num++;
							continue;
						}
						goto IL_007a;
					}
				}
			}
			continue;
			IL_007a:
			flag = true;
			break;
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	public void Notify_NewLink(Thing facility)
	{
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (linkedFacilities[i] == facility)
			{
				Log.Error("Notify_NewLink was called but the link is already here.");
				return;
			}
		}
		Thing potentiallySupplantedFacility = GetPotentiallySupplantedFacility(facility.def, facility.Position, facility.Rotation);
		if (potentiallySupplantedFacility != null)
		{
			potentiallySupplantedFacility.TryGetComp<CompFacility>().Notify_LinkRemoved(parent);
			linkedFacilities.Remove(potentiallySupplantedFacility);
		}
		linkedFacilities.Add(facility);
	}

	public void Notify_LinkRemoved(Thing thing)
	{
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (linkedFacilities[i] == thing)
			{
				linkedFacilities.RemoveAt(i);
				return;
			}
		}
		Log.Error("Notify_LinkRemoved was called but there is no such link here.");
	}

	public void Notify_FacilityDespawned()
	{
		RelinkAll();
	}

	public void Notify_LOSBlockerSpawnedOrDespawned()
	{
		RelinkAll();
	}

	public void Notify_ThingChanged()
	{
		RelinkAll();
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		LinkToNearbyFacilities();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		UnlinkAll();
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (IsFacilityActive(linkedFacilities[i]))
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), linkedFacilities[i].TrueCenter());
			}
			else
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), linkedFacilities[i].TrueCenter(), InactiveFacilityLineMat);
			}
		}
	}

	private bool IsBetter(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, Thing thanThisFacility)
	{
		if (facilityDef != thanThisFacility.def)
		{
			Log.Error("Comparing two different facility defs.");
			return false;
		}
		Vector3 b = GenThing.TrueCenter(facilityPos, facilityRot, facilityDef.size, facilityDef.Altitude);
		Vector3 a = parent.TrueCenter();
		float num = Vector3.Distance(a, b);
		float num2 = Vector3.Distance(a, thanThisFacility.TrueCenter());
		if (num != num2)
		{
			return num < num2;
		}
		if (facilityPos.x != thanThisFacility.Position.x)
		{
			return facilityPos.x < thanThisFacility.Position.x;
		}
		return facilityPos.z < thanThisFacility.Position.z;
	}

	public static IEnumerable<Thing> PotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map myMap)
	{
		alreadyReturnedCount.Clear();
		CompProperties_AffectedByFacilities compProperties = myDef.GetCompProperties<CompProperties_AffectedByFacilities>();
		if (compProperties.linkableFacilities == null)
		{
			yield break;
		}
		IEnumerable<Thing> enumerable = Enumerable.Empty<Thing>();
		for (int i = 0; i < compProperties.linkableFacilities.Count; i++)
		{
			enumerable = enumerable.Concat(myMap.listerThings.ThingsOfDef(compProperties.linkableFacilities[i]));
		}
		Vector3 myTrueCenter = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
		IOrderedEnumerable<Thing> orderedEnumerable = from x in enumerable
			orderby Vector3.Distance(myTrueCenter, x.TrueCenter()), x.Position.x, x.Position.z
			select x;
		foreach (Thing item in orderedEnumerable)
		{
			if (!item.TryGetComp(out CompFacility comp) || !comp.CanLink() || !CanPotentiallyLinkTo_Static(item, myDef, myPos, myRot, myMap))
			{
				continue;
			}
			if (alreadyReturnedCount.TryGetValue(item.def, out var value))
			{
				if (value >= comp.Props.maxSimultaneous)
				{
					continue;
				}
			}
			else
			{
				alreadyReturnedCount.Add(item.def, 0);
			}
			alreadyReturnedCount[item.def]++;
			yield return item;
		}
	}

	public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
	{
		Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
		foreach (Thing item in PotentialThingsToLinkTo(myDef, myPos, myRot, map))
		{
			GenDraw.DrawLineBetween(a, item.TrueCenter());
		}
	}

	public static void DrawPlaceMouseAttachmentsToPotentialThingsToLinkTo(float curX, ref float curY, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
	{
		int num = 0;
		foreach (Thing item in PotentialThingsToLinkTo(myDef, myPos, myRot, map))
		{
			num++;
			if (num == 1)
			{
				DrawTextLine(ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");
			}
			DrawTextLine(ref curY, "  - " + item.LabelCap);
		}
		void DrawTextLine(ref float y, string text)
		{
			float lineHeight = Text.LineHeight;
			Widgets.Label(new Rect(curX, y, 999f, lineHeight), text);
			y += lineHeight;
		}
	}

	public void DrawRedLineToPotentiallySupplantedFacility(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot)
	{
		Thing potentiallySupplantedFacility = GetPotentiallySupplantedFacility(facilityDef, facilityPos, facilityRot);
		if (potentiallySupplantedFacility != null)
		{
			GenDraw.DrawLineBetween(parent.TrueCenter(), potentiallySupplantedFacility.TrueCenter(), InactiveFacilityLineMat);
		}
	}

	private Thing GetPotentiallySupplantedFacility(ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot)
	{
		Thing thing = null;
		int num = 0;
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			if (linkedFacilities[i].def == facilityDef)
			{
				if (thing == null)
				{
					thing = linkedFacilities[i];
				}
				num++;
			}
		}
		if (num == 0)
		{
			return null;
		}
		CompProperties_Facility compProperties = facilityDef.GetCompProperties<CompProperties_Facility>();
		if (num + 1 <= compProperties.maxSimultaneous)
		{
			return null;
		}
		Thing thing2 = thing;
		for (int j = 0; j < linkedFacilities.Count; j++)
		{
			if (facilityDef == linkedFacilities[j].def && IsBetter(thing2.def, thing2.Position, thing2.Rotation, linkedFacilities[j]))
			{
				thing2 = linkedFacilities[j];
			}
		}
		return thing2;
	}

	public override float GetStatOffset(StatDef stat)
	{
		float num = 0f;
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			CompFacility compFacility = linkedFacilities[i].TryGetComp<CompFacility>();
			if (compFacility.StatOffsets != null)
			{
				float statOffsetFromList = compFacility.StatOffsets.GetStatOffsetFromList(stat);
				if (statOffsetFromList != 0f && IsFacilityActive(linkedFacilities[i]))
				{
					num += statOffsetFromList;
				}
			}
		}
		return num;
	}

	public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
	{
		alreadyUsed.Clear();
		bool flag = false;
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			bool flag2 = false;
			for (int j = 0; j < alreadyUsed.Count; j++)
			{
				if (alreadyUsed[j] == linkedFacilities[i].def)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2 || !IsFacilityActive(linkedFacilities[i]))
			{
				continue;
			}
			CompFacility compFacility = linkedFacilities[i].TryGetComp<CompFacility>();
			if (compFacility.StatOffsets == null)
			{
				continue;
			}
			float statOffsetFromList = compFacility.StatOffsets.GetStatOffsetFromList(stat);
			if (statOffsetFromList == 0f)
			{
				continue;
			}
			if (!flag)
			{
				flag = true;
				sb.AppendLine();
				sb.AppendLine(whitespace + "StatsReport_Facilities".Translate() + ":");
			}
			int num = 0;
			for (int k = 0; k < linkedFacilities.Count; k++)
			{
				if (IsFacilityActive(linkedFacilities[k]) && linkedFacilities[k].def == linkedFacilities[i].def)
				{
					num++;
				}
			}
			statOffsetFromList *= (float)num;
			sb.Append(whitespace + "    ");
			if (num != 1)
			{
				sb.Append(num + "x ");
			}
			sb.AppendLine(linkedFacilities[i].LabelCap + ": " + statOffsetFromList.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
			alreadyUsed.Add(linkedFacilities[i].def);
		}
	}

	private void RelinkAll()
	{
		LinkToNearbyFacilities();
	}

	public bool IsFacilityActive(Thing facility)
	{
		return facility.TryGetComp<CompFacility>().CanBeActive;
	}

	private void LinkToNearbyFacilities()
	{
		UnlinkAll();
		if (!parent.Spawned)
		{
			return;
		}
		foreach (Thing item in ThingsICanLinkTo)
		{
			if (item.TryGetComp(out CompFacility comp))
			{
				linkedFacilities.Add(item);
				comp.Notify_NewLink(parent);
			}
		}
	}

	private void UnlinkAll()
	{
		for (int i = 0; i < linkedFacilities.Count; i++)
		{
			linkedFacilities[i].TryGetComp<CompFacility>().Notify_LinkRemoved(parent);
		}
		linkedFacilities.Clear();
	}
}
