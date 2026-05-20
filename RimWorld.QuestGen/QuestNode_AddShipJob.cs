using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddShipJob : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<TransportShip> transportShip;

	public SlateRef<ShipJobDef> jobDef;

	public SlateRef<ShipJobStartMode?> shipJobStartMode;

	protected virtual ShipJobDef DefaultShipJobDef => null;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		ShipJob shipJob = ShipJobMaker.MakeShipJob(jobDef.GetValue(slate) ?? DefaultShipJobDef);
		AddJobVars(shipJob, slate);
		QuestPart_AddShipJob part = new QuestPart_AddShipJob
		{
			inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal")),
			shipJob = shipJob,
			shipJobStartMode = shipJobStartMode.GetValue(slate).GetValueOrDefault(),
			transportShip = transportShip.GetValue(slate)
		};
		QuestGen.quest.AddPart(part);
	}

	protected virtual void AddJobVars(ShipJob shipJob, Slate slate)
	{
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (jobDef.GetValue(slate) == null)
		{
			return DefaultShipJobDef != null;
		}
		return true;
	}
}
