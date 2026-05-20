using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GenThing
{
	private static List<Thing> tmpThings = new List<Thing>();

	private static List<string> tmpThingLabels = new List<string>();

	private static List<Pair<string, int>> tmpThingCounts = new List<Pair<string, int>>();

	public static Vector3 TrueCenter(this Thing t)
	{
		if (t is Pawn pawn)
		{
			return pawn.Drawer.DrawPos;
		}
		if (t.def.category == ThingCategory.Item && t.Spawned)
		{
			return ItemCenterAt(t);
		}
		return TrueCenter(t.Position, t.Rotation, t.def.size, t.def.Altitude);
	}

	public static Vector3 TrueCenter(IntVec3 loc, Rot4 rotation, IntVec2 thingSize, float altitude)
	{
		Vector3 result = loc.ToVector3ShiftedWithAltitude(altitude);
		if (thingSize.x != 1 || thingSize.z != 1)
		{
			if (rotation.IsHorizontal)
			{
				ref int x = ref thingSize.x;
				ref int z = ref thingSize.z;
				int z2 = thingSize.z;
				int x2 = thingSize.x;
				x = z2;
				z = x2;
			}
			switch (rotation.AsInt)
			{
			case 0:
				if (thingSize.x % 2 == 0)
				{
					result.x += 0.5f;
				}
				if (thingSize.z % 2 == 0)
				{
					result.z += 0.5f;
				}
				break;
			case 1:
				if (thingSize.x % 2 == 0)
				{
					result.x += 0.5f;
				}
				if (thingSize.z % 2 == 0)
				{
					result.z -= 0.5f;
				}
				break;
			case 2:
				if (thingSize.x % 2 == 0)
				{
					result.x -= 0.5f;
				}
				if (thingSize.z % 2 == 0)
				{
					result.z -= 0.5f;
				}
				break;
			case 3:
				if (thingSize.x % 2 == 0)
				{
					result.x -= 0.5f;
				}
				if (thingSize.z % 2 == 0)
				{
					result.z += 0.5f;
				}
				break;
			}
		}
		return result;
	}

	private static Vector3 ItemCenterAt(Thing thing)
	{
		IntVec3 position = thing.Position;
		int num = 0;
		int itemsWithLowerID = 0;
		bool flag = false;
		bool flag2 = true;
		ThingDef thingDef = null;
		List<Thing> thingList = position.GetThingList(thing.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing2 = thingList[i];
			if (thing2.def.category == ThingCategory.Item)
			{
				if (thingDef == null)
				{
					thingDef = thing2.def;
				}
				num++;
				if (thing2.def.IsWeapon && thing2.def != ThingDefOf.WoodLog)
				{
					flag = true;
				}
				if (thing2.thingIDNumber < thing.thingIDNumber)
				{
					itemsWithLowerID++;
				}
				if (thing2.def != thingDef)
				{
					flag2 = false;
				}
			}
		}
		float num2 = (float)itemsWithLowerID * 0.03658537f / 10f;
		if (num <= 1)
		{
			Vector3 vector = position.ToVector3Shifted();
			return new Vector3(vector.x, thing.def.Altitude, vector.z);
		}
		if (flag)
		{
			Vector3 vector2 = position.ToVector3Shifted();
			float num3 = 1f / (float)num;
			int num4 = GetAdjacencyOffset(position + IntVec3.West);
			return new Vector3(vector2.x - 0.5f + num3 * ((float)itemsWithLowerID + 0.5f), thing.def.Altitude + num2, vector2.z + ((num4 % 2 == 0) ? (-0.02f) : 0.2f));
		}
		if (flag2)
		{
			Vector3 vector3 = position.ToVector3Shifted();
			return new Vector3(vector3.x + (float)itemsWithLowerID * 0.11f - 0.08f, thing.def.Altitude + num2, vector3.z + (float)itemsWithLowerID * 0.24f - 0.05f);
		}
		Vector3 vector4 = position.ToVector3Shifted();
		Vector2 vector5 = GenGeo.RegularPolygonVertexPosition(num, itemsWithLowerID, ((position.x + position.z) % 2 == 0) ? 0f : 60f) * 0.3f;
		return new Vector3(vector5.x + vector4.x, thing.def.Altitude + num2, vector5.y + vector4.z);
		int GetAdjacencyOffset(IntVec3 x)
		{
			int num5 = itemsWithLowerID;
			while (x.InBounds(thing.Map))
			{
				int itemCount = x.GetItemCount(thing.Map);
				if (itemCount <= 1)
				{
					break;
				}
				if (itemCount % 2 == 1)
				{
					num5++;
				}
				x += IntVec3.West;
			}
			return num5;
		}
	}

	public static void TryDirtyAdjacentGroupContainers(ISlotGroupParent parent, Map map)
	{
		SlotGroup group;
		for (IntVec3 intVec = parent.Position + IntVec3.East; intVec.InBounds(map) && intVec.TryGetSlotGroup(map, out group); intVec += IntVec3.East)
		{
			if (group.parent == null)
			{
				break;
			}
			map.mapDrawer.MapMeshDirty(intVec, MapMeshFlagDefOf.Things);
		}
	}

	public static bool TryDropAndSetForbidden(Thing th, IntVec3 pos, Map map, ThingPlaceMode mode, out Thing resultingThing, bool forbidden)
	{
		if (GenDrop.TryDropSpawn(th, pos, map, ThingPlaceMode.Near, out resultingThing))
		{
			if (resultingThing != null)
			{
				resultingThing.SetForbidden(forbidden, warnOnFail: false);
			}
			return true;
		}
		resultingThing = null;
		return false;
	}

	public static string ThingsToCommaList(IList<Thing> things, bool useAnd = false, bool aggregate = true, int maxCount = -1)
	{
		tmpThings.Clear();
		tmpThingLabels.Clear();
		tmpThingCounts.Clear();
		tmpThings.AddRange(things);
		if (tmpThings.Count >= 2)
		{
			tmpThings.SortByDescending((Thing x) => x is Pawn, (Thing x) => x.def.BaseMarketValue * (float)x.stackCount);
		}
		for (int num = 0; num < tmpThings.Count; num++)
		{
			string text = ((tmpThings[num] is Pawn) ? tmpThings[num].LabelShort : tmpThings[num].LabelNoCount);
			bool flag = false;
			if (aggregate)
			{
				for (int num2 = 0; num2 < tmpThingCounts.Count; num2++)
				{
					if (tmpThingCounts[num2].First == text)
					{
						tmpThingCounts[num2] = new Pair<string, int>(tmpThingCounts[num2].First, tmpThingCounts[num2].Second + tmpThings[num].stackCount);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				tmpThingCounts.Add(new Pair<string, int>(text, tmpThings[num].stackCount));
			}
		}
		tmpThings.Clear();
		bool flag2 = false;
		int num3 = tmpThingCounts.Count;
		if (maxCount >= 0 && num3 > maxCount)
		{
			num3 = maxCount;
			flag2 = true;
		}
		for (int num4 = 0; num4 < num3; num4++)
		{
			string text2 = tmpThingCounts[num4].First;
			if (tmpThingCounts[num4].Second != 1)
			{
				text2 = text2 + " x" + tmpThingCounts[num4].Second;
			}
			tmpThingLabels.Add(text2);
		}
		string text3 = tmpThingLabels.ToCommaList(useAnd && !flag2);
		if (flag2)
		{
			text3 += "...";
		}
		return text3;
	}

	public static float GetMarketValue(IList<Thing> things)
	{
		float num = 0f;
		for (int i = 0; i < things.Count; i++)
		{
			num += things[i].MarketValue * (float)things[i].stackCount;
		}
		return num;
	}

	public static bool CloserThingBetween(ThingDef thingDef, IntVec3 a, IntVec3 b, Map map, Thing thingToIgnore = null)
	{
		foreach (IntVec3 item in CellRect.FromLimits(a, b))
		{
			if (item == a || item == b || !item.InBounds(map))
			{
				continue;
			}
			foreach (Thing thing in item.GetThingList(map))
			{
				if ((thingToIgnore == null || thingToIgnore != thing) && (thing.def == thingDef || thing.def.entityDefToBuild == thingDef))
				{
					return true;
				}
			}
		}
		return false;
	}
}
