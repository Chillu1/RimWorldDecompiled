using Verse;

namespace RimWorld;

public class Building_HackableDoor : Building_SupportedDoor, IHackable
{
	private CompHackable hackableInt;

	public CompHackable Hackable => hackableInt ?? (hackableInt = GetComp<CompHackable>());

	public bool Locked => !Hackable.IsHacked;

	protected override bool CheckFaction => false;

	public override bool PawnCanOpen(Pawn p)
	{
		if (!Locked)
		{
			return base.PawnCanOpen(p);
		}
		return false;
	}

	public void OnLockedOut(Pawn pawn = null)
	{
	}

	public void OnHacked(Pawn pawn = null)
	{
		if (pawn?.Faction != null)
		{
			SetFaction(pawn.Faction);
		}
	}
}
