using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenThing
	{
		private static List<Thing> tmpThings = new List<Thing>();

		private static List<string> tmpThingLabels = new List<string>();

		private static List<Pair<string, int>> tmpThingCounts = new List<Pair<string, int>>();

		public static Vector3 TrueCenter(this Thing t)
		{
			return (t as Pawn)?.Drawer.DrawPos ?? TrueCenter(t.Position, t.Rotation, t.def.size, t.def.Altitude);
		}

		public static Vector3 TrueCenter(IntVec3 loc, Rot4 rotation, IntVec2 thingSize, float altitude)
		{
			Vector3 result = loc.ToVector3ShiftedWithAltitude(altitude);
			if (thingSize.x != 1 || thingSize.z != 1)
			{
				if (rotation.IsHorizontal)
				{
					int x = thingSize.x;
					thingSize.x = thingSize.z;
					thingSize.z = x;
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

		public static bool TryDropAndSetForbidden(Thing th, IntVec3 pos, Map map, ThingPlaceMode mode, out Thing resultingThing, bool forbidden)
		{
			if (GenDrop.TryDropSpawn_NewTmp(th, pos, map, ThingPlaceMode.Near, out resultingThing))
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
			for (int i = 0; i < tmpThings.Count; i++)
			{
				string text = (tmpThings[i] is Pawn) ? tmpThings[i].LabelShort : tmpThings[i].LabelNoCount;
				bool flag = false;
				if (aggregate)
				{
					for (int j = 0; j < tmpThingCounts.Count; j++)
					{
						if (tmpThingCounts[j].First == text)
						{
							tmpThingCounts[j] = new Pair<string, int>(tmpThingCounts[j].First, tmpThingCounts[j].Second + tmpThings[i].stackCount);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					tmpThingCounts.Add(new Pair<string, int>(text, tmpThings[i].stackCount));
				}
			}
			tmpThings.Clear();
			bool flag2 = false;
			int num = tmpThingCounts.Count;
			if (maxCount >= 0 && num > maxCount)
			{
				num = maxCount;
				flag2 = true;
			}
			for (int k = 0; k < num; k++)
			{
				string text2 = tmpThingCounts[k].First;
				if (tmpThingCounts[k].Second != 1)
				{
					text2 = text2 + " x" + tmpThingCounts[k].Second;
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
}
