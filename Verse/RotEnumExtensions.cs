using System.Collections.Generic;
using Verse.Utility;

namespace Verse;

public static class RotEnumExtensions
{
	private static readonly List<Rot4> options = new List<Rot4>();

	public static Rot4 Random(this RotEnum r)
	{
		options.Clear();
		foreach (RotEnum bitFlag in r.GetBitFlags())
		{
			options.Add(bitFlag);
		}
		if (options.Count == 0)
		{
			return Rot4.North;
		}
		return options[Rand.Range(0, options.Count)];
	}
}
