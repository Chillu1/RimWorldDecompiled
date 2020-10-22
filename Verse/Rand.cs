using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public static class Rand
	{
		private static uint seed;

		private static uint iterations;

		private static Stack<ulong> stateStack;

		private static List<int> tmpRange;

		public static int Seed
		{
			set
			{
				if (stateStack.Count == 0)
				{
					Log.ErrorOnce("Modifying the initial rand seed. Call PushState() first. The initial rand seed should always be based on the startup time and set only once.", 825343540);
				}
				seed = (uint)value;
				iterations = 0u;
			}
		}

		public static float Value => (float)(((double)MurmurHash.GetInt(seed, iterations++) - -2147483648.0) / 4294967295.0);

		public static bool Bool => Value < 0.5f;

		public static int Sign
		{
			get
			{
				if (!Bool)
				{
					return -1;
				}
				return 1;
			}
		}

		public static int Int => MurmurHash.GetInt(seed, iterations++);

		public static Vector3 UnitVector3 => new Vector3(Gaussian(), Gaussian(), Gaussian()).normalized;

		public static Vector2 UnitVector2 => new Vector2(Gaussian(), Gaussian()).normalized;

		public static Vector2 InsideUnitCircle
		{
			get
			{
				Vector2 result;
				do
				{
					result = new Vector2(Value - 0.5f, Value - 0.5f) * 2f;
				}
				while (!(result.sqrMagnitude <= 1f));
				return result;
			}
		}

		public static Vector3 InsideUnitCircleVec3
		{
			get
			{
				Vector2 insideUnitCircle = InsideUnitCircle;
				return new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
			}
		}

		private static ulong StateCompressed
		{
			get
			{
				return seed | ((ulong)iterations << 32);
			}
			set
			{
				seed = (uint)(value & 0xFFFFFFFFu);
				iterations = (uint)((value >> 32) & 0xFFFFFFFFu);
			}
		}

		static Rand()
		{
			iterations = 0u;
			stateStack = new Stack<ulong>();
			tmpRange = new List<int>();
			seed = (uint)DateTime.Now.GetHashCode();
		}

		public static void EnsureStateStackEmpty()
		{
			if (stateStack.Count > 0)
			{
				Log.Warning("Random state stack is not empty. There were more calls to PushState than PopState. Fixing.");
				while (stateStack.Any())
				{
					PopState();
				}
			}
		}

		public static float Gaussian(float centerX = 0f, float widthFactor = 1f)
		{
			float value = Value;
			float value2 = Value;
			return Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin((float)Math.PI * 2f * value2) * widthFactor + centerX;
		}

		public static float GaussianAsymmetric(float centerX = 0f, float lowerWidthFactor = 1f, float upperWidthFactor = 1f)
		{
			float value = Value;
			float value2 = Value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin((float)Math.PI * 2f * value2);
			if (num <= 0f)
			{
				return num * lowerWidthFactor + centerX;
			}
			return num * upperWidthFactor + centerX;
		}

		public static int Range(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return min + Mathf.Abs(Int % (max - min));
		}

		public static int RangeInclusive(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return Range(min, max + 1);
		}

		public static float Range(float min, float max)
		{
			if (max <= min)
			{
				return min;
			}
			return Value * (max - min) + min;
		}

		public static bool Chance(float chance)
		{
			if (chance <= 0f)
			{
				return false;
			}
			if (chance >= 1f)
			{
				return true;
			}
			return Value < chance;
		}

		public static bool ChanceSeeded(float chance, int specialSeed)
		{
			PushState(specialSeed);
			bool result = Chance(chance);
			PopState();
			return result;
		}

		public static float ValueSeeded(int specialSeed)
		{
			PushState(specialSeed);
			float value = Value;
			PopState();
			return value;
		}

		public static float RangeSeeded(float min, float max, int specialSeed)
		{
			PushState(specialSeed);
			float result = Range(min, max);
			PopState();
			return result;
		}

		public static int RangeSeeded(int min, int max, int specialSeed)
		{
			PushState(specialSeed);
			int result = Range(min, max);
			PopState();
			return result;
		}

		public static int RangeInclusiveSeeded(int min, int max, int specialSeed)
		{
			PushState(specialSeed);
			int result = RangeInclusive(min, max);
			PopState();
			return result;
		}

		public static T Element<T>(T a, T b)
		{
			if (!Bool)
			{
				return b;
			}
			return a;
		}

		public static T Element<T>(T a, T b, T c)
		{
			float value = Value;
			if (value < 0.33333f)
			{
				return a;
			}
			if (value < 0.66666f)
			{
				return b;
			}
			return c;
		}

		public static T Element<T>(T a, T b, T c, T d)
		{
			float value = Value;
			if (value < 0.25f)
			{
				return a;
			}
			if (value < 0.5f)
			{
				return b;
			}
			if (value < 0.75f)
			{
				return c;
			}
			return d;
		}

		public static T Element<T>(T a, T b, T c, T d, T e)
		{
			float value = Value;
			if (value < 0.2f)
			{
				return a;
			}
			if (value < 0.4f)
			{
				return b;
			}
			if (value < 0.6f)
			{
				return c;
			}
			if (value < 0.8f)
			{
				return d;
			}
			return e;
		}

		public static T Element<T>(T a, T b, T c, T d, T e, T f)
		{
			float value = Value;
			if (value < 0.16666f)
			{
				return a;
			}
			if (value < 0.33333f)
			{
				return b;
			}
			if (value < 0.5f)
			{
				return c;
			}
			if (value < 0.66666f)
			{
				return d;
			}
			if (value < 0.83333f)
			{
				return e;
			}
			return f;
		}

		public static T ElementByWeight<T>(T a, float weightA, T b, float weightB)
		{
			float num = weightA + weightB;
			if (Value < weightA / num)
			{
				return a;
			}
			return b;
		}

		public static T ElementByWeight<T>(T a, float weightA, T b, float weightB, T c, float weightC)
		{
			float num = weightA + weightB + weightC;
			float value = Value;
			if (value < weightA / num)
			{
				return a;
			}
			if (value < (weightA + weightB) / num)
			{
				return b;
			}
			return c;
		}

		public static T ElementByWeight<T>(T a, float weightA, T b, float weightB, T c, float weightC, T d, float weightD)
		{
			float num = weightA + weightB + weightC + weightD;
			float value = Value;
			if (value < weightA / num)
			{
				return a;
			}
			if (value < (weightA + weightB) / num)
			{
				return b;
			}
			if (value < (weightA + weightB + weightC) / num)
			{
				return c;
			}
			return d;
		}

		public static T ElementByWeight<T>(T a, float weightA, T b, float weightB, T c, float weightC, T d, float weightD, T e, float weightE)
		{
			float num = weightA + weightB + weightC + weightD + weightE;
			float value = Value;
			if (value < weightA / num)
			{
				return a;
			}
			if (value < (weightA + weightB) / num)
			{
				return b;
			}
			if (value < (weightA + weightB + weightC) / num)
			{
				return c;
			}
			if (value < (weightA + weightB + weightC + weightD) / num)
			{
				return d;
			}
			return e;
		}

		public static T ElementByWeight<T>(T a, float weightA, T b, float weightB, T c, float weightC, T d, float weightD, T e, float weightE, T f, float weightF)
		{
			float num = weightA + weightB + weightC + weightD + weightE + weightF;
			float value = Value;
			if (value < weightA / num)
			{
				return a;
			}
			if (value < (weightA + weightB) / num)
			{
				return b;
			}
			if (value < (weightA + weightB + weightC) / num)
			{
				return c;
			}
			if (value < (weightA + weightB + weightC + weightD) / num)
			{
				return d;
			}
			if (value < (weightA + weightB + weightC + weightD + weightE) / num)
			{
				return e;
			}
			return f;
		}

		public static void PushState()
		{
			stateStack.Push(StateCompressed);
		}

		public static void PushState(int replacementSeed)
		{
			PushState();
			Seed = replacementSeed;
		}

		public static void PopState()
		{
			StateCompressed = stateStack.Pop();
		}

		public static float ByCurve(SimpleCurve curve)
		{
			if (curve.PointsCount < 3)
			{
				throw new ArgumentException("curve has < 3 points");
			}
			if (curve[0].y != 0f || curve[curve.PointsCount - 1].y != 0f)
			{
				throw new ArgumentException("curve has start/end point with y != 0");
			}
			float num = 0f;
			for (int i = 0; i < curve.PointsCount - 1; i++)
			{
				if (curve[i].y < 0f)
				{
					throw new ArgumentException("curve has point with y < 0");
				}
				num += (curve[i + 1].x - curve[i].x) * (curve[i].y + curve[i + 1].y);
			}
			float num2 = Range(0f, num);
			for (int j = 0; j < curve.PointsCount - 1; j++)
			{
				float num3 = (curve[j + 1].x - curve[j].x) * (curve[j].y + curve[j + 1].y);
				if (num3 < num2)
				{
					num2 -= num3;
					continue;
				}
				float num4 = curve[j + 1].x - curve[j].x;
				float y = curve[j].y;
				float y2 = curve[j + 1].y;
				float num5 = num2 / (y + y2);
				if (Range(0f, (y + y2) / 2f) > Mathf.Lerp(y, y2, num5 / num4))
				{
					num5 = num4 - num5;
				}
				return num5 + curve[j].x;
			}
			throw new Exception("Reached end of Rand.ByCurve without choosing a point.");
		}

		public static float ByCurveAverage(SimpleCurve curve)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < curve.PointsCount - 1; i++)
			{
				num += (curve[i + 1].x - curve[i].x) * (curve[i].y + curve[i + 1].y);
				num2 += (curve[i + 1].x - curve[i].x) * (curve[i].x * (2f * curve[i].y + curve[i + 1].y) + curve[i + 1].x * (curve[i].y + 2f * curve[i + 1].y));
			}
			return num2 / num / 3f;
		}

		public static bool MTBEventOccurs(float mtb, float mtbUnit, float checkDuration)
		{
			if (mtb == float.PositiveInfinity)
			{
				return false;
			}
			if (mtb <= 0f)
			{
				Log.Error("MTBEventOccurs with mtb=" + mtb);
				return true;
			}
			if (mtbUnit <= 0f)
			{
				Log.Error("MTBEventOccurs with mtbUnit=" + mtbUnit);
				return false;
			}
			if (checkDuration <= 0f)
			{
				Log.Error("MTBEventOccurs with checkDuration=" + checkDuration);
				return false;
			}
			double num = (double)checkDuration / ((double)mtb * (double)mtbUnit);
			if (num <= 0.0)
			{
				Log.Error("chancePerCheck is " + num + ". mtb=" + mtb + ", mtbUnit=" + mtbUnit + ", checkDuration=" + checkDuration);
				return false;
			}
			double num2 = 1.0;
			if (num < 0.0001)
			{
				while (num < 0.0001)
				{
					num *= 8.0;
					num2 /= 8.0;
				}
				if ((double)Value > num2)
				{
					return false;
				}
			}
			return (double)Value < num;
		}

		public static void SplitRandomly(float valueToSplit, int splitIntoCount, List<float> outValues, List<float> zeroIfFractionBelow = null, List<float> minFractions = null)
		{
			outValues.Clear();
			if (splitIntoCount <= 0)
			{
				if (valueToSplit != 0f)
				{
					throw new ArgumentException("splitIntoCount <= 0");
				}
				return;
			}
			if (minFractions != null)
			{
				float num = 0f;
				for (int i = 0; i < minFractions.Count; i++)
				{
					if (minFractions[i] > 1f)
					{
						throw new ArgumentException("minFractions[i] > 1");
					}
					num += minFractions[i];
				}
				if (num > 1f)
				{
					throw new ArgumentException("minFractions sum > 1");
				}
			}
			float num2 = 0f;
			int num3 = 0;
			bool flag;
			do
			{
				num3++;
				if (num3 > 10000)
				{
					Log.Error("Could not meet SplitRandomly requirements. valueToSplit=" + valueToSplit + ", splitIntoCount=" + splitIntoCount + ", zeroIfFractionsBelow=" + zeroIfFractionBelow.ToStringSafeEnumerable() + ", minFractions=" + minFractions.ToStringSafeEnumerable());
					break;
				}
				outValues.Clear();
				for (int j = 0; j < splitIntoCount; j++)
				{
					float num4 = Range(0f, 1f);
					num2 += num4;
					outValues.Add(num4);
				}
				flag = true;
				if (minFractions == null)
				{
					continue;
				}
				for (int k = 0; k < minFractions.Count; k++)
				{
					if (outValues[k] / num2 < minFractions[k])
					{
						flag = false;
						break;
					}
				}
			}
			while (!flag);
			if (zeroIfFractionBelow != null)
			{
				for (int l = 0; l < zeroIfFractionBelow.Count; l++)
				{
					if (outValues[l] / num2 < zeroIfFractionBelow[l])
					{
						outValues[l] = 0f;
						num2 = 0f;
						for (int m = 0; m < outValues.Count; m++)
						{
							num2 += outValues[m];
						}
					}
				}
			}
			if (num2 != 0f)
			{
				for (int n = 0; n < outValues.Count; n++)
				{
					outValues[n] = outValues[n] / num2 * valueToSplit;
				}
			}
		}

		[DebugOutput("System", false)]
		internal static void RandTests()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Random generation tests");
			stringBuilder.Append("To see texture outputs, turn on 'draw recorded noise' and run this again.");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Performance test with " + 10000000 + " values");
			Stopwatch stopwatch = new Stopwatch();
			float num = 0f;
			PushState();
			stopwatch.Start();
			for (int i = 0; i < 10000000; i++)
			{
				num += Value;
			}
			stopwatch.Stop();
			PopState();
			stringBuilder.AppendLine("Time: " + stopwatch.ElapsedMilliseconds.ToString() + "ms (for sum " + num + ")");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Distribution test with " + 100000 + " values");
			DebugHistogram debugHistogram = new DebugHistogram(new float[11]
			{
				0f,
				0.1f,
				0.2f,
				0.3f,
				0.4f,
				0.5f,
				0.6f,
				0.7f,
				0.8f,
				0.9f,
				1f
			});
			DebugHistogram debugHistogram2 = new DebugHistogram(new float[12]
			{
				0f,
				0.01f,
				0.02f,
				0.03f,
				0.04f,
				0.05f,
				0.06f,
				0.07f,
				0.08f,
				0.09f,
				0.1f,
				1f
			});
			PushState();
			for (int j = 0; j < 100000; j++)
			{
				debugHistogram.Add(Value);
				debugHistogram2.Add(Value);
			}
			PopState();
			stringBuilder.AppendLine("Gross histogram:");
			debugHistogram.Display(stringBuilder);
			stringBuilder.AppendLine("Fine histogram:");
			debugHistogram2.Display(stringBuilder);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Long-term tests");
			for (int k = 0; k < 3; k++)
			{
				int num2 = 0;
				for (int l = 0; l < 5000000; l++)
				{
					if (MTBEventOccurs(250f, 60000f, 60f))
					{
						num2++;
					}
				}
				string value = "MTB=" + 250 + " days, MTBUnit=" + 60000 + ", check duration=" + 60 + " Simulated " + 5000 + " days (" + 5000000 + " tests). Got " + num2 + " events.";
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Short-term tests");
			for (int m = 0; m < 5; m++)
			{
				int num3 = 0;
				for (int n = 0; n < 10000; n++)
				{
					if (MTBEventOccurs(1f, 24000f, 12000f))
					{
						num3++;
					}
				}
				string value2 = "MTB=" + 1f + " days, MTBUnit=" + 24000f + ", check duration=" + 12000f + ", " + 10000 + " tests got " + num3 + " events.";
				stringBuilder.AppendLine(value2);
			}
			for (int num4 = 0; num4 < 5; num4++)
			{
				int num5 = 0;
				for (int num6 = 0; num6 < 10000; num6++)
				{
					if (MTBEventOccurs(2f, 24000f, 6000f))
					{
						num5++;
					}
				}
				string value3 = "MTB=" + 2f + " days, MTBUnit=" + 24000f + ", check duration=" + 6000f + ", " + 10000 + " tests got " + num5 + " events.";
				stringBuilder.AppendLine(value3);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Near seed tests");
			DebugHistogram debugHistogram3 = new DebugHistogram(new float[11]
			{
				0f,
				0.1f,
				0.2f,
				0.3f,
				0.4f,
				0.5f,
				0.6f,
				0.7f,
				0.8f,
				0.9f,
				1f
			});
			PushState();
			for (int num7 = 0; num7 < 1000; num7++)
			{
				Seed = num7;
				debugHistogram3.Add(Value);
			}
			PopState();
			debugHistogram3.Display(stringBuilder);
			int @int = Int;
			stringBuilder.AppendLine("Repeating single ValueSeeded with seed " + @int + ". This should give the same result:");
			for (int num8 = 0; num8 < 4; num8++)
			{
				stringBuilder.AppendLine("   " + ValueSeeded(@int));
			}
			Log.Message(stringBuilder.ToString());
			if (DebugViewSettings.drawRecordedNoise)
			{
				int[] array = new int[65536];
				PushState();
				for (int num9 = 0; num9 < 65536; num9++)
				{
					array[num9] = (int)(Value * 1000f);
				}
				PopState();
				DebugStoreBucketsAsTexture("One rand output per pixel", array, 1000f, 256);
				int[] array2 = new int[65536];
				PushState();
				for (int num10 = 0; num10 < 65536; num10++)
				{
					Seed = num10;
					array2[num10] = (int)(Value * 1000f);
				}
				PopState();
				DebugStoreBucketsAsTexture("One seed per pixel", array2, 1000f, 256);
				int[] array3 = new int[65536];
				PushState();
				for (int num11 = 0; num11 < 300000; num11++)
				{
					int num12 = (int)(Value * 65536f);
					array3[num12]++;
				}
				PopState();
				float whiteValue = 9.155273f;
				DebugStoreBucketsAsTexture("Brighten random pixel index", array3, whiteValue, 256);
				int[] array4 = new int[65536];
				PushState();
				for (int num13 = 0; num13 < 300000; num13++)
				{
					int num14 = (int)(Value * 256f);
					int num15 = (int)(Value * 256f);
					int num16 = num14 + 256 * num15;
					array4[num16]++;
				}
				PopState();
				float whiteValue2 = 9.155273f;
				DebugStoreBucketsAsTexture("Brighten random pixel coords", array4, whiteValue2, 256);
			}
		}

		private static void DebugStoreBucketsAsTexture(string name, int[] buckets, float whiteValue, int texSize)
		{
			Texture2D texture2D = new Texture2D(texSize, texSize);
			for (int i = 0; i < texSize; i++)
			{
				for (int j = 0; j < texSize; j++)
				{
					int num = i + j * texSize;
					float value = (float)buckets[num] / whiteValue;
					value = Mathf.Clamp01(value);
					texture2D.SetPixel(i, j, new Color(value, value, value));
				}
			}
			texture2D.Apply();
			NoiseDebugUI.StoreTexture(texture2D, name);
		}

		public static int RandSeedForHour(this Thing t, int salt)
		{
			return Gen.HashCombineInt(Gen.HashCombineInt(t.HashOffset(), Find.TickManager.TicksAbs / 2500), salt);
		}

		public static bool TryRangeInclusiveWhere(int from, int to, Predicate<int> predicate, out int value)
		{
			int num = to - from + 1;
			if (num <= 0)
			{
				value = 0;
				return false;
			}
			int num2 = Mathf.Max(Mathf.RoundToInt(Mathf.Sqrt(num)), 5);
			for (int i = 0; i < num2; i++)
			{
				int num3 = RangeInclusive(from, to);
				if (predicate(num3))
				{
					value = num3;
					return true;
				}
			}
			tmpRange.Clear();
			for (int j = from; j <= to; j++)
			{
				tmpRange.Add(j);
			}
			tmpRange.Shuffle();
			int k = 0;
			for (int count = tmpRange.Count; k < count; k++)
			{
				if (predicate(tmpRange[k]))
				{
					value = tmpRange[k];
					return true;
				}
			}
			value = 0;
			return false;
		}

		public static Vector3 PointOnSphereCap(Vector3 center, float angle)
		{
			if (angle <= 0f)
			{
				return center;
			}
			if (angle >= 180f)
			{
				return UnitVector3;
			}
			float num = Range(Mathf.Cos(angle * ((float)Math.PI / 180f)), 1f);
			float f = Range(0f, (float)Math.PI * 2f);
			Vector3 point = new Vector3(Mathf.Sqrt(1f - num * num) * Mathf.Cos(f), Mathf.Sqrt(1f - num * num) * Mathf.Sin(f), num);
			return Quaternion.FromToRotation(Vector3.forward, center) * point;
		}
	}
}
