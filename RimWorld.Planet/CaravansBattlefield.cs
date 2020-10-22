using Verse;

namespace RimWorld.Planet
{
	public class CaravansBattlefield : MapParent
	{
		private bool wonBattle;

		public bool WonBattle => wonBattle;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref wonBattle, "wonBattle", defaultValue: false);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				alsoRemoveWorldObject = true;
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void Tick()
		{
			base.Tick();
			if (base.HasMap)
			{
				CheckWonBattle();
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			GetComponent<TimedDetectionRaids>().StartDetectionCountdown(240000);
		}

		private void CheckWonBattle()
		{
			if (!wonBattle && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				TimedDetectionRaids component = GetComponent<TimedDetectionRaids>();
				component.SetNotifiedSilently();
				string detectionCountdownTimeLeftString = component.DetectionCountdownTimeLeftString;
				Find.LetterStack.ReceiveLetter("LetterLabelCaravansBattlefieldVictory".Translate(), "LetterCaravansBattlefieldVictory".Translate(detectionCountdownTimeLeftString), LetterDefOf.PositiveEvent, this);
				TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, base.Map.mapPawns.FreeColonists.RandomElement());
				wonBattle = true;
			}
		}
	}
}
