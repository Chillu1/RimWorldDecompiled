using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FeatureDef : Def
	{
		public Type workerClass = typeof(FeatureWorker);

		public float order;

		public int minSize = 50;

		public int maxSize = int.MaxValue;

		public bool canTouchWorldEdge = true;

		public RulePackDef nameMaker;

		public int maxPossiblyAllowedSizeToTake = 30;

		public float maxPossiblyAllowedSizePctOfMeToTake = 0.5f;

		public List<BiomeDef> rootBiomes = new List<BiomeDef>();

		public List<BiomeDef> acceptableBiomes = new List<BiomeDef>();

		public int maxSpaceBetweenRootGroups = 5;

		public int minRootGroupsInCluster = 3;

		public int minRootGroupSize = 10;

		public int maxRootGroupSize = int.MaxValue;

		public int maxPassageWidth = 3;

		public float maxPctOfWholeArea = 0.1f;

		[Unsaved(false)]
		private FeatureWorker workerInt;

		public FeatureWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (FeatureWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
