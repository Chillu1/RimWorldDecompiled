namespace RimWorld.QuestGen;

public class QuestNode_AddShipJob_Unload : QuestNode_AddShipJob
{
	public SlateRef<TransportShipDropMode?> dropMode;

	protected override ShipJobDef DefaultShipJobDef => ShipJobDefOf.Unload;

	protected override void AddJobVars(ShipJob shipJob, Slate slate)
	{
		if (shipJob is ShipJob_Unload shipJob_Unload)
		{
			shipJob_Unload.dropMode = dropMode.GetValue(slate) ?? TransportShipDropMode.All;
		}
	}
}
