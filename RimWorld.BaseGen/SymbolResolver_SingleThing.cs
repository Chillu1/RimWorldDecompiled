using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_SingleThing : SymbolResolver
{
	private static Rot4[] tmpRotations = new Rot4[4]
	{
		Rot4.North,
		Rot4.South,
		Rot4.West,
		Rot4.East
	};

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (rp.singleThingToSpawn != null && rp.singleThingToSpawn.Spawned)
		{
			return true;
		}
		IntVec3 cell;
		if (rp.singleThingToSpawn is Pawn)
		{
			ResolveParams rp2 = rp;
			rp2.singlePawnToSpawn = (Pawn)rp.singleThingToSpawn;
			if (!SymbolResolver_SinglePawn.TryFindSpawnCell(rp2, out cell))
			{
				return false;
			}
		}
		if (((rp.singleThingDef != null && rp.singleThingDef.category == ThingCategory.Item) || (rp.singleThingToSpawn != null && rp.singleThingToSpawn.def.category == ThingCategory.Item)) && !TryFindSpawnCellForItem(rp.rect, out cell))
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (rp.singleThingToSpawn is Pawn)
		{
			ResolveParams resolveParams = rp;
			resolveParams.singlePawnToSpawn = (Pawn)rp.singleThingToSpawn;
			BaseGen.symbolStack.Push("pawn", resolveParams);
		}
		else
		{
			if (rp.singleThingToSpawn != null && rp.singleThingToSpawn.Spawned)
			{
				return;
			}
			ThingDef thingDef = ((rp.singleThingToSpawn != null) ? rp.singleThingToSpawn.def : (rp.singleThingDef ?? ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsWeapon || x.IsMedicine || x.IsDrug).RandomElement()));
			Rot4? rot = rp.thingRot;
			IntVec3 result;
			if (thingDef.category == ThingCategory.Item)
			{
				rot = Rot4.North;
				if (!TryFindSpawnCellForItem(rp.rect, out result, rp.spawnOutside == true))
				{
					if (rp.singleThingToSpawn != null)
					{
						rp.singleThingToSpawn.Destroy();
					}
					return;
				}
			}
			else
			{
				result = FindBestSpawnCellForNonItem(rp.rect, thingDef, ref rot, out var hasToWipeBuilding, out var doesntFit, rp.spawnOutside == true);
				if ((hasToWipeBuilding || doesntFit) && rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue && rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value)
				{
					return;
				}
			}
			if (!rot.HasValue)
			{
				Log.Error("Could not resolve rotation. Bug.");
			}
			Thing thing;
			if (rp.singleThingToSpawn == null)
			{
				ThingDef stuff = ((rp.singleThingStuff != null && rp.singleThingStuff.stuffProps.CanMake(thingDef)) ? rp.singleThingStuff : ((thingDef.building == null) ? GenStuff.RandomStuffInexpensiveFor(thingDef, rp.faction) : BaseGenUtility.CheapStuffFor(thingDef, rp.faction)));
				thing = ThingMaker.MakeThing(thingDef, stuff);
				thing.stackCount = rp.singleThingStackCount ?? 1;
				if (thing.stackCount <= 0)
				{
					thing.stackCount = 1;
				}
				if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
				{
					thing.SetFaction(rp.faction);
					thing.SetStyleDef(rp.faction.ideos?.PrimaryIdeo?.GetStyleFor(thing.def));
				}
				thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);
				if (rp.postThingGenerate != null)
				{
					rp.postThingGenerate(thing);
				}
			}
			else
			{
				thing = rp.singleThingToSpawn;
			}
			if (!rp.spawnBridgeIfTerrainCantSupportThing.HasValue || rp.spawnBridgeIfTerrainCantSupportThing.Value)
			{
				BaseGenUtility.CheckSpawnBridgeUnder(thing.def, result, rot.Value);
			}
			thing = GenSpawn.Spawn(thing, result, BaseGen.globalSettings.map, rot.Value);
			if (thing != null && rp.singleThingInnerThings != null)
			{
				CompThingContainer compThingContainer = thing.TryGetComp<CompThingContainer>();
				if (compThingContainer != null)
				{
					foreach (Thing singleThingInnerThing in rp.singleThingInnerThings)
					{
						if (compThingContainer.Accepts(singleThingInnerThing))
						{
							compThingContainer.innerContainer.TryAdd(singleThingInnerThing);
						}
					}
				}
				else if (thing is Building_Casket building_Casket)
				{
					ThingOwner directlyHeldThings = building_Casket.GetDirectlyHeldThings();
					foreach (Thing singleThingInnerThing2 in rp.singleThingInnerThings)
					{
						directlyHeldThings.TryAdd(singleThingInnerThing2);
					}
				}
			}
			if (thing != null && thing.def.category == ThingCategory.Item)
			{
				thing.SetForbidden(value: true, warnOnFail: false);
			}
			if (rp.postThingSpawn != null)
			{
				rp.postThingSpawn(thing);
			}
		}
	}

	private bool TryFindSpawnCellForItem(CellRect rect, out IntVec3 result, bool spawnOutside = false)
	{
		Map map = BaseGen.globalSettings.map;
		return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 c)
		{
			if (c.GetFirstItem(map) != null)
			{
				return false;
			}
			if (!c.Standable(map))
			{
				SurfaceType surfaceType = c.GetSurfaceType(map);
				if (surfaceType != SurfaceType.Item && surfaceType != SurfaceType.Eat)
				{
					return false;
				}
			}
			return (!spawnOutside || !c.Roofed(map)) ? true : false;
		}, out result);
	}

	private IntVec3 FindBestSpawnCellForNonItem(CellRect rect, ThingDef thingDef, ref Rot4? rot, out bool hasToWipeBuilding, out bool doesntFit, bool spawnOutside = false)
	{
		if (!thingDef.rotatable)
		{
			rot = Rot4.North;
		}
		if (!rot.HasValue)
		{
			tmpRotations.Shuffle();
			for (int i = 0; i < tmpRotations.Length; i++)
			{
				IntVec3 result = FindBestSpawnCellForNonItem(rect, thingDef, tmpRotations[i], out hasToWipeBuilding, out doesntFit, spawnOutside);
				if (!hasToWipeBuilding && !doesntFit)
				{
					rot = tmpRotations[i];
					return result;
				}
			}
			for (int j = 0; j < tmpRotations.Length; j++)
			{
				IntVec3 result2 = FindBestSpawnCellForNonItem(rect, thingDef, tmpRotations[j], out hasToWipeBuilding, out doesntFit, spawnOutside);
				if (!doesntFit)
				{
					rot = tmpRotations[j];
					return result2;
				}
			}
			rot = Rot4.Random;
			return FindBestSpawnCellForNonItem(rect, thingDef, rot.Value, out hasToWipeBuilding, out doesntFit, spawnOutside);
		}
		return FindBestSpawnCellForNonItem(rect, thingDef, rot.Value, out hasToWipeBuilding, out doesntFit, spawnOutside);
	}

	private IntVec3 FindBestSpawnCellForNonItem(CellRect rect, ThingDef thingDef, Rot4 rot, out bool hasToWipeBuilding, out bool doesntFit, bool spawnOutside = false)
	{
		Map map = BaseGen.globalSettings.map;
		if (thingDef.category == ThingCategory.Building)
		{
			foreach (IntVec3 item in rect.Cells.InRandomOrder())
			{
				CellRect rect2 = GenAdj.OccupiedRect(item, rot, thingDef.size);
				if (rect2.FullyContainedWithin(rect) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(rect2, map) && !AnyNonStandableCellOrAnyBuildingInside(rect2) && GenConstruct.TerrainCanSupport(rect2, map, thingDef) && (!spawnOutside || !item.Roofed(map)) && !InteractionCellBlocked(thingDef, item, rot, map))
				{
					hasToWipeBuilding = false;
					doesntFit = false;
					return item;
				}
			}
			foreach (IntVec3 item2 in rect.Cells.InRandomOrder())
			{
				CellRect rect3 = GenAdj.OccupiedRect(item2, rot, thingDef.size);
				if (rect3.FullyContainedWithin(rect) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(rect3, map) && !AnyNonStandableCellOrAnyBuildingInside(rect3) && (!spawnOutside || !item2.Roofed(map)) && !InteractionCellBlocked(thingDef, item2, rot, map))
				{
					hasToWipeBuilding = false;
					doesntFit = false;
					return item2;
				}
			}
		}
		foreach (IntVec3 item3 in rect.Cells.InRandomOrder())
		{
			CellRect rect4 = GenAdj.OccupiedRect(item3, rot, thingDef.size);
			if (rect4.FullyContainedWithin(rect) && !AnyNonStandableCellOrAnyBuildingInside(rect4) && (!spawnOutside || !item3.Roofed(map)) && !InteractionCellBlocked(thingDef, item3, rot, map))
			{
				hasToWipeBuilding = false;
				doesntFit = false;
				return item3;
			}
		}
		foreach (IntVec3 item4 in rect.Cells.InRandomOrder())
		{
			if (GenAdj.OccupiedRect(item4, rot, thingDef.size).FullyContainedWithin(rect) && (!spawnOutside || !item4.Roofed(map)))
			{
				hasToWipeBuilding = true;
				doesntFit = false;
				return item4;
			}
		}
		IntVec3 centerCell = rect.CenterCell;
		CellRect cellRect = GenAdj.OccupiedRect(centerCell, rot, thingDef.size);
		if (cellRect.minX < 0)
		{
			centerCell.x += -cellRect.minX;
		}
		if (cellRect.minZ < 0)
		{
			centerCell.z += -cellRect.minZ;
		}
		if (cellRect.maxX >= map.Size.x)
		{
			centerCell.x -= cellRect.maxX - map.Size.x + 1;
		}
		if (cellRect.maxZ >= map.Size.z)
		{
			centerCell.z -= cellRect.maxZ - map.Size.z + 1;
		}
		hasToWipeBuilding = true;
		doesntFit = true;
		return centerCell;
	}

	private bool AnyNonStandableCellOrAnyBuildingInside(CellRect rect)
	{
		Map map = BaseGen.globalSettings.map;
		foreach (IntVec3 item in rect)
		{
			if (!item.Standable(map) || item.GetEdifice(map) != null)
			{
				return true;
			}
		}
		return false;
	}

	private bool InteractionCellBlocked(ThingDef thingDef, IntVec3 cell, Rot4 rot, Map map)
	{
		if (!thingDef.hasInteractionCell)
		{
			return false;
		}
		if (!ThingUtility.InteractionCellWhenAt(thingDef, cell, rot, map).Standable(map))
		{
			return true;
		}
		return false;
	}
}
