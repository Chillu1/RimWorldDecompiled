using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Verse;

public static class FadedMaterialPool
{
	private struct FadedMatRequest : IEquatable<FadedMatRequest>
	{
		private Material mat;

		private int alphaIndex;

		public FadedMatRequest(Material mat, int alphaIndex)
		{
			this.mat = mat;
			this.alphaIndex = alphaIndex;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is FadedMatRequest)
			{
				return Equals((FadedMatRequest)obj);
			}
			return false;
		}

		public bool Equals(FadedMatRequest other)
		{
			if (mat == other.mat)
			{
				return alphaIndex == other.alphaIndex;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(mat.GetHashCode(), alphaIndex);
		}

		public static bool operator ==(FadedMatRequest lhs, FadedMatRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(FadedMatRequest lhs, FadedMatRequest rhs)
		{
			return !(lhs == rhs);
		}
	}

	private class FadedMatRequestComparer : IEqualityComparer<FadedMatRequest>
	{
		public static readonly FadedMatRequestComparer Instance = new FadedMatRequestComparer();

		public bool Equals(FadedMatRequest x, FadedMatRequest y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(FadedMatRequest obj)
		{
			return obj.GetHashCode();
		}
	}

	private static Dictionary<FadedMatRequest, Material> cachedMats = new Dictionary<FadedMatRequest, Material>(FadedMatRequestComparer.Instance);

	private const int NumFadeSteps = 30;

	public static int TotalMaterialCount => cachedMats.Count;

	public static long TotalMaterialBytes
	{
		get
		{
			long num = 0L;
			foreach (KeyValuePair<FadedMatRequest, Material> cachedMat in cachedMats)
			{
				num += Profiler.GetRuntimeMemorySizeLong(cachedMat.Value);
			}
			return num;
		}
	}

	public static Material FadedVersionOf(Material sourceMat, float alpha)
	{
		int num = IndexFromAlpha(alpha);
		switch (num)
		{
		case 0:
			return BaseContent.ClearMat;
		case 29:
			return sourceMat;
		default:
		{
			FadedMatRequest key = new FadedMatRequest(sourceMat, num);
			if (!cachedMats.TryGetValue(key, out var value))
			{
				value = MaterialAllocator.Create(sourceMat);
				value.color = new Color(1f, 1f, 1f, (float)IndexFromAlpha(alpha) / 30f);
				cachedMats.Add(key, value);
			}
			return value;
		}
		}
	}

	private static int IndexFromAlpha(float alpha)
	{
		int num = Mathf.FloorToInt(alpha * 30f);
		if (num == 30)
		{
			num = 29;
		}
		return num;
	}
}
