using System;

namespace Verse;

[Flags]
public enum CellConnection : byte
{
	Self = 0,
	North = 1,
	South = 2,
	East = 4,
	West = 8,
	NorthEast = 0x10,
	SouthEast = 0x20,
	SouthWest = 0x40,
	NorthWest = 0x80,
	CardinalNeighbors = 0xF,
	DiagonalNeighbors = 0xF0,
	NorthNeighbours = 0x91,
	EastNeighbours = 0x34,
	SouthNeighbours = 0x62,
	WestNeighbours = 0xC8,
	AllNeighbours = byte.MaxValue
}
