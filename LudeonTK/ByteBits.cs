using System.Runtime.CompilerServices;

namespace LudeonTK;

public struct ByteBits
{
	private byte intValue;

	public bool this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (intValue & (1 << index)) != 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (value)
			{
				intValue = (byte)(intValue | (1 << index));
			}
			else
			{
				intValue = (byte)(intValue & ~(1 << index));
			}
		}
	}
}
