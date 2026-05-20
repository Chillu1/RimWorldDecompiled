using Verse;

namespace RimWorld
{
	public class QuestPart_AssignMechToMechanitor : QuestPart
	{
		public Pawn mech;

		public Pawn mechanitor;

		public string inSignal;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && MechanitorUtility.IsMechanitor(mechanitor) && mechanitor.mechanitor.CanOverseeSubject(mech))
			{
				mechanitor.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
			}
		}

		public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
		{
			if (mech == pawn)
			{
				mech = null;
			}
			if (mechanitor == pawn)
			{
				mechanitor = null;
			}
		}

		public override bool QuestPartReserves(Pawn p)
		{
			if (p != mech)
			{
				return p == mechanitor;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref mech, "mech");
			Scribe_References.Look(ref mechanitor, "mechanitor");
		}
	}
}
