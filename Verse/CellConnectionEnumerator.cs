using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse;

public struct CellConnectionEnumerator : IEnumerator<CellConnection>, IEnumerator, IDisposable
{
	private readonly CellConnection bitmask;

	private CellConnection current;

	private int index;

	private int end;

	public CellConnection Current => current;

	object IEnumerator.Current => current;

	internal CellConnectionEnumerator(CellConnection bitmask)
	{
		this.bitmask = bitmask;
		current = CellConnection.Self;
		index = bitmask.BitIndex();
		end = bitmask.BitLoopEndIndex();
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		while (index < end)
		{
			CellConnection testFlag = CellConnectionExtensions.FlagFromBitIndex[index++];
			if (bitmask.HasBit(testFlag))
			{
				current = testFlag;
				return true;
			}
		}
		current = CellConnection.Self;
		return false;
	}

	void IEnumerator.Reset()
	{
		current = CellConnection.Self;
		index = bitmask.BitIndex();
		end = bitmask.BitLoopEndIndex();
	}
}
