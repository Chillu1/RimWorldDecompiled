using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetFilterDef : Def
	{
		public Type workerClass;

		public bool colonistThingsOnly = true;

		public List<ThingDef> thingDefs;

		public int minUnroofedCells;

		public int unroofedCellSearchRadius;

		public int woodPerParticipant;

		public int maxSpeakerDistance;

		public int maxDrumDistance;

		public RitualObligationTargetFilter GetInstance()
		{
			return (RitualObligationTargetFilter)Activator.CreateInstance(workerClass, this);
		}
	}
}
