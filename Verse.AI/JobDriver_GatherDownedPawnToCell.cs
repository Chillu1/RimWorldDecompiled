using Verse.AI.Group;

namespace Verse.AI;

public class JobDriver_GatherDownedPawnToCell : JobDriver_HaulToCell
{
	protected override bool DropCarriedThingIfNotTarget => true;

	protected override Toil BeforeDrop()
	{
		return Toils_General.DoAtomic(delegate
		{
			if (base.pawn.carryTracker.CarriedThing is Pawn pawn)
			{
				base.pawn.GetLord().CurLordToil?.Notify_ReachedDutyLocation(pawn);
			}
			else
			{
				Log.Error("Carried thing wasn't a Pawn in JobDriver_GatherDownedPawnToCell.");
			}
		});
	}
}
