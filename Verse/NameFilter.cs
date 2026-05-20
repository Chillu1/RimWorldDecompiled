using System;

namespace Verse;

[Flags]
public enum NameFilter
{
	None = 0,
	First = 1,
	Nick = 2,
	Last = 4,
	Title = 8
}
