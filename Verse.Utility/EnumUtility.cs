using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Verse.Utility;

public static class EnumUtility
{
	public static IEnumerable<T> GetValues<T>()
	{
		return Enum.GetValues(typeof(T)).Cast<T>();
	}

	public static IEnumerable<T> GetValuesReverse<T>()
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Reverse();
	}

	public static IEnumerable<Enum> GetBitFlags(this Enum input)
	{
		Array values = Enum.GetValues(input.GetType());
		for (int i = 0; i < values.Length; i++)
		{
			if (math.countbits(Convert.ToUInt64(values.GetValue(i))) == 1 && input.HasFlag((Enum)values.GetValue(i)))
			{
				yield return (Enum)values.GetValue(i);
			}
		}
	}
}
