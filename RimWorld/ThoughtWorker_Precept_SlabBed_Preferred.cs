using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_SlabBed_Preferred : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return p.mindState.lastBedDefSleptIn != null && p.mindState.lastBedDefSleptIn.building != null && p.mindState.lastBedDefSleptIn.building.bed_slabBed;
		}
	}
}
