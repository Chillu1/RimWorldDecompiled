using Verse;

namespace RimWorld
{
	public class QuestPart_TransporterPawns_TendWithMedicine : QuestPart_TransporterPawns_Tend
	{
		public ThingDef medicineDef;

		public bool allowSelfTend;

		protected override void DoTend(Pawn pawn)
		{
			Pawn doctor = null;
			if (allowSelfTend && pawn.playerSettings != null && pawn.playerSettings.selfTend && pawn.GetStatValue(StatDefOf.MedicalTendQuality) > 0.75f)
			{
				doctor = pawn;
			}
			Medicine medicine = (Medicine)ThingMaker.MakeThing(medicineDef);
			TendUtility.DoTend(doctor, pawn, medicine);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref medicineDef, "medicineDef");
			Scribe_Values.Look(ref allowSelfTend, "allowSelfTend", defaultValue: false);
		}
	}
}
