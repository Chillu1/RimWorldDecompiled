using System;

namespace Verse;

public struct GenStepWithParams : IEquatable<GenStepWithParams>
{
	public GenStepDef def;

	public GenStepParams parms;

	public GenStepWithParams(GenStepDef def, GenStepParams parms)
	{
		this.def = def;
		this.parms = parms;
	}

	public bool Equals(GenStepWithParams other)
	{
		return object.Equals(def, other.def);
	}

	public override bool Equals(object obj)
	{
		if (obj is GenStepWithParams other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(def, parms);
	}
}
