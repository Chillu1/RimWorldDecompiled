using System;

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

		public IntVec3? preferredCastPosition;

		public Func<IntVec3, bool> validator;

		public int maxRegions;
	}
}
