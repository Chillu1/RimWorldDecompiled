using Verse;

namespace RimWorld
{
	public class JobDriver_Uninstall : JobDriver_RemoveBuilding
	{
		protected override DesignationDef Designation => DesignationDefOf.Uninstall;

		protected override float TotalNeededWork => base.TargetA.Thing.def.building.uninstallWork;

		protected override void FinishedRemoving()
		{
			base.Building.Uninstall();
			pawn.records.Increment(RecordDefOf.ThingsUninstalled);
		}
	}
}
