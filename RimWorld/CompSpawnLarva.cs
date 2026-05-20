namespace RimWorld;

public class CompSpawnLarva : CompSpawnPawnOnDestroyed
{
	protected override bool JoinLord => parent.Faction != Faction.OfPlayer;
}
