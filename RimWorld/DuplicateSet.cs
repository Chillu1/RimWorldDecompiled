using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class DuplicateSet : IExposable
{
	private HashSet<Pawn> pawns = new HashSet<Pawn>();

	public int Count => pawns.Count;

	public void Add(Pawn pawn)
	{
		pawns.Add(pawn);
	}

	public void Remove(Pawn pawn)
	{
		pawns.Remove(pawn);
	}

	public bool Contains(Pawn pawn)
	{
		return pawns.Contains(pawn);
	}

	public void Clear()
	{
		pawns.Clear();
	}

	public Pawn First()
	{
		return pawns.First();
	}

	public HashSet<Pawn>.Enumerator GetEnumerator()
	{
		return pawns.GetEnumerator();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && pawns.Contains(null))
		{
			pawns.Remove(null);
		}
	}
}
