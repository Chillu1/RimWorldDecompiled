using Verse;

namespace RimWorld
{
	public abstract class RitualObligationTrigger : IExposable
	{
		public Precept_Ritual ritual;

		protected bool mustBePlayerIdeo;

		public virtual string TriggerExtraDesc => null;

		public virtual void Init(RitualObligationTriggerProperties props)
		{
			mustBePlayerIdeo = props.mustBePlayerIdeo;
		}

		public virtual void Tick()
		{
		}

		public virtual void Notify_MemberChangedFaction(Pawn p, Faction oldFaction, Faction newFaction)
		{
		}

		public virtual void Notify_MemberGenerated(Pawn pawn)
		{
		}

		public virtual void Notify_MemberDied(Pawn p)
		{
		}

		public virtual void Notify_MemberCorpseDestroyed(Pawn p)
		{
		}

		public virtual void Notify_MemberLost(Pawn p)
		{
		}

		public virtual void Notify_MemberGained(Pawn p)
		{
		}

		public virtual void Notify_GameStarted()
		{
		}

		public virtual void Notify_IdeoReformed()
		{
		}

		public virtual void Notify_RitualExecuted(LordJob_Ritual ritual)
		{
		}

		public virtual void Notify_MemberGuestStatusChanged(Pawn pawn)
		{
		}

		public virtual void CopyTo(RitualObligationTrigger other)
		{
			other.ritual = ritual;
			other.mustBePlayerIdeo = mustBePlayerIdeo;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref mustBePlayerIdeo, "mustBePlayerIdeo", defaultValue: false);
		}
	}
}
