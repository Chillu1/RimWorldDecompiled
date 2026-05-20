using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class DebugSolidColorMats
{
	private static Dictionary<Color, Material> colorMatDict = new Dictionary<Color, Material>();

	public static Material MaterialOf(Color col)
	{
		if (colorMatDict.TryGetValue(col, out var value))
		{
			return value;
		}
		value = SolidColorMaterials.SimpleSolidColorMaterial(col);
		colorMatDict.Add(col, value);
		return value;
	}
}
