using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RiverDef : Def
	{
		public class Branch
		{
			public int minFlow;

			public RiverDef child;

			public float chance = 1f;
		}

		public int spawnFlowThreshold = -1;

		public float spawnChance = 1f;

		public int degradeThreshold;

		public RiverDef degradeChild;

		public List<Branch> branches;

		public float widthOnWorld = 0.5f;

		public float widthOnMap = 10f;

		public float debugOpacity;
	}
}
