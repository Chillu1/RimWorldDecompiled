using Verse;

namespace RimWorld
{
	public class Thought_MemoryObservationTerror : Thought_MemoryObservation
	{
		public int intensity;

		private Thing target;

		public override string LabelCap => base.CurStage.label.Formatted(target.Named("THING"), pawn.Named("PAWN"));

		public override bool Save => false;

		public override Thing Target
		{
			set
			{
				targetThingID = value.thingIDNumber;
				target = value;
			}
		}
	}
}
