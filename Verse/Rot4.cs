using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public struct Rot4 : IEquatable<Rot4>
	{
		private byte rotInt;

		public bool IsValid => rotInt < 100;

		public byte AsByte
		{
			get
			{
				return rotInt;
			}
			set
			{
				rotInt = (byte)((int)value % 4);
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
				if (value < 0)
				{
					value += 4000;
				}
				rotInt = (byte)(value % 4);
			}
		}

		public float AsAngle
		{
			get
			{
				switch (AsInt)
				{
				case 0:
					return 0f;
				case 1:
					return 90f;
				case 2:
					return 180f;
				case 3:
					return 270f;
				default:
					return 0f;
				}
			}
		}

		public SpectateRectSide AsSpectateSide
		{
			get
			{
				switch (AsInt)
				{
				case 0:
					return SpectateRectSide.Up;
				case 1:
					return SpectateRectSide.Right;
				case 2:
					return SpectateRectSide.Down;
				case 3:
					return SpectateRectSide.Left;
				default:
					return SpectateRectSide.None;
				}
			}
		}

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

		public Vector2 AsVector2
		{
			get
			{
				switch (rotInt)
				{
				case 0:
					return Vector2.up;
				case 1:
					return Vector2.right;
				case 2:
					return Vector2.down;
				case 3:
					return Vector2.left;
				default:
					throw new Exception("rotInt's value cannot be >3 but it is:" + rotInt);
				}
			}
		}

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

		public static Rot4 North => new Rot4(0);

		public static Rot4 East => new Rot4(1);

		public static Rot4 South => new Rot4(2);

		public static Rot4 West => new Rot4(3);

		public static Rot4 Random => new Rot4(Rand.RangeInclusive(0, 3));

		public static Rot4 Invalid
		{
			get
			{
				Rot4 result = default(Rot4);
				result.rotInt = 200;
				return result;
			}
		}

		public IntVec3 FacingCell
		{
			get
			{
				switch (AsInt)
				{
				case 0:
					return new IntVec3(0, 0, 1);
				case 1:
					return new IntVec3(1, 0, 0);
				case 2:
					return new IntVec3(0, 0, -1);
				case 3:
					return new IntVec3(-1, 0, 0);
				default:
					return default(IntVec3);
				}
			}
		}

		public IntVec3 RighthandCell
		{
			get
			{
				switch (AsInt)
				{
				case 0:
					return new IntVec3(1, 0, 0);
				case 1:
					return new IntVec3(0, 0, -1);
				case 2:
					return new IntVec3(-1, 0, 0);
				case 3:
					return new IntVec3(0, 0, 1);
				default:
					return default(IntVec3);
				}
			}
		}

		public Rot4 Opposite
		{
			get
			{
				switch (AsInt)
				{
				case 0:
					return new Rot4(2);
				case 1:
					return new Rot4(3);
				case 2:
					return new Rot4(0);
				case 3:
					return new Rot4(1);
				default:
					return default(Rot4);
				}
			}
		}

		public Rot4(byte newRot)
		{
			rotInt = newRot;
		}

		public Rot4(int newRot)
		{
			rotInt = (byte)(newRot % 4);
		}

		public void Rotate(RotationDirection RotDir)
		{
			if (RotDir == RotationDirection.Clockwise)
			{
				AsInt++;
			}
			if (RotDir == RotationDirection.Counterclockwise)
			{
				AsInt--;
			}
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
			Log.Error("FromIntVec3 with bad offset " + offset);
			return North;
		}

		public static Rot4 FromIntVec2(IntVec2 offset)
		{
			return FromIntVec3(offset.ToIntVec3);
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
			switch (rotInt)
			{
			case 0:
				return 235515;
			case 1:
				return 5612938;
			case 2:
				return 1215650;
			case 3:
				return 9231792;
			default:
				return rotInt;
			}
		}

		public override string ToString()
		{
			return rotInt.ToString();
		}

		public string ToStringHuman()
		{
			switch (rotInt)
			{
			case 0:
				return "North".Translate();
			case 1:
				return "East".Translate();
			case 2:
				return "South".Translate();
			case 3:
				return "West".Translate();
			default:
				return "error";
			}
		}

		public string ToStringWord()
		{
			switch (rotInt)
			{
			case 0:
				return "North";
			case 1:
				return "East";
			case 2:
				return "South";
			case 3:
				return "West";
			default:
				return "error";
			}
		}

		public static Rot4 FromString(string str)
		{
			byte newRot;
			if (int.TryParse(str, out int result))
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
}
