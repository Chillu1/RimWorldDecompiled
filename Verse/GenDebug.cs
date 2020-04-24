using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenDebug
	{
		public static void DebugPlaceSphere(Vector3 Loc, float Scale)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.transform.position = Loc;
			gameObject.transform.localScale = new Vector3(Scale, Scale, Scale);
		}

		public static void LogList<T>(IEnumerable<T> list)
		{
			foreach (T item in list)
			{
				Log.Message("    " + item.ToString());
			}
		}

		public static void ClearArea(CellRect r, Map map)
		{
			r.ClipInsideMap(map);
			foreach (IntVec3 item in r)
			{
				map.roofGrid.SetRoof(item, null);
			}
			foreach (IntVec3 item2 in r)
			{
				foreach (Thing item3 in item2.GetThingList(map).ToList())
				{
					if (item3.def.destroyable)
					{
						item3.Destroy();
					}
				}
			}
		}
	}
}
