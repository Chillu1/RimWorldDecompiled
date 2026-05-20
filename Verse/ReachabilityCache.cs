using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class ReachabilityCache
{
	private struct CachedEntry : IEquatable<CachedEntry>
	{
		public int FirstID { get; private set; }

		public int SecondID { get; private set; }

		public TraverseParms TraverseParms { get; private set; }

		public CachedEntry(int firstID, int secondID, TraverseParms traverseParms)
		{
			this = default(CachedEntry);
			if (firstID < secondID)
			{
				FirstID = firstID;
				SecondID = secondID;
			}
			else
			{
				FirstID = secondID;
				SecondID = firstID;
			}
			TraverseParms = traverseParms;
		}

		public static bool operator ==(CachedEntry lhs, CachedEntry rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(CachedEntry lhs, CachedEntry rhs)
		{
			return !lhs.Equals(rhs);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CachedEntry))
			{
				return false;
			}
			return Equals((CachedEntry)obj);
		}

		public bool Equals(CachedEntry other)
		{
			if (FirstID == other.FirstID && SecondID == other.SecondID)
			{
				return TraverseParms == other.TraverseParms;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(Gen.HashCombineInt(FirstID, SecondID), TraverseParms);
		}
	}

	private Dictionary<CachedEntry, bool> cacheDict = new Dictionary<CachedEntry, bool>();

	private static List<CachedEntry> tmpCachedEntries = new List<CachedEntry>();

	public int Count => cacheDict.Count;

	public BoolUnknown CachedResultFor(District A, District B, TraverseParms traverseParams)
	{
		if (cacheDict.TryGetValue(new CachedEntry(A.ID, B.ID, traverseParams), out var value))
		{
			if (!value)
			{
				return BoolUnknown.False;
			}
			return BoolUnknown.True;
		}
		return BoolUnknown.Unknown;
	}

	public void AddCachedResult(District A, District B, TraverseParms traverseParams, bool reachable)
	{
		CachedEntry key = new CachedEntry(A.ID, B.ID, traverseParams);
		if (!cacheDict.ContainsKey(key))
		{
			cacheDict.Add(key, reachable);
		}
	}

	public void Clear()
	{
		cacheDict.Clear();
	}

	public void ClearFor(Pawn p)
	{
		tmpCachedEntries.Clear();
		foreach (KeyValuePair<CachedEntry, bool> item in cacheDict)
		{
			if (item.Key.TraverseParms.pawn == p)
			{
				tmpCachedEntries.Add(item.Key);
			}
		}
		for (int i = 0; i < tmpCachedEntries.Count; i++)
		{
			cacheDict.Remove(tmpCachedEntries[i]);
		}
		tmpCachedEntries.Clear();
	}

	public void ClearForHostile(Thing hostileTo)
	{
		tmpCachedEntries.Clear();
		foreach (KeyValuePair<CachedEntry, bool> item in cacheDict)
		{
			Pawn pawn = item.Key.TraverseParms.pawn;
			if (pawn != null && pawn.HostileTo(hostileTo))
			{
				tmpCachedEntries.Add(item.Key);
			}
		}
		for (int i = 0; i < tmpCachedEntries.Count; i++)
		{
			cacheDict.Remove(tmpCachedEntries[i]);
		}
		tmpCachedEntries.Clear();
	}
}
