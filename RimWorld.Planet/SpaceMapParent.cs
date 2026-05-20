using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class SpaceMapParent : MapParent, INameableWorldObject, IResourceWorldObject
{
	public ThingDef preciousResource;

	public string nameInt;

	public override bool GravShipCanLandOn => !base.HasMap;

	public override string Label
	{
		get
		{
			if (!string.IsNullOrEmpty(nameInt))
			{
				return nameInt;
			}
			return base.Label;
		}
	}

	public string Name
	{
		get
		{
			return nameInt;
		}
		set
		{
			nameInt = value;
		}
	}

	public ThingDef PreciousResource
	{
		get
		{
			return preciousResource;
		}
		set
		{
			preciousResource = value;
		}
	}

	public override MapGeneratorDef MapGeneratorDef => def.mapGenerator ?? MapGeneratorDefOf.Space;

	public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
		{
			yield return transportersFloatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSpace.GetFloatMenuOptions(launchAction, pods, this))
		{
			yield return floatMenuOption;
		}
	}

	public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		foreach (FloatMenuOption shuttleFloatMenuOption in base.GetShuttleFloatMenuOptions(pods, launchAction))
		{
			yield return shuttleFloatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSpace.GetFloatMenuOptions(launchAction, pods, this))
		{
			yield return floatMenuOption;
		}
	}

	public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
	{
		alsoRemoveWorldObject = false;
		if (base.Map.mapPawns.AnyPawnBlockingMapRemoval)
		{
			return false;
		}
		foreach (PocketMapParent item in Find.World.pocketMaps.ToList())
		{
			if (item.sourceMap == base.Map && item.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				return false;
			}
		}
		if (base.Map.AnyBuildingBlockingMapRemoval)
		{
			return false;
		}
		if (TransporterUtility.IncomingTransporterPreventingMapRemoval(base.Map))
		{
			return false;
		}
		alsoRemoveWorldObject = true;
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref nameInt, "nameInt");
		Scribe_Defs.Look(ref preciousResource, "preciousResource");
	}
}
