using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ImprisonedByFaction : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			return p.IsPrisoner && p.guest.HostFaction == other.Faction;
		}
	}
}
