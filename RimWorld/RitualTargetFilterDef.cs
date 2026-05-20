using System;
using Verse;

namespace RimWorld
{
	public class RitualTargetFilterDef : Def
	{
		public Type workerClass;

		public bool fallBackToGatherSpot;

		public bool fallbackToRitualSpot;

		public RitualTargetFilter GetInstance()
		{
			return (RitualTargetFilter)Activator.CreateInstance(workerClass, this);
		}
	}
}
