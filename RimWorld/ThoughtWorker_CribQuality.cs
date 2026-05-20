using Verse;

namespace RimWorld
{
	public class ThoughtWorker_CribQuality : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Building_Bed building_Bed = p.CurrentBed();
			if (building_Bed == null || !building_Bed.def.building.bed_crib)
			{
				return false;
			}
			building_Bed.TryGetQuality(out var qc);
			return ThoughtState.ActiveAtStage((int)qc);
		}
	}
}
