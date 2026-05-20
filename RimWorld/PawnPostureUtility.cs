namespace RimWorld
{
	public static class PawnPostureUtility
	{
		public static bool Laying(this PawnPosture posture)
		{
			return (posture & PawnPosture.LayingOnGroundNormal) != 0;
		}

		public static bool InBed(this PawnPosture posture)
		{
			return (posture & PawnPosture.InBedMask) != 0;
		}

		public static bool FaceUp(this PawnPosture posture)
		{
			return (posture & PawnPosture.FaceUpMask) != 0;
		}
	}
}
