using RimWorld;

namespace Verse
{
	public abstract class PawnTrigger : Thing
	{
		public string signalTag;

		protected bool TriggeredBy(Thing thing)
		{
			if (thing.def.category == ThingCategory.Pawn && thing.def.race.intelligence == Intelligence.Humanlike)
			{
				return thing.Faction == Faction.OfPlayer;
			}
			return false;
		}

		public void ActivatedBy(Pawn p)
		{
			Find.SignalManager.SendSignal(new Signal(signalTag, p.Named("SUBJECT")));
			if (!base.Destroyed)
			{
				Destroy();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref signalTag, "signalTag");
		}
	}
}
