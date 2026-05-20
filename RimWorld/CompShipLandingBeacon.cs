using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompShipLandingBeacon : ThingComp
{
	private List<ShipLandingArea> landingAreas = new List<ShipLandingArea>();

	private Color fieldColor = Color.white;

	public CompProperties_ShipLandingBeacon Props => (CompProperties_ShipLandingBeacon)props;

	public List<ShipLandingArea> LandingAreas => landingAreas;

	public bool Active => parent.GetComp<CompPowerTrader>()?.PowerOn ?? true;

	private bool CanLinkTo(CompShipLandingBeacon other)
	{
		if (other == this)
		{
			return false;
		}
		return ShipLandingBeaconUtility.CanLinkTo(parent.Position, other);
	}

	public void EstablishConnections()
	{
		if (!parent.Spawned)
		{
			return;
		}
		List<CompShipLandingBeacon> list = new List<CompShipLandingBeacon>();
		List<CompShipLandingBeacon> list2 = new List<CompShipLandingBeacon>();
		List<Thing> list3 = parent.Map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon);
		foreach (Thing item in list3)
		{
			CompShipLandingBeacon compShipLandingBeacon = item.TryGetComp<CompShipLandingBeacon>();
			if (compShipLandingBeacon != null && CanLinkTo(compShipLandingBeacon))
			{
				if (parent.Position.x == compShipLandingBeacon.parent.Position.x)
				{
					list2.Add(compShipLandingBeacon);
				}
				else if (parent.Position.z == compShipLandingBeacon.parent.Position.z)
				{
					list.Add(compShipLandingBeacon);
				}
			}
		}
		foreach (CompShipLandingBeacon h in list)
		{
			foreach (CompShipLandingBeacon v in list2)
			{
				Thing thing = list3.FirstOrDefault((Thing x) => x.Position.x == h.parent.Position.x && x.Position.z == v.parent.Position.z);
				if (thing != null)
				{
					ShipLandingArea shipLandingArea = new ShipLandingArea(CellRect.FromLimits(thing.Position, parent.Position).ContractedBy(1), parent.Map);
					shipLandingArea.beacons = new List<CompShipLandingBeacon>
					{
						this,
						thing.TryGetComp<CompShipLandingBeacon>(),
						v,
						h
					};
					TryAddArea(shipLandingArea);
				}
			}
		}
		for (int num = landingAreas.Count - 1; num >= 0; num--)
		{
			foreach (CompShipLandingBeacon beacon in landingAreas[num].beacons)
			{
				if (!beacon.TryAddArea(landingAreas[num]))
				{
					RemoveArea(landingAreas[num]);
					break;
				}
			}
		}
	}

	private void RemoveArea(ShipLandingArea area)
	{
		foreach (CompShipLandingBeacon beacon in area.beacons)
		{
			if (beacon.landingAreas.Contains(area))
			{
				beacon.landingAreas.Remove(area);
			}
		}
		landingAreas.Remove(area);
	}

	public bool TryAddArea(ShipLandingArea newArea)
	{
		if (!landingAreas.Contains(newArea))
		{
			for (int num = landingAreas.Count - 1; num >= 0; num--)
			{
				if (landingAreas[num].MyRect.Overlaps(newArea.MyRect) && landingAreas[num].MyRect != newArea.MyRect)
				{
					if (landingAreas[num].MyRect.Area <= newArea.MyRect.Area)
					{
						return false;
					}
					RemoveArea(landingAreas[num]);
				}
			}
			landingAreas.Add(newArea);
		}
		return true;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		CompGlower compGlower = parent.TryGetComp<CompGlower>();
		if (compGlower != null)
		{
			fieldColor = compGlower.GlowColor.ToColor.ToOpaque();
		}
		EstablishConnections();
		foreach (ShipLandingArea landingArea in landingAreas)
		{
			landingArea.RecalculateBlockingThing();
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		for (int num = landingAreas.Count - 1; num >= 0; num--)
		{
			RemoveArea(landingAreas[num]);
		}
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon))
		{
			item.TryGetComp<CompShipLandingBeacon>()?.EstablishConnections();
		}
	}

	public override void CompTickRare()
	{
		foreach (ShipLandingArea landingArea in landingAreas)
		{
			landingArea.RecalculateBlockingThing();
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		foreach (ShipLandingArea landingArea in landingAreas)
		{
			if (landingArea.Active)
			{
				Color color = (landingArea.Clear ? fieldColor : Color.red);
				color.a = Pulser.PulseBrightness(1f, 0.6f);
				GenDraw.DrawFieldEdges(landingArea.MyRect.ToList(), color);
			}
			foreach (CompShipLandingBeacon beacon in landingArea.beacons)
			{
				if (CanLinkTo(beacon))
				{
					GenDraw.DrawLineBetween(parent.TrueCenter(), beacon.parent.TrueCenter(), SimpleColor.White);
				}
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!parent.Spawned)
		{
			return null;
		}
		string text = "";
		if (!Active)
		{
			text += "NotUsable".Translate() + ": " + "Unpowered".Translate().CapitalizeFirst();
		}
		for (int i = 0; i < landingAreas.Count; i++)
		{
			if (!landingAreas[i].Clear)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "NotUsable".Translate() + ": ";
				text = ((!landingAreas[i].BlockedByRoof) ? ((!landingAreas[i].BlockedByTerrainAffordance) ? ((string)(text + "BlockedBy".Translate(landingAreas[i].FirstBlockingThing).CapitalizeFirst())) : ((string)(text + "BlockedByTerrain".Translate(Props.landingAreaTerrainSupport)))) : ((string)(text + "BlockedByRoof".Translate().CapitalizeFirst())));
				break;
			}
		}
		foreach (Thing item in parent.Map.listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon))
		{
			if (item != parent && ShipLandingBeaconUtility.AlignedDistanceTooShort(parent.Position, item.Position, Props.edgeLengthRange.min - 1f))
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "NotUsable".Translate() + ": " + "TooCloseToOtherBeacon".Translate().CapitalizeFirst();
				break;
			}
		}
		return text;
	}
}
