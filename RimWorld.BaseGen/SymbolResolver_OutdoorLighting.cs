using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_OutdoorLighting : SymbolResolver
{
	private static List<CompGlower> nearbyGlowers = new List<CompGlower>();

	private const float Margin = 2f;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		ThingDef thingDef = ((rp.faction != null && (int)rp.faction.def.techLevel < 4) ? ThingDefOf.TorchLamp : ThingDefOf.StandingLamp);
		FindNearbyGlowers(rp.rect);
		for (int i = 0; i < rp.rect.Area / 4; i++)
		{
			IntVec3 randomCell = rp.rect.RandomCell;
			if (!randomCell.Standable(map) || randomCell.GetFirstItem(map) != null || randomCell.GetFirstPawn(map) != null || randomCell.GetFirstBuilding(map) != null)
			{
				continue;
			}
			Region region = randomCell.GetRegion(map);
			if (region != null && region.Room.PsychologicallyOutdoors && region.Room.UsesOutdoorTemperature && !AnyGlowerNearby(randomCell) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(randomCell, map))
			{
				if (!rp.spawnBridgeIfTerrainCantSupportThing.HasValue || rp.spawnBridgeIfTerrainCantSupportThing.Value)
				{
					BaseGenUtility.CheckSpawnBridgeUnder(thingDef, randomCell, Rot4.North);
				}
				Thing thing = GenSpawn.Spawn(thingDef, randomCell, map);
				if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
				{
					thing.SetFaction(rp.faction);
					thing.SetStyleDef(rp.faction.ideos?.PrimaryIdeo?.GetStyleFor(thing.def));
				}
				nearbyGlowers.Add(thing.TryGetComp<CompGlower>());
			}
		}
		nearbyGlowers.Clear();
	}

	private void FindNearbyGlowers(CellRect rect)
	{
		Map map = BaseGen.globalSettings.map;
		nearbyGlowers.Clear();
		rect = rect.ExpandedBy(4);
		rect = rect.ClipInsideMap(map);
		foreach (IntVec3 item in rect)
		{
			Region region = item.GetRegion(map);
			if (region == null || !region.Room.PsychologicallyOutdoors)
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				CompGlower compGlower = thingList[i].TryGetComp<CompGlower>();
				if (compGlower != null)
				{
					nearbyGlowers.Add(compGlower);
				}
			}
		}
	}

	private bool AnyGlowerNearby(IntVec3 c)
	{
		for (int i = 0; i < nearbyGlowers.Count; i++)
		{
			if (c.InHorDistOf(nearbyGlowers[i].parent.Position, nearbyGlowers[i].GlowRadius + 2f))
			{
				return true;
			}
		}
		return false;
	}
}
