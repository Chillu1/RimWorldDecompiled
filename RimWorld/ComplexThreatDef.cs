using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ComplexThreatDef : Def
	{
		public Type workerClass = typeof(ComplexThreatWorker);

		public FactionDef faction;

		public float postSpawnPassiveThreatFactor = 1f;

		public int minPoints;

		public float spawnInOtherRoomChance;

		public bool allowPassive = true;

		public bool fallbackToRoomEnteredTrigger = true;

		public float delayChance;

		public List<int> delayTickOptions;

		public SimpleCurve threatFactorOverDelayTicksCurve;

		public SignalActionAmbushType signalActionAmbushType;

		public bool spawnAroundComplex;

		public bool useDropPods;

		[Unsaved(false)]
		private ComplexThreatWorker workerInt;

		public ComplexThreatWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (ComplexThreatWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (delayChance > 0f && delayTickOptions == null)
			{
				yield return "Chance to have a delayed threat is > 0 but no signal delay tick options are set.";
			}
		}
	}
}
