using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class InteractionWorker_SanguophageTalk : InteractionWorker
	{
		private const float SelectionWeight_SanguophageMeeting = 999f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (!ModsConfig.BiotechActive)
			{
				return 0f;
			}
			if (initiator.genes == null || recipient.genes == null || initiator.genes.Xenotype != XenotypeDefOf.Sanguophage || recipient.genes.Xenotype != XenotypeDefOf.Sanguophage)
			{
				return 0f;
			}
			Lord lord = initiator.GetLord();
			Lord lord2 = recipient.GetLord();
			if (lord == null || lord2 == null || lord != lord2)
			{
				return 0f;
			}
			if (lord.CurLordToil == null || !(lord.CurLordToil is LordToil_SanguophageMeeting))
			{
				return 0f;
			}
			return 999f;
		}
	}
}
