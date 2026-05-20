using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class ShadowMeshPool
{
	private static Dictionary<int, Mesh> shadowMeshDict = new Dictionary<int, Mesh>();

	public static Mesh GetShadowMesh(ShadowData sha)
	{
		return GetShadowMesh(sha.BaseX, sha.BaseZ, sha.BaseY);
	}

	public static Mesh GetShadowMesh(float baseEdgeLength, float tallness)
	{
		return GetShadowMesh(baseEdgeLength, baseEdgeLength, tallness);
	}

	public static Mesh GetShadowMesh(float baseWidth, float baseHeight, float tallness)
	{
		int key = HashOf(baseWidth, baseHeight, tallness);
		if (!shadowMeshDict.TryGetValue(key, out var value))
		{
			value = MeshMakerShadows.NewShadowMesh(baseWidth, baseHeight, tallness);
			shadowMeshDict.Add(key, value);
		}
		return value;
	}

	private static int HashOf(float baseWidth, float baseheight, float tallness)
	{
		int num = (int)(baseWidth * 1000f);
		int num2 = (int)(baseheight * 1000f);
		int num3 = (int)(tallness * 1000f);
		return (num * 391) ^ 0x3FC6F ^ (num2 * 612331) ^ (num3 * 456123);
	}
}
