namespace RimWorld
{
	public static class PawnPostureUtility
	{
		public static bool Laying(this PawnPosture posture)
		{
			if (posture != PawnPosture.LayingOnGroundFaceUp && posture != PawnPosture.LayingOnGroundNormal)
			{
				return posture == PawnPosture.LayingInBed;
			}
			return true;
		}
	}
}
