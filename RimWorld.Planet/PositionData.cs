using Verse;

namespace RimWorld.Planet;

public class PositionData : IExposable
{
	public struct Data
	{
		public IntVec3 local;

		public Rot4 rotation;

		public bool drafted;
	}

	public IntVec3 position;

	public RotationDirection relativeRotation;

	public bool drafted;

	public PositionData()
	{
	}

	public PositionData(IntVec3 position, Rot4 rotation, bool drafted = false)
	{
		RotationDirection rotationDirection = RotationDirection.None;
		if (rotation == Rot4.East)
		{
			rotationDirection = RotationDirection.Clockwise;
		}
		else if (rotation == Rot4.South)
		{
			rotationDirection = RotationDirection.Opposite;
		}
		else if (rotation == Rot4.West)
		{
			rotationDirection = RotationDirection.Counterclockwise;
		}
		relativeRotation = rotationDirection;
		this.position = position;
		this.drafted = drafted;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref position, "position");
		Scribe_Values.Look(ref relativeRotation, "relativeRotation", RotationDirection.None);
		Scribe_Values.Look(ref drafted, "drafted", defaultValue: false);
	}
}
