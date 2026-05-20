using System;

namespace Verse;

public struct TextureAtlasGroupKey : IEquatable<TextureAtlasGroupKey>
{
	public TextureAtlasGroup group;

	public bool hasMask;

	public bool Equals(TextureAtlasGroupKey other)
	{
		if (other.group == group)
		{
			return other.hasMask == hasMask;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextureAtlasGroupKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = group.GetHashCode();
		if (hasMask)
		{
			num *= -1;
		}
		return num;
	}

	public override string ToString()
	{
		return group.ToString() + "_" + hasMask;
	}
}
