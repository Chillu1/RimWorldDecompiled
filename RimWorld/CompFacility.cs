using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompFacility : ThingComp
{
	private List<Thing> linkedBuildings = new List<Thing>();

	private const int UpdateRateIntervalTicks = 120;

	private HashSet<Thing> thingsToNotify = new HashSet<Thing>();

	public virtual bool CanBeActive
	{
		get
		{
			CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				return false;
			}
			return true;
		}
	}

	public List<Thing> LinkedBuildings => linkedBuildings;

	public virtual List<StatModifier> StatOffsets => Props.statOffsets;

	public CompProperties_Facility Props => (CompProperties_Facility)props;

	protected virtual string MaxConnectedString => "FacilityMaxSimultaneousConnections".Translate();

	public event Action<CompFacility, Thing> OnLinkAdded;

	public event Action<CompFacility, Thing> OnLinkRemoved;

	public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
	{
		CompProperties_Facility compProperties = myDef.GetCompProperties<CompProperties_Facility>();
		if (compProperties?.linkableBuildings == null)
		{
			return;
		}
		Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
		for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
			{
				CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
				{
					GenDraw.DrawLineBetween(a, item.TrueCenter());
					compAffectedByFacilities.DrawRedLineToPotentiallySupplantedFacility(myDef, myPos, myRot);
				}
			}
		}
	}

	public static void DrawPlaceMouseAttachmentsToPotentialThingsToLinkTo(float curX, ref float curY, ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
	{
		CompProperties_Facility compProperties = myDef.GetCompProperties<CompProperties_Facility>();
		int num = 0;
		for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
			{
				CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
				{
					num++;
					if (num == 1)
					{
						DrawTextLine(ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");
					}
					DrawTextLine(ref curY, "  - " + item.LabelCap);
				}
			}
		}
		if (num == 0)
		{
			DrawTextLine(ref curY, "FacilityNoPotentialLinks".Translate());
		}
		void DrawTextLine(ref float y, string text)
		{
			float lineHeight = Text.LineHeight;
			Widgets.Label(new Rect(curX, y, 999f, lineHeight), text);
			y += lineHeight;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (Props.mustBePlacedFacingThingLinear && parent.Spawned && parent.IsHashIntervalTick(120))
		{
			bool flag = ContainmentUtility.IsLinearBuildingBlocked(parent.def, parent.Position, parent.Rotation, parent.Map);
			if ((linkedBuildings.Any() && flag) || (linkedBuildings.Empty() && !flag))
			{
				RelinkAll();
			}
		}
	}

	public virtual bool CanLink()
	{
		return true;
	}

	public void Notify_NewLink(Thing thing)
	{
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			if (linkedBuildings[i] == thing)
			{
				Log.Error("Notify_NewLink was called but the link is already here.");
				return;
			}
		}
		linkedBuildings.Add(thing);
		this.OnLinkAdded?.Invoke(this, thing);
	}

	public void Notify_LinkRemoved(Thing thing)
	{
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			if (linkedBuildings[i] == thing)
			{
				linkedBuildings.RemoveAt(i);
				this.OnLinkRemoved?.Invoke(this, thing);
				return;
			}
		}
		Log.Error("Notify_LinkRemoved was called but there is no such link here.");
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
		LinkToNearbyBuildings();
	}

	public override void PostMapInit()
	{
		RelinkAll();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		thingsToNotify.Clear();
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			thingsToNotify.Add(linkedBuildings[i]);
		}
		UnlinkAll();
		foreach (Thing item in thingsToNotify)
		{
			item.TryGetComp<CompAffectedByFacilities>().Notify_FacilityDespawned();
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			if (linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().IsFacilityActive(parent))
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), linkedBuildings[i].TrueCenter());
			}
			else
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), linkedBuildings[i].TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (StatOffsets != null)
		{
			bool flag = AmIActiveForAnyone();
			for (int i = 0; i < StatOffsets.Count; i++)
			{
				StatDef stat = StatOffsets[i].stat;
				stringBuilder.Append(stat.OffsetLabelCap);
				stringBuilder.Append(": ");
				stringBuilder.Append(StatOffsets[i].ValueToStringAsOffset);
				if (!flag)
				{
					stringBuilder.Append(" (");
					stringBuilder.Append("InactiveFacility".Translate());
					stringBuilder.Append(")");
				}
				if (i < StatOffsets.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.Append("\n");
		}
		CompProperties_Facility compProperties_Facility = Props;
		if (compProperties_Facility.showMaxSimultaneous)
		{
			stringBuilder.Append(MaxConnectedString);
			stringBuilder.Append(": " + compProperties_Facility.maxSimultaneous);
		}
		if (compProperties_Facility.mustBePlacedFacingThingLinear && parent.Spawned && ContainmentUtility.IsLinearBuildingBlocked(parent.def, parent.Position, parent.Rotation, parent.Map))
		{
			stringBuilder.AppendInNewLine("FacilityFrontBlocked".Translate());
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private void RelinkAll()
	{
		LinkToNearbyBuildings();
	}

	private void LinkToNearbyBuildings()
	{
		UnlinkAll();
		CompProperties_Facility compProperties_Facility = Props;
		if (compProperties_Facility.linkableBuildings == null)
		{
			return;
		}
		for (int i = 0; i < compProperties_Facility.linkableBuildings.Count; i++)
		{
			foreach (Thing item in parent.Map.listerThings.ThingsOfDef(compProperties_Facility.linkableBuildings[i]))
			{
				CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null && compAffectedByFacilities.CanLinkTo(parent))
				{
					linkedBuildings.Add(item);
					compAffectedByFacilities.Notify_NewLink(parent);
					this.OnLinkAdded?.Invoke(this, compAffectedByFacilities.parent);
				}
			}
		}
	}

	private bool AmIActiveForAnyone()
	{
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			if (linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().IsFacilityActive(parent))
			{
				return true;
			}
		}
		return false;
	}

	private void UnlinkAll()
	{
		for (int i = 0; i < linkedBuildings.Count; i++)
		{
			linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().Notify_LinkRemoved(parent);
			this.OnLinkRemoved?.Invoke(this, linkedBuildings[i]);
		}
		linkedBuildings.Clear();
	}
}
