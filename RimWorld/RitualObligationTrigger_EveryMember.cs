using Verse;

namespace RimWorld
{
	public abstract class RitualObligationTrigger_EveryMember : RitualObligationTrigger
	{
		protected abstract void Recache();

		public override void Notify_MemberDied(Pawn p)
		{
			Recache();
		}

		public override void Notify_MemberChangedFaction(Pawn p, Faction oldFaction, Faction newFaction)
		{
			Recache();
		}

		public override void Notify_MemberGenerated(Pawn pawn)
		{
			Recache();
		}

		public override void Notify_GameStarted()
		{
			Recache();
		}

		public override void Notify_RitualExecuted(LordJob_Ritual ritual)
		{
			Recache();
		}

		public override void Notify_MemberGained(Pawn pawn)
		{
			Recache();
		}

		public override void Notify_MemberLost(Pawn pawn)
		{
			Recache();
		}

		public override void Notify_MemberGuestStatusChanged(Pawn pawn)
		{
			Recache();
		}

		public override void Notify_IdeoReformed()
		{
			Recache();
		}
	}
}
