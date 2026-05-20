using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_BiosculpterPod_HealingCycle : CompProperties_BiosculpterPod_BaseCycle
	{
		public List<BodyPartDef> bodyPartsToRestore;

		public List<HediffDef> conditionsToPossiblyCure;
	}
}
