using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class FireUtility
{
	private static readonly SimpleCurve ChanceToCatchFirePerSecondForPawnFromFlammability = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.1f, 0.07f),
		new CurvePoint(0.3f, 1f),
		new CurvePoint(1f, 1f)
	};

	private static readonly List<Fire> fireList = new List<Fire>();

	public static bool CanEverAttachFire(this Thing t)
	{
		if (t.Destroyed)
		{
			return false;
		}
		if (!t.FlammableNow)
		{
			return false;
		}
		if (t.def.category != ThingCategory.Pawn)
		{
			return false;
		}
		if (t.TryGetComp<CompAttachBase>() == null)
		{
			return false;
		}
		return true;
	}

	public static float ChanceToStartFireIn(IntVec3 c, Map map, SimpleCurve flammabilityChanceCurve = null)
	{
		List<Thing> thingList = c.GetThingList(map);
		float num = c.TerrainFlammability(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (thing is Fire)
			{
				return 0f;
			}
			if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
			{
				num = Mathf.Max(num, thing.GetStatValue(StatDefOf.Flammability));
			}
		}
		if (flammabilityChanceCurve != null)
		{
			num = flammabilityChanceCurve.Evaluate(num);
		}
		if (num > 0f)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
			{
				return 0f;
			}
			List<Thing> thingList2 = c.GetThingList(map);
			for (int j = 0; j < thingList2.Count; j++)
			{
				if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
				{
					return 0f;
				}
			}
		}
		return num;
	}

	public static bool TryStartFireIn(IntVec3 c, Map map, float fireSize, Thing instigator, SimpleCurve flammabilityChanceCurve = null)
	{
		if (ChanceToStartFireIn(c, map, flammabilityChanceCurve) <= 0f)
		{
			return false;
		}
		Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
		obj.fireSize = fireSize;
		obj.instigator = instigator;
		GenSpawn.Spawn(obj, c, map, Rot4.North);
		return true;
	}

	public static float ChanceToAttachFireFromEvent(Thing t)
	{
		return ChanceToAttachFireCumulative(t, 60f);
	}

	public static float ChanceToAttachFireCumulative(Thing t, float freqInTicks)
	{
		if (!t.CanEverAttachFire())
		{
			return 0f;
		}
		if (t.HasAttachment(ThingDefOf.Fire))
		{
			return 0f;
		}
		float num = ChanceToCatchFirePerSecondForPawnFromFlammability.Evaluate(t.GetStatValue(StatDefOf.Flammability));
		return 1f - Mathf.Pow(1f - num, freqInTicks / 60f);
	}

	public static void TryAttachFire(this Thing t, float fireSize, Thing instigator)
	{
		if (t.CanEverAttachFire() && !t.HasAttachment(ThingDefOf.Fire))
		{
			Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
			obj.fireSize = fireSize;
			obj.instigator = instigator;
			obj.AttachTo(t);
			GenSpawn.Spawn(obj, t.Position, t.Map, Rot4.North);
			if (t is Pawn pawn)
			{
				pawn.jobs.StopAll();
				pawn.records.Increment(RecordDefOf.TimesOnFire);
			}
		}
	}

	public static bool IsBurning(this TargetInfo t)
	{
		if (t.HasThing)
		{
			return t.Thing.IsBurning();
		}
		return t.Cell.ContainsStaticFire(t.Map);
	}

	public static bool IsBurning(this Thing t)
	{
		if (t.Destroyed || !t.Spawned)
		{
			return false;
		}
		if (t.def.size == IntVec2.One)
		{
			if (t is Pawn)
			{
				return t.HasAttachment(ThingDefOf.Fire);
			}
			return t.Position.ContainsStaticFire(t.Map);
		}
		foreach (IntVec3 item in t.OccupiedRect())
		{
			if (item.ContainsStaticFire(t.Map))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsStaticFire(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Fire { parent: null })
			{
				return true;
			}
		}
		return false;
	}

	public static int NumFiresAt(IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsBurning())
			{
				num++;
			}
			if (list[i].def.CompDefForAssignableFrom<CompFireOverlayBase>() != null)
			{
				CompGlower compGlower = list[i].TryGetComp<CompGlower>();
				if (compGlower != null && compGlower.Glows)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool ContainsTrap(this IntVec3 c, Map map)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null)
		{
			return edifice is Building_Trap;
		}
		return false;
	}

	public static bool Flammable(this TerrainDef terrain)
	{
		return terrain.GetStatValueAbstract(StatDefOf.Flammability) > 0.01f;
	}

	public static bool TerrainFlammableNow(this IntVec3 c, Map map)
	{
		TerrainDef terrain = c.GetTerrain(map);
		TerrainDef terrainDef = map.terrainGrid.FoundationAt(c);
		if (!terrain.Flammable() && (terrainDef == null || !terrainDef.Flammable()))
		{
			return false;
		}
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].FireBulwark)
			{
				return false;
			}
		}
		return true;
	}

	public static float TerrainFlammability(this IntVec3 c, Map map)
	{
		if (!c.TerrainFlammableNow(map))
		{
			return 0f;
		}
		TerrainDef terrain = c.GetTerrain(map);
		TerrainDef terrainDef = map.terrainGrid.FoundationAt(c);
		float num = terrain.GetStatValueAbstract(StatDefOf.Flammability);
		if (terrainDef != null)
		{
			num = Mathf.Max(num, terrainDef.GetStatValueAbstract(StatDefOf.Flammability));
		}
		return num;
	}

	public static List<Fire> GetFiresNearCell(this IntVec3 cell, Map map)
	{
		fireList.Clear();
		Room room = RegionAndRoomQuery.RoomAt(cell, map);
		if (room == null || room.Dereferenced || room.Fogged || room.IsHuge || room.TouchesMapEdge)
		{
			Region region = cell.GetRegion(map);
			if (region == null)
			{
				List<Thing> list = map.thingGrid.ThingsListAt(cell);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is Fire { parent: null } fire)
					{
						fireList.Add(fire);
					}
				}
			}
			else
			{
				region.ListerThings.GetThingsOfType(fireList);
			}
		}
		else
		{
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int j = 0; j < containedAndAdjacentThings.Count; j++)
			{
				if (containedAndAdjacentThings[j] is Fire item)
				{
					fireList.Add(item);
				}
			}
		}
		fireList.Shuffle();
		fireList.Swap(0, fireList.FindIndex(0, (Fire f) => f.Position == cell));
		return fireList;
	}

	public static float GetEffectiveVacuumForFire(IntVec3 c, Map map)
	{
		float num = 0f;
		Building edifice = c.GetEdifice(map);
		if (edifice != null && map.Biome.inVacuum && edifice.def.passability == Traversability.Impassable && edifice.def.Fillage == FillCategory.Full)
		{
			if (c.GetRoof(map) == null)
			{
				num = 1f;
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					if ((c + GenAdj.CardinalDirections[i]).GetRoof(map) == null)
					{
						num = 1f;
						break;
					}
					num = Mathf.Max(num, c.GetVacuum(map));
				}
			}
		}
		else
		{
			num = c.GetVacuum(map);
		}
		return num;
	}
}
