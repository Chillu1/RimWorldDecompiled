using Verse;

namespace RimWorld
{
	public class SignalAction_OpenDoor : SignalAction
	{
		public Building_Door door;

		protected override void DoAction(SignalArgs args)
		{
			if (door != null)
			{
				if (args.TryGetArg("SUBJECT", out Pawn arg))
				{
					door.StartManualOpenBy(arg);
				}
				else
				{
					door.StartManualOpenBy(null);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref door, "door");
		}
	}
}
