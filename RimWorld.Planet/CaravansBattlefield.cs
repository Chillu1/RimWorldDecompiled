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

		private void CheckWonBattle()
		{
			if (!wonBattle && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				string forceExitAndRemoveMapCountdownTimeLeftString = TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000);
				Find.LetterStack.ReceiveLetter("LetterLabelCaravansBattlefieldVictory".Translate(), "LetterCaravansBattlefieldVictory".Translate(forceExitAndRemoveMapCountdownTimeLeftString), LetterDefOf.PositiveEvent, this);
				TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, base.Map.mapPawns.FreeColonists.RandomElement());
				wonBattle = true;
				GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown();
			}
		}
	}
}
