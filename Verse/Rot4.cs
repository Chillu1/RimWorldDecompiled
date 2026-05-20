using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public struct Rot4 : IEquatable<Rot4>
{
	public const int NorthInt = 0;

	public const int EastInt = 1;

	public const int SouthInt = 2;

	public const int WestInt = 3;

	public const int RotationCount = 4;

	private byte rotInt;

	public static readonly Rot4 North = new Rot4(0);

	public static readonly Rot4 East = new Rot4(1);

	public static readonly Rot4 South = new Rot4(2);

	public static readonly Rot4 West = new Rot4(3);

	public static readonly Rot4 Invalid = new Rot4
	{
		rotInt = 200
	};

	public bool IsValid => rotInt < 100;

	public byte AsByte
	{
		get
		{
			return rotInt;
		}
		set
		{
			rotInt = (byte)GenMath.PositiveMod(value, 4);
		}
	}

	public int AsInt
	{
		get
		{
			return rotInt;
		}
		set
		{
			rotInt = (byte)GenMath.PositiveMod(value, 4);
		}
	}

	public float AsAngle => AsInt switch
	{
		0 => 0f, 
		1 => 90f, 
		2 => 180f, 
		3 => 270f, 
		_ => 0f, 
	};

	public SpectateRectSide AsSpectateSide => AsInt switch
	{
		0 => SpectateRectSide.Up, 
		1 => SpectateRectSide.Right, 
		2 => SpectateRectSide.Down, 
		3 => SpectateRectSide.Left, 
		_ => SpectateRectSide.None, 
	};

	public Quaternion AsQuat
	{
		get
		{
			switch (rotInt)
			{
			case 0:
				return Quaternion.identity;
			case 1:
				return Quaternion.LookRotation(Vector3.right);
			case 2:
				return Quaternion.LookRotation(Vector3.back);
			case 3:
				return Quaternion.LookRotation(Vector3.left);
			default:
				Log.Error("ToQuat with Rot = " + AsInt);
				return Quaternion.identity;
			}
		}
	}

	public Vector2 AsVector2 => rotInt switch
	{
		0 => Vector2.up, 
		1 => Vector2.right, 
		2 => Vector2.down, 
		3 => Vector2.left, 
		_ => throw new Exception("rotInt's value cannot be >3 but it is:" + rotInt), 
	};

	public IntVec3 AsIntVec3 => rotInt switch
	{
		0 => IntVec3.North, 
		1 => IntVec3.East, 
		2 => IntVec3.South, 
		3 => IntVec3.West, 
		_ => throw new Exception("rotInt's value cannot be >3 but it is:" + rotInt), 
	};

	public bool IsHorizontal
	{
		get
		{
			if (rotInt != 1)
			{
				return rotInt == 3;
			}
			return true;
		}
	}

	public bool IsVertical
	{
		get
		{
			if (rotInt != 0)
			{
				return rotInt == 2;
			}
			return true;
		}
	}

	public static Rot4 Random => new Rot4(Rand.RangeInclusive(0, 3));

	public static IEnumerable<Rot4> AllRotations
	{
		get
		{
			yield return North;
			yield return East;
			yield return South;
			yield return West;
		}
	}

	public IntVec3 FacingCell => AsInt switch
	{
		0 => new IntVec3(0, 0, 1), 
		1 => new IntVec3(1, 0, 0), 
		2 => new IntVec3(0, 0, -1), 
		3 => new IntVec3(-1, 0, 0), 
		_ => default(IntVec3), 
	};

	public IntVec3 RighthandCell => AsInt switch
	{
		0 => new IntVec3(1, 0, 0), 
		1 => new IntVec3(0, 0, -1), 
		2 => new IntVec3(-1, 0, 0), 
		3 => new IntVec3(0, 0, 1), 
		_ => default(IntVec3), 
	};

	public Rot4 Opposite => AsInt switch
	{
		0 => new Rot4(2), 
		1 => new Rot4(3), 
		2 => new Rot4(0), 
		3 => new Rot4(1), 
		_ => default(Rot4), 
	};

	public static implicit operator Rot4(RotEnum rot)
	{
		return rot switch
		{
			RotEnum.East => East, 
			RotEnum.South => South, 
			RotEnum.West => West, 
			_ => North, 
		};
	}

	public static implicit operator RotEnum(Rot4 rot)
	{
		if (rot == East)
		{
			return RotEnum.East;
		}
		if (rot == South)
		{
			return RotEnum.South;
		}
		if (rot == West)
		{
			return RotEnum.West;
		}
		return RotEnum.North;
	}

	public Rot4(byte newRot)
	{
		rotInt = (byte)GenMath.PositiveMod(newRot, 4);
	}

	public Rot4(int newRot)
	{
		rotInt = (byte)GenMath.PositiveMod(newRot, 4);
	}

	public void Rotate(RotationDirection RotDir)
	{
		AsByte += (byte)RotDir;
	}

	public Rot4 Rotated(RotationDirection RotDir)
	{
		Rot4 result = this;
		result.Rotate(RotDir);
		return result;
	}

	public static Rot4 FromAngleFlat(float angle)
	{
		angle = GenMath.PositiveMod(angle, 360f);
		if (angle < 45f)
		{
			return North;
		}
		if (angle < 135f)
		{
			return East;
		}
		if (angle < 225f)
		{
			return South;
		}
		if (angle < 315f)
		{
			return West;
		}
		return North;
	}

	public static Rot4 FromIntVec3(IntVec3 offset)
	{
		if (offset.x == 1)
		{
			return East;
		}
		if (offset.x == -1)
		{
			return West;
		}
		if (offset.z == 1)
		{
			return North;
		}
		if (offset.z == -1)
		{
			return South;
		}
		IntVec3 intVec = offset;
		Log.Error("FromIntVec3 with bad offset " + intVec.ToString());
		return North;
	}

	public static Rot4 FromIntVec2(IntVec2 offset)
	{
		return FromIntVec3(offset.ToIntVec3);
	}

	public static RotationDirection GetRelativeRotation(Rot4 from, Rot4 to)
	{
		if (!from.IsValid || !to.IsValid)
		{
			Log.Error($"Both from ({from}) and to ({to}) must be valid.");
			return RotationDirection.None;
		}
		return (RotationDirection)GenMath.PositiveMod(to.AsByte - from.AsByte, 4);
	}

	public static bool operator ==(Rot4 a, Rot4 b)
	{
		return a.AsInt == b.AsInt;
	}

	public static bool operator !=(Rot4 a, Rot4 b)
	{
		return a.AsInt != b.AsInt;
	}

	public override int GetHashCode()
	{
		return rotInt switch
		{
			0 => 235515, 
			1 => 5612938, 
			2 => 1215650, 
			3 => 9231792, 
			_ => rotInt, 
		};
	}

	public override string ToString()
	{
		return rotInt.ToString();
	}

	public string ToStringHuman()
	{
		return rotInt switch
		{
			0 => "North".Translate(), 
			1 => "East".Translate(), 
			2 => "South".Translate(), 
			3 => "West".Translate(), 
			_ => "error", 
		};
	}

	public string ToStringWord()
	{
		return rotInt switch
		{
			0 => "North", 
			1 => "East", 
			2 => "South", 
			3 => "West", 
			_ => "error", 
		};
	}

	public static Rot4 FromString(string str)
	{
		byte newRot;
		if (int.TryParse(str, out var result))
		{
			newRot = (byte)result;
		}
		else
		{
			switch (str)
			{
			case "North":
				newRot = 0;
				break;
			case "East":
				newRot = 1;
				break;
			case "South":
				newRot = 2;
				break;
			case "West":
				newRot = 3;
				break;
			default:
				newRot = 0;
				Log.Error("Invalid rotation: " + str);
				break;
			}
		}
		return new Rot4(newRot);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Rot4))
		{
			return false;
		}
		return Equals((Rot4)obj);
	}

	public bool Equals(Rot4 other)
	{
		return rotInt == other.rotInt;
	}
}
