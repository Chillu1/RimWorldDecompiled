namespace Verse.AI
{
	public struct CastPositionRequest
	{
		public Pawn caster;

		public Thing target;

		public Verb verb;

		public float maxRangeFromCaster;

		public float maxRangeFromTarget;

		public IntVec3 locus;

		public float maxRangeFromLocus;

		public bool wantCoverFromTarget;

		public int maxRegions;
	}
}
