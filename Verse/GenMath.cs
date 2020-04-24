using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class GenMath
	{
		public struct BezierCubicControls
		{
			public Vector3 w0;

			public Vector3 w1;

			public Vector3 w2;

			public Vector3 w3;
		}

		public const float BigEpsilon = 1E-07f;

		public const float Sqrt2 = 1.41421354f;

		private static List<float> tmpElements = new List<float>();

		private static List<Pair<float, float>> tmpPairs = new List<Pair<float, float>>();

		private static List<float> tmpScores = new List<float>();

		private static List<float> tmpCalcList = new List<float>();

		public static float RoundedHundredth(float f)
		{
			return Mathf.Round(f * 100f) / 100f;
		}

		public static int RoundTo(int value, int roundToNearest)
		{
			return (int)Math.Round((float)value / (float)roundToNearest) * roundToNearest;
		}

		public static float RoundTo(float value, float roundToNearest)
		{
			return (float)(int)Math.Round(value / roundToNearest) * roundToNearest;
		}

		public static float ChanceEitherHappens(float chanceA, float chanceB)
		{
			return chanceA + (1f - chanceA) * chanceB;
		}

		public static float SmootherStep(float edge0, float edge1, float x)
		{
			x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
			return x * x * x * (x * (x * 6f - 15f) + 10f);
		}

		public static int RoundRandom(float f)
		{
			return (int)f + ((Rand.Value < f % 1f) ? 1 : 0);
		}

		public static float WeightedAverage(float A, float weightA, float B, float weightB)
		{
			return (A * weightA + B * weightB) / (weightA + weightB);
		}

		public static float Median<T>(IList<T> list, Func<T, float> orderBy, float noneValue = 0f, float center = 0.5f)
		{
			if (list.NullOrEmpty())
			{
				return noneValue;
			}
			tmpElements.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				tmpElements.Add(orderBy(list[i]));
			}
			tmpElements.Sort();
			return tmpElements[Mathf.Min(Mathf.FloorToInt((float)tmpElements.Count * center), tmpElements.Count - 1)];
		}

		public static float WeightedMedian(IList<Pair<float, float>> list, float noneValue = 0f, float center = 0.5f)
		{
			tmpPairs.Clear();
			tmpPairs.AddRange(list);
			float num = 0f;
			for (int i = 0; i < tmpPairs.Count; i++)
			{
				float second = tmpPairs[i].Second;
				if (second < 0f)
				{
					Log.ErrorOnce("Negative weight in WeightedMedian: " + second, tmpPairs.GetHashCode());
				}
				else
				{
					num += second;
				}
			}
			if (num <= 0f)
			{
				return noneValue;
			}
			tmpPairs.SortBy((Pair<float, float> x) => x.First);
			float num2 = 0f;
			for (int j = 0; j < tmpPairs.Count; j++)
			{
				float first = tmpPairs[j].First;
				float second2 = tmpPairs[j].Second;
				num2 += second2 / num;
				if (num2 >= center)
				{
					return first;
				}
			}
			return tmpPairs.Last().First;
		}

		public static float Sqrt(float f)
		{
			return (float)Math.Sqrt(f);
		}

		public static float LerpDouble(float inFrom, float inTo, float outFrom, float outTo, float x)
		{
			float num = (x - inFrom) / (inTo - inFrom);
			return outFrom + (outTo - outFrom) * num;
		}

		public static float LerpDoubleClamped(float inFrom, float inTo, float outFrom, float outTo, float x)
		{
			return LerpDouble(inFrom, inTo, outFrom, outTo, Mathf.Clamp(x, Mathf.Min(inFrom, inTo), Mathf.Max(inFrom, inTo)));
		}

		public static float Reflection(float value, float mirror)
		{
			return mirror - (value - mirror);
		}

		public static Quaternion ToQuat(this float ang)
		{
			return Quaternion.AngleAxis(ang, Vector3.up);
		}

		public static float GetFactorInInterval(float min, float mid, float max, float power, float x)
		{
			if (min > max)
			{
				return 0f;
			}
			if (x <= min || x >= max)
			{
				return 0f;
			}
			if (x == mid)
			{
				return 1f;
			}
			float num = 0f;
			num = ((!(x < mid)) ? (1f - (x - mid) / (max - mid)) : (1f - (mid - x) / (mid - min)));
			return Mathf.Pow(num, power);
		}

		public static float FlatHill(float min, float lower, float upper, float max, float x)
		{
			if (x < min)
			{
				return 0f;
			}
			if (x < lower)
			{
				return Mathf.InverseLerp(min, lower, x);
			}
			if (x < upper)
			{
				return 1f;
			}
			if (x < max)
			{
				return Mathf.InverseLerp(max, upper, x);
			}
			return 0f;
		}

		public static float FlatHill(float minY, float min, float lower, float upper, float max, float maxY, float x)
		{
			if (x < min)
			{
				return minY;
			}
			if (x < lower)
			{
				return LerpDouble(min, lower, minY, 1f, x);
			}
			if (x < upper)
			{
				return 1f;
			}
			if (x < max)
			{
				return LerpDouble(upper, max, 1f, maxY, x);
			}
			return maxY;
		}

		public static int OctileDistance(int dx, int dz, int cardinal, int diagonal)
		{
			return cardinal * (dx + dz) + (diagonal - 2 * cardinal) * Mathf.Min(dx, dz);
		}

		public static float UnboundedValueToFactor(float val)
		{
			if (val > 0f)
			{
				return 1f + val;
			}
			return 1f / (1f - val);
		}

		[DebugOutput("System", false)]
		public static void TestMathPerf()
		{
			IntVec3 intVec = new IntVec3(72, 0, 65);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Math perf tests (" + 1E+07f + " tests each)");
			float num = 0f;
			Stopwatch stopwatch = Stopwatch.StartNew();
			for (int i = 0; (float)i < 1E+07f; i++)
			{
				num += (float)Math.Sqrt(101.20999908447266);
			}
			stringBuilder.AppendLine("(float)System.Math.Sqrt(" + 101.21f + "): " + stopwatch.ElapsedTicks);
			Stopwatch stopwatch2 = Stopwatch.StartNew();
			for (int j = 0; (float)j < 1E+07f; j++)
			{
				num += Mathf.Sqrt(101.21f);
			}
			stringBuilder.AppendLine("UnityEngine.Mathf.Sqrt(" + 101.21f + "): " + stopwatch2.ElapsedTicks);
			Stopwatch stopwatch3 = Stopwatch.StartNew();
			for (int k = 0; (float)k < 1E+07f; k++)
			{
				num += Sqrt(101.21f);
			}
			stringBuilder.AppendLine("Verse.GenMath.Sqrt(" + 101.21f + "): " + stopwatch3.ElapsedTicks);
			Stopwatch stopwatch4 = Stopwatch.StartNew();
			for (int l = 0; (float)l < 1E+07f; l++)
			{
				num += (float)intVec.LengthManhattan;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthManhattan: " + stopwatch4.ElapsedTicks);
			Stopwatch stopwatch5 = Stopwatch.StartNew();
			for (int m = 0; (float)m < 1E+07f; m++)
			{
				num += intVec.LengthHorizontal;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthHorizontal: " + stopwatch5.ElapsedTicks);
			Stopwatch stopwatch6 = Stopwatch.StartNew();
			for (int n = 0; (float)n < 1E+07f; n++)
			{
				num += (float)intVec.LengthHorizontalSquared;
			}
			stringBuilder.AppendLine("Verse.IntVec3.LengthHorizontalSquared: " + stopwatch6.ElapsedTicks);
			stringBuilder.AppendLine("total: " + num);
			Log.Message(stringBuilder.ToString());
		}

		public static float Min(float a, float b, float c)
		{
			if (a < b)
			{
				if (a < c)
				{
					return a;
				}
				return c;
			}
			if (b < c)
			{
				return b;
			}
			return c;
		}

		public static int Max(int a, int b, int c)
		{
			if (a > b)
			{
				if (a > c)
				{
					return a;
				}
				return c;
			}
			if (b > c)
			{
				return b;
			}
			return c;
		}

		public static float SphericalDistance(Vector3 normalizedA, Vector3 normalizedB)
		{
			if (normalizedA == normalizedB)
			{
				return 0f;
			}
			return Mathf.Acos(Vector3.Dot(normalizedA, normalizedB));
		}

		public static void DHondtDistribution(List<int> candidates, Func<int, float> scoreGetter, int numToDistribute)
		{
			tmpScores.Clear();
			tmpCalcList.Clear();
			for (int i = 0; i < candidates.Count; i++)
			{
				float item = scoreGetter(i);
				candidates[i] = 0;
				tmpScores.Add(item);
				tmpCalcList.Add(item);
			}
			for (int j = 0; j < numToDistribute; j++)
			{
				int index = tmpCalcList.IndexOf(tmpCalcList.Max());
				candidates[index]++;
				tmpCalcList[index] = tmpScores[index] / ((float)candidates[index] + 1f);
			}
		}

		public static int PositiveMod(int x, int m)
		{
			return (x % m + m) % m;
		}

		public static long PositiveMod(long x, long m)
		{
			return (x % m + m) % m;
		}

		public static float PositiveMod(float x, float m)
		{
			return (x % m + m) % m;
		}

		public static int PositiveModRemap(long x, int d, int m)
		{
			if (x < 0)
			{
				x -= d - 1;
			}
			return (int)((x / d % m + m) % m);
		}

		public static Vector3 BezierCubicEvaluate(float t, BezierCubicControls bcc)
		{
			return BezierCubicEvaluate(t, bcc.w0, bcc.w1, bcc.w2, bcc.w3);
		}

		public static Vector3 BezierCubicEvaluate(float t, Vector3 w0, Vector3 w1, Vector3 w2, Vector3 w3)
		{
			float d = t * t;
			float num = 1f - t;
			float d2 = num * num;
			return w0 * d2 * num + 3f * w1 * d2 * t + 3f * w2 * num * d + w3 * d * t;
		}

		public static float CirclesOverlapArea(float x1, float y1, float r1, float x2, float y2, float r2)
		{
			float num = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
			float num2 = Mathf.Sqrt(num);
			float num3 = r1 * r1;
			float num4 = r2 * r2;
			float num5 = Mathf.Abs(r1 - r2);
			if (num2 >= r1 + r2)
			{
				return 0f;
			}
			if (num2 <= num5 && r1 >= r2)
			{
				return (float)Math.PI * num4;
			}
			if (num2 <= num5 && r2 >= r1)
			{
				return (float)Math.PI * num3;
			}
			float num6 = Mathf.Acos((num3 - num4 + num) / (2f * r1 * num2)) * 2f;
			float num7 = Mathf.Acos((num4 - num3 + num) / (2f * r2 * num2)) * 2f;
			float num8 = (num7 * num4 - num4 * Mathf.Sin(num7)) * 0.5f;
			float num9 = (num6 * num3 - num3 * Mathf.Sin(num6)) * 0.5f;
			return num8 + num9;
		}

		public static bool AnyIntegerInRange(float min, float max)
		{
			return Mathf.Ceil(min) <= max;
		}

		public static void NormalizeToSum1(ref float a, ref float b, ref float c)
		{
			float num = a + b + c;
			if (num == 0f)
			{
				a = 1f;
				b = 0f;
				c = 0f;
			}
			else
			{
				a /= num;
				b /= num;
				c /= num;
			}
		}

		public static float InverseLerp(float a, float b, float value)
		{
			if (a == b)
			{
				if (!(value < a))
				{
					return 1f;
				}
				return 0f;
			}
			return Mathf.InverseLerp(a, b, value);
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3)
		{
			if (by1 >= by2 && by1 >= by3)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3)
			{
				return elem2;
			}
			return elem3;
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4)
		{
			if (by1 >= by2 && by1 >= by3 && by1 >= by4)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3 && by2 >= by4)
			{
				return elem2;
			}
			if (by3 >= by1 && by3 >= by2 && by3 >= by4)
			{
				return elem3;
			}
			return elem4;
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5)
		{
			if (by1 >= by2 && by1 >= by3 && by1 >= by4 && by1 >= by5)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3 && by2 >= by4 && by2 >= by5)
			{
				return elem2;
			}
			if (by3 >= by1 && by3 >= by2 && by3 >= by4 && by3 >= by5)
			{
				return elem3;
			}
			if (by4 >= by1 && by4 >= by2 && by4 >= by3 && by4 >= by5)
			{
				return elem4;
			}
			return elem5;
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6)
		{
			if (by1 >= by2 && by1 >= by3 && by1 >= by4 && by1 >= by5 && by1 >= by6)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3 && by2 >= by4 && by2 >= by5 && by2 >= by6)
			{
				return elem2;
			}
			if (by3 >= by1 && by3 >= by2 && by3 >= by4 && by3 >= by5 && by3 >= by6)
			{
				return elem3;
			}
			if (by4 >= by1 && by4 >= by2 && by4 >= by3 && by4 >= by5 && by4 >= by6)
			{
				return elem4;
			}
			if (by5 >= by1 && by5 >= by2 && by5 >= by3 && by5 >= by4 && by5 >= by6)
			{
				return elem5;
			}
			return elem6;
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6, T elem7, float by7)
		{
			if (by1 >= by2 && by1 >= by3 && by1 >= by4 && by1 >= by5 && by1 >= by6 && by1 >= by7)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3 && by2 >= by4 && by2 >= by5 && by2 >= by6 && by2 >= by7)
			{
				return elem2;
			}
			if (by3 >= by1 && by3 >= by2 && by3 >= by4 && by3 >= by5 && by3 >= by6 && by3 >= by7)
			{
				return elem3;
			}
			if (by4 >= by1 && by4 >= by2 && by4 >= by3 && by4 >= by5 && by4 >= by6 && by4 >= by7)
			{
				return elem4;
			}
			if (by5 >= by1 && by5 >= by2 && by5 >= by3 && by5 >= by4 && by5 >= by6 && by5 >= by7)
			{
				return elem5;
			}
			if (by6 >= by1 && by6 >= by2 && by6 >= by3 && by6 >= by4 && by6 >= by5 && by6 >= by7)
			{
				return elem6;
			}
			return elem7;
		}

		public static T MaxBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6, T elem7, float by7, T elem8, float by8)
		{
			if (by1 >= by2 && by1 >= by3 && by1 >= by4 && by1 >= by5 && by1 >= by6 && by1 >= by7 && by1 >= by8)
			{
				return elem1;
			}
			if (by2 >= by1 && by2 >= by3 && by2 >= by4 && by2 >= by5 && by2 >= by6 && by2 >= by7 && by2 >= by8)
			{
				return elem2;
			}
			if (by3 >= by1 && by3 >= by2 && by3 >= by4 && by3 >= by5 && by3 >= by6 && by3 >= by7 && by3 >= by8)
			{
				return elem3;
			}
			if (by4 >= by1 && by4 >= by2 && by4 >= by3 && by4 >= by5 && by4 >= by6 && by4 >= by7 && by4 >= by8)
			{
				return elem4;
			}
			if (by5 >= by1 && by5 >= by2 && by5 >= by3 && by5 >= by4 && by5 >= by6 && by5 >= by7 && by5 >= by8)
			{
				return elem5;
			}
			if (by6 >= by1 && by6 >= by2 && by6 >= by3 && by6 >= by4 && by6 >= by5 && by6 >= by7 && by6 >= by8)
			{
				return elem6;
			}
			if (by7 >= by1 && by7 >= by2 && by7 >= by3 && by7 >= by4 && by7 >= by5 && by7 >= by6 && by7 >= by8)
			{
				return elem7;
			}
			return elem8;
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3);
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3, elem4, 0f - by4);
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3, elem4, 0f - by4, elem5, 0f - by5);
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3, elem4, 0f - by4, elem5, 0f - by5, elem6, 0f - by6);
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6, T elem7, float by7)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3, elem4, 0f - by4, elem5, 0f - by5, elem6, 0f - by6, elem7, 0f - by7);
		}

		public static T MinBy<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6, T elem7, float by7, T elem8, float by8)
		{
			return MaxBy(elem1, 0f - by1, elem2, 0f - by2, elem3, 0f - by3, elem4, 0f - by4, elem5, 0f - by5, elem6, 0f - by6, elem7, 0f - by7, elem8, 0f - by8);
		}

		public static T MaxByRandomIfEqual<T>(T elem1, float by1, T elem2, float by2, T elem3, float by3, T elem4, float by4, T elem5, float by5, T elem6, float by6, T elem7, float by7, T elem8, float by8, float eps = 0.0001f)
		{
			return MaxBy(elem1, by1 + Rand.Range(0f, eps), elem2, by2 + Rand.Range(0f, eps), elem3, by3 + Rand.Range(0f, eps), elem4, by4 + Rand.Range(0f, eps), elem5, by5 + Rand.Range(0f, eps), elem6, by6 + Rand.Range(0f, eps), elem7, by7 + Rand.Range(0f, eps), elem8, by8 + Rand.Range(0f, eps));
		}

		public static float Stddev(IEnumerable<float> data)
		{
			int num = 0;
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (float datum in data)
			{
				num++;
				num2 += (double)datum;
				num3 += (double)(datum * datum);
			}
			double num4 = num2 / (double)num;
			return Mathf.Sqrt((float)(num3 / (double)num - num4 * num4));
		}
	}
}
