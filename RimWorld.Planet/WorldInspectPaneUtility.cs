using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldInspectPaneUtility
	{
		public static string AdjustedLabelFor(List<WorldObject> worldObjects, Rect rect)
		{
			if (worldObjects.Count == 1)
			{
				return worldObjects[0].LabelCap;
			}
			if (AllLabelsAreSame(worldObjects))
			{
				return worldObjects[0].LabelCap + " x" + worldObjects.Count.ToStringCached();
			}
			return "VariousLabel".Translate();
		}

		private static bool AllLabelsAreSame(List<WorldObject> worldObjects)
		{
			if (worldObjects.Count <= 1)
			{
				return true;
			}
			string labelCap = worldObjects[0].LabelCap;
			for (int i = 1; i < worldObjects.Count; i++)
			{
				if (worldObjects[i].LabelCap != labelCap)
				{
					return false;
				}
			}
			return true;
		}
	}
}
