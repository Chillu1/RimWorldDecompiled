using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitGrave : JobDriver_VisitJoyThing
	{
		private Building_Grave Grave => (Building_Grave)job.GetTarget(TargetIndex.A).Thing;

		protected override void WaitTickAction()
		{
			float num = 1f;
			Room room = pawn.GetRoom();
			if (room != null)
			{
				num *= room.GetStat(RoomStatDefOf.GraveVisitingJoyGainFactor);
			}
			pawn.GainComfortFromCellIfPossible();
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, num, Grave);
		}

		public override object[] TaleParameters()
		{
			return new object[2]
			{
				pawn,
				(Grave.Corpse != null) ? Grave.Corpse.InnerPawn : null
			};
		}
	}
}
