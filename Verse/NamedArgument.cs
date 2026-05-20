using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public struct NamedArgument
{
	public object arg;

	public string label;

	public NamedArgument(object arg, string label)
	{
		this.arg = arg;
		this.label = label;
	}

	public static implicit operator NamedArgument(int value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(char value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(float value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(double value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(long value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(string value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(uint value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(byte value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(ulong value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(StringBuilder value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(Thing value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(Def value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(WorldObject value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(Faction value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(IntVec3 value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(LocalTargetInfo value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(TargetInfo value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(GlobalTargetInfo value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(Map value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(TaggedString value)
	{
		return new NamedArgument(value, null);
	}

	public static implicit operator NamedArgument(Ideo value)
	{
		return new NamedArgument(value, null);
	}

	public override string ToString()
	{
		return (label.NullOrEmpty() ? "unnamed" : label) + "->" + arg.ToStringSafe();
	}
}
