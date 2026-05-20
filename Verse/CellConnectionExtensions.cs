using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Verse;

public static class CellConnectionExtensions
{
	public static readonly IntVec3[] OffsetFromBitIndex = new IntVec3[8]
	{
		new IntVec3(0, 0, 1),
		new IntVec3(0, 0, -1),
		new IntVec3(1, 0, 0),
		new IntVec3(-1, 0, 0),
		new IntVec3(1, 0, 1),
		new IntVec3(1, 0, -1),
		new IntVec3(-1, 0, -1),
		new IntVec3(-1, 0, 1)
	};

	public static readonly CellConnection[] FlagFromBitIndex = new CellConnection[8]
	{
		CellConnection.North,
		CellConnection.South,
		CellConnection.East,
		CellConnection.West,
		CellConnection.NorthEast,
		CellConnection.SouthEast,
		CellConnection.SouthWest,
		CellConnection.NorthWest
	};

	public static readonly CellConnection[] InverseFromBitIndex = new CellConnection[8]
	{
		CellConnection.South,
		CellConnection.North,
		CellConnection.West,
		CellConnection.East,
		CellConnection.SouthWest,
		CellConnection.NorthWest,
		CellConnection.NorthEast,
		CellConnection.SouthEast
	};

	public static IntVec3 Offset(this CellConnection connection)
	{
		if (connection == CellConnection.Self)
		{
			return IntVec3.Zero;
		}
		if (math.countbits((int)connection) != 1)
		{
			throw new ArgumentException("connection");
		}
		return OffsetFromBitIndex[connection.BitIndex()];
	}

	public static CellConnection Inverse(this CellConnection connection)
	{
		return connection switch
		{
			CellConnection.North => CellConnection.South, 
			CellConnection.South => CellConnection.North, 
			CellConnection.East => CellConnection.West, 
			CellConnection.West => CellConnection.East, 
			CellConnection.NorthEast => CellConnection.SouthWest, 
			CellConnection.SouthEast => CellConnection.NorthWest, 
			CellConnection.SouthWest => CellConnection.NorthEast, 
			CellConnection.NorthWest => CellConnection.SouthEast, 
			_ => throw new ArgumentException("connection"), 
		};
	}

	public static CellConnectionEnumerator GetEnumerator(this CellConnection connections)
	{
		return new CellConnectionEnumerator(connections);
	}

	public static CellConnection? ConnectionFromCells(IntVec3 first, IntVec3 second)
	{
		IntVec3 intVec = second - first;
		for (int i = 0; i < OffsetFromBitIndex.Length; i++)
		{
			if (OffsetFromBitIndex[i] == intVec)
			{
				return FlagFromBitIndex[i];
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Diagonal(this CellConnection connection)
	{
		return (connection & CellConnection.DiagonalNeighbors) > CellConnection.North;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Cardinal(this CellConnection connection)
	{
		return (connection & CellConnection.CardinalNeighbors) > CellConnection.North;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitIndex(this CellConnection connection)
	{
		return math.tzcnt((uint)connection);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitLoopEndIndex(this CellConnection connections)
	{
		return 8 - (math.lzcnt((uint)connections) - 8) % 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasBit(this CellConnection connections, CellConnection testFlag)
	{
		return (connections & testFlag) != 0;
	}
}
