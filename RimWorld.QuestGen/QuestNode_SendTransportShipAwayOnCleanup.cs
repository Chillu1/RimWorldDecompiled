namespace RimWorld.QuestGen
{
	public class QuestNode_SendTransportShipAwayOnCleanup : QuestNode
	{
		public SlateRef<TransportShip> transportShip;

		public SlateRef<bool> unloadContents;

		public SlateRef<TransportShipDropMode?> unsatisfiedDropMode;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_SendTransportShipAwayOnCleanup part = new QuestPart_SendTransportShipAwayOnCleanup
			{
				transportShip = transportShip.GetValue(slate),
				unsatisfiedDropMode = (unsatisfiedDropMode.GetValue(slate) ?? TransportShipDropMode.NonRequired),
				unloadContents = unloadContents.GetValue(slate)
			};
			QuestGen.quest.AddPart(part);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
