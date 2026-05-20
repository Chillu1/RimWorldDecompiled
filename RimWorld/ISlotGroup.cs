using System.Collections.Generic;
using Verse;

namespace RimWorld;

public interface ISlotGroup
{
	StorageSettings Settings { get; }

	IEnumerable<Thing> HeldThings { get; }

	StorageGroup StorageGroup { get; }

	List<IntVec3> CellsList { get; }

	string GroupingLabel { get; }

	int GroupingOrder { get; }
}
