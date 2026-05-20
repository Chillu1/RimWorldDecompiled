using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_AddShipJob_Unload : QuestPart_AddShipJob
	{
		public bool unforbidAll = true;

		public override ShipJob GetShipJob()
		{
			ShipJob_Unload obj = (ShipJob_Unload)ShipJobMaker.MakeShipJob(shipJobDef);
			obj.unforbidAll = unforbidAll;
			return obj;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref unforbidAll, "unforbidAll", defaultValue: false);
		}
	}
}
