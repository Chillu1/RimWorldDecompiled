using Verse;

namespace RimWorld
{
	public class SignalAction_Infestation : SignalAction_Delay
	{
		public int hivesCount = 1;

		public float? insectsPoints;

		public bool spawnAnywhereIfNoGoodCell;

		public bool ignoreRoofedRequirement;

		public IntVec3? overrideLoc;

		public bool sendStandardLetter;

		private Alert_ActionDelay cachedAlert;

		public override Alert_ActionDelay Alert
		{
			get
			{
				if (cachedAlert == null)
				{
					cachedAlert = new Alert_InfestationDelay(this);
				}
				return cachedAlert;
			}
		}

		protected override void Complete()
		{
			base.Complete();
			Thing thing = InfestationUtility.SpawnTunnels(hivesCount, base.Map, spawnAnywhereIfNoGoodCell, ignoreRoofedRequirement, null, overrideLoc, insectsPoints);
			if (thing != null && sendStandardLetter)
			{
				IntVec3 cell = overrideLoc ?? thing.Position;
				Find.LetterStack.ReceiveLetter(IncidentDefOf.Infestation.letterLabel, IncidentDefOf.Infestation.letterText, IncidentDefOf.Infestation.letterDef, new TargetInfo(cell, base.Map));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref hivesCount, "hivesCount", 0);
			Scribe_Values.Look(ref spawnAnywhereIfNoGoodCell, "spawnAnywhereIfNoGoodCell", defaultValue: false);
			Scribe_Values.Look(ref ignoreRoofedRequirement, "ignoreRoofedRequirement", defaultValue: false);
			Scribe_Values.Look(ref overrideLoc, "overrideLoc");
			Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: false);
			Scribe_Values.Look(ref insectsPoints, "insectsPoints");
		}
	}
}
