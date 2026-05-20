using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class Gen
{
	private static MethodInfo s_memberwiseClone;

	public static Vector3 AveragePosition(List<IntVec3> cells)
	{
		return new Vector3((float)cells.Average((IntVec3 c) => c.x) + 0.5f, 0f, (float)cells.Average((IntVec3 c) => c.z) + 0.5f);
	}

	public static T RandomEnumValue<T>(bool disallowFirstValue)
	{
		int minInclusive = (disallowFirstValue ? 1 : 0);
		T[] array = (T[])Enum.GetValues(typeof(T));
		int num = Rand.Range(minInclusive, array.Length);
		return array[num];
	}

	public static Vector3 RandomHorizontalVector(float max)
	{
		return new Vector3(Rand.Range(0f - max, max), 0f, Rand.Range(0f - max, max));
	}

	public static Vector3 Random2DVector(Vector3 max)
	{
		return new Vector3(Rand.Range(0f - max.x, max.x), 0f, Rand.Range(0f - max.z, max.z));
	}

	public static int GetBitCountOf(long lValue)
	{
		int num = 0;
		while (lValue != 0L)
		{
			lValue &= lValue - 1;
			num++;
		}
		return num;
	}

	public static IEnumerable<T> GetAllSelectedItems<T>(this Enum value)
	{
		CultureInfo cult = CultureInfo.InvariantCulture;
		int valueAsInt = Convert.ToInt32(value, cult);
		foreach (object value2 in Enum.GetValues(typeof(T)))
		{
			int num = Convert.ToInt32(value2, cult);
			if (num == (valueAsInt & num))
			{
				yield return (T)value2;
			}
		}
	}

	public static IEnumerable<T> YieldSingle<T>(T val)
	{
		yield return val;
	}

	public static IEnumerable YieldSingleNonGeneric<T>(T val)
	{
		yield return val;
	}

	public static string ToStringSafe<T>(this T obj)
	{
		if (obj == null)
		{
			return "null";
		}
		try
		{
			return obj.ToString();
		}
		catch (Exception ex)
		{
			int num = 0;
			bool flag = false;
			try
			{
				num = obj.GetHashCode();
				flag = true;
			}
			catch
			{
			}
			if (flag)
			{
				Log.ErrorOnce("Exception in ToString(): " + ex, num ^ 0x6EB69D11);
			}
			else
			{
				Log.Error("Exception in ToString(): " + ex);
			}
			return "error";
		}
	}

	public static string ToStringSafeEnumerable(this IEnumerable enumerable)
	{
		if (enumerable == null)
		{
			return "null";
		}
		try
		{
			string text = "";
			foreach (object item in enumerable)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += item.ToStringSafe();
			}
			return text;
		}
		catch (Exception ex)
		{
			int num = 0;
			bool flag = false;
			try
			{
				num = enumerable.GetHashCode();
				flag = true;
			}
			catch
			{
			}
			if (flag)
			{
				Log.ErrorOnce("Exception while enumerating: " + ex, num ^ 0x22AC96D9);
			}
			else
			{
				Log.Error("Exception while enumerating: " + ex);
			}
			return "error";
		}
	}

	public static void Swap<T>(ref T x, ref T y)
	{
		T val = y;
		y = x;
		x = val;
	}

	public static T MemberwiseClone<T>(T obj)
	{
		if (s_memberwiseClone == null)
		{
			s_memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		return (T)s_memberwiseClone.Invoke(obj, null);
	}

	public static void MemberwiseShallowCopy(object from, object to)
	{
		FieldInfo[] fields = from.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(to, fieldInfo.GetValue(from));
		}
	}

	public static int FixedTimeStepUpdate(ref float timeBuffer, float fps)
	{
		timeBuffer += Mathf.Min(Time.deltaTime, 1f);
		float num = 1f / fps;
		int num2 = Mathf.FloorToInt(timeBuffer / num);
		timeBuffer -= (float)num2 * num;
		return num2;
	}

	public static int HashCombine<T>(int seed, T obj)
	{
		int num = obj?.GetHashCode() ?? 0;
		return (int)(seed ^ (num + 2654435769u + (seed << 6) + (seed >> 2)));
	}

	public static int HashCombineStruct<T>(int seed, T obj) where T : struct
	{
		return (int)(seed ^ (obj.GetHashCode() + 2654435769u + (seed << 6) + (seed >> 2)));
	}

	public static int HashCombineInt(int seed, int value)
	{
		return (int)(seed ^ (value + 2654435769u + (seed << 6) + (seed >> 2)));
	}

	public static int HashCombineInt(int v1, int v2, int v3, int v4)
	{
		int num = 352654597;
		int num2 = num;
		num = ((num << 5) + num + (num >> 27)) ^ v1;
		num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ v2;
		num = ((num << 5) + num + (num >> 27)) ^ v3;
		num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ v4;
		return num + num2 * 1566083941;
	}

	public static int HashOffset(this int baseInt)
	{
		return HashCombineInt(baseInt, 169495093);
	}

	public static int HashOffset(this Thing t)
	{
		return t.thingIDNumber.HashOffset();
	}

	public static int HashOffset(this WorldObject o)
	{
		return o.ID.HashOffset();
	}

	public static bool IsHashIntervalTick(this Map m, int interval)
	{
		return m.HashOffsetTicks() % interval == 0;
	}

	public static bool IsHashIntervalTick(this Map m, int interval, int offset)
	{
		return (m.HashOffsetTicks() + offset) % interval == 0;
	}

	public static int HashOffsetTicks(this Map m)
	{
		return Find.TickManager.TicksGame + m.uniqueID.HashOffset();
	}

	public static bool IsHashIntervalTick(this Thing t, int interval)
	{
		return t.HashOffsetTicks() % interval == 0;
	}

	public static bool IsHashIntervalTick(this Thing t, int interval, int delta)
	{
		return Math.Abs(t.HashOffsetTicks() % interval) < delta;
	}

	public static int HashOffsetTicks(this Thing t)
	{
		return Find.TickManager.TicksGame + t.thingIDNumber.HashOffset();
	}

	public static bool IsHashIntervalTick(this WorldObject o, int interval)
	{
		if (interval <= 0)
		{
			return true;
		}
		int num = o.HashOffsetTicks() % interval;
		if (num < 0)
		{
			num += interval;
		}
		return num == 0;
	}

	public static bool IsHashIntervalTick(this WorldObject o, int interval, int delta)
	{
		if (interval <= 0)
		{
			return true;
		}
		if (delta <= 0)
		{
			return false;
		}
		int num = o.HashOffsetTicks() % interval;
		if (num < 0)
		{
			num += interval;
		}
		return num < delta;
	}

	public static int HashOffsetTicks(this WorldObject o)
	{
		return Find.TickManager.TicksGame + o.ID.HashOffset();
	}

	public static bool IsHashIntervalTick(this Faction f, int interval)
	{
		return f.HashOffsetTicks() % interval == 0;
	}

	public static int HashOffsetTicks(this Faction f)
	{
		return Find.TickManager.TicksGame + f.randomKey.HashOffset();
	}

	public static int HashOrderless(int v1, int v2)
	{
		return HashCombineInt(Math.Min(v1, v2), Math.Max(v1, v2));
	}

	public static bool IsNestedHashIntervalTick(this Thing t, int outerInterval, int approxInnerInterval)
	{
		int num = Mathf.Max(Mathf.RoundToInt((float)approxInnerInterval / (float)outerInterval), 1);
		return t.HashOffsetTicks() / outerInterval % num == 0;
	}

	public static void ReplaceNullFields<T>(ref T replaceIn, T replaceWith)
	{
		if (replaceIn == null || replaceWith == null)
		{
			return;
		}
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetValue(replaceIn) == null)
			{
				object value = fieldInfo.GetValue(replaceWith);
				if (value != null)
				{
					object obj = replaceIn;
					fieldInfo.SetValue(obj, value);
					replaceIn = (T)obj;
				}
			}
		}
	}

	public static void EnsureAllFieldsNullable(Type type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (fieldType.IsValueType && !(Nullable.GetUnderlyingType(fieldType) != null))
			{
				Log.Warning("Field " + type.Name + "." + fieldInfo.Name + " is not nullable.");
			}
		}
	}

	public static string GetNonNullFieldsDebugInfo(object obj)
	{
		if (obj == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(obj);
			if (value != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(fieldInfo.Name + "=" + value.ToStringSafe());
			}
		}
		return stringBuilder.ToString();
	}

	public static bool InBounds<T>(this T[,] array, int i, int j)
	{
		if (i >= 0 && j >= 0 && i < array.GetLength(0))
		{
			return j < array.GetLength(1);
		}
		return false;
	}
}
