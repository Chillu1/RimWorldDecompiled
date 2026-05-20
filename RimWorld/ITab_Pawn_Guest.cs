namespace RimWorld;

public class ITab_Pawn_Guest : ITab_Pawn_Visitor
{
	public override bool IsVisible
	{
		get
		{
			if (SelPawn.HostFaction == Faction.OfPlayer && !SelPawn.IsPrisoner)
			{
				return !SelPawn.IsSlaveOfColony;
			}
			return false;
		}
	}

	public ITab_Pawn_Guest()
	{
		labelKey = "TabGuest";
		tutorTag = "Guest";
	}
}
