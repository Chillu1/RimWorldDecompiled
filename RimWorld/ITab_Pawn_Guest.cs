namespace RimWorld
{
	public class ITab_Pawn_Guest : ITab_Pawn_Visitor
	{
		public override bool IsVisible
		{
			get
			{
				if (base.SelPawn.HostFaction == Faction.OfPlayer)
				{
					return !base.SelPawn.IsPrisoner;
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
}
