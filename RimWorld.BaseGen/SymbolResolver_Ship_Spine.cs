using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Ship_Spine : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ThingDef ship_Beam = ThingDefOf.Ship_Beam;
		Map map = BaseGen.globalSettings.map;
		if (!rp.thingRot.HasValue && !rp.thrustAxis.HasValue)
		{
			rp.thrustAxis = Rot4.Random;
			rp.thingRot = rp.thrustAxis;
		}
		IntVec2 intVec = IntVec2.Invalid;
		IntVec2 intVec2 = IntVec2.Invalid;
		IntVec2 toIntVec = rp.thingRot.Value.FacingCell.ToIntVec2;
		int num = 0;
		while (true)
		{
			if (rp.thingRot.Value.IsHorizontal)
			{
				int newZ = Rand.Range(rp.rect.minZ + 1, rp.rect.maxZ - 2);
				intVec = new IntVec2((rp.thingRot.Value == Rot4.East) ? rp.rect.minX : rp.rect.maxX, newZ);
				intVec2 = new IntVec2((rp.thingRot.Value == Rot4.East) ? rp.rect.maxX : rp.rect.minX, newZ);
			}
			else
			{
				int newX = Rand.Range(rp.rect.minX + 1, rp.rect.maxX - 2);
				intVec = new IntVec2(newX, (rp.thingRot.Value == Rot4.North) ? rp.rect.minZ : rp.rect.maxZ);
				intVec2 = new IntVec2(newX, (rp.thingRot.Value == Rot4.North) ? rp.rect.maxZ : rp.rect.minZ);
			}
			bool? allowPlacementOffEdge = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge.HasValue || allowPlacementOffEdge == true || (intVec - toIntVec).ToIntVec3.GetThingList(map).Any((Thing thing2) => thing2.def == ThingDefOf.Ship_Beam))
			{
				break;
			}
			if (num == 20)
			{
				return;
			}
			num++;
		}
		int magnitudeManhattan = (intVec - intVec2).MagnitudeManhattan;
		if (!((intVec - intVec2).Magnitude < (float)ship_Beam.Size.z))
		{
			int num2;
			int num4;
			do
			{
				num2 = ((rp.allowPlacementOffEdge ?? true) ? Rand.Range(0, 7) : 0);
				int num3 = Rand.Range(0, 7);
				num2 = 0;
				num3 = 0;
				num4 = (magnitudeManhattan - num2 - num3) / ship_Beam.Size.z;
			}
			while (num4 <= 0);
			IntVec2 intVec3 = intVec + toIntVec * (num2 + ship_Beam.Size.z / 2 - 1);
			Thing t = null;
			for (int num5 = 0; num5 < num4; num5++)
			{
				Thing thing = ThingMaker.MakeThing(ship_Beam);
				thing.SetFaction(rp.faction);
				t = GenSpawn.Spawn(thing, intVec3.ToIntVec3, map, rp.thingRot.Value);
				intVec3 += toIntVec * ship_Beam.Size.z;
			}
			if (rp.allowPlacementOffEdge ?? true)
			{
				BaseGen.symbolStack.Push("ship_populate", rp);
			}
			CellRect rect;
			Rot4 value;
			CellRect rect2;
			Rot4 value2;
			if (rp.thingRot.Value.IsHorizontal)
			{
				rect = rp.rect;
				rect.minZ = t.OccupiedRect().maxZ + 1;
				value = Rot4.North;
				rect2 = rp.rect;
				rect2.maxZ = t.OccupiedRect().minZ - 1;
				value2 = Rot4.South;
			}
			else
			{
				rect = rp.rect;
				rect.maxX = t.OccupiedRect().minX - 1;
				value = Rot4.West;
				rect2 = rp.rect;
				rect2.minX = t.OccupiedRect().maxX + 1;
				value2 = Rot4.East;
			}
			bool? allowPlacementOffEdge = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge.HasValue || allowPlacementOffEdge == true || Rand.Value < 0.3f)
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = rect;
				resolveParams.thingRot = value;
				resolveParams.allowPlacementOffEdge = false;
				BaseGen.symbolStack.Push("ship_spine", resolveParams);
			}
			allowPlacementOffEdge = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge.HasValue || allowPlacementOffEdge == true || Rand.Value < 0.3f)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.rect = rect2;
				resolveParams2.thingRot = value2;
				resolveParams2.allowPlacementOffEdge = false;
				BaseGen.symbolStack.Push("ship_spine", resolveParams2);
			}
			ResolveParams resolveParams3 = rp;
			resolveParams3.floorDef = TerrainDefOf.Concrete;
			BaseGen.symbolStack.Push("floor", resolveParams3);
		}
	}
}
