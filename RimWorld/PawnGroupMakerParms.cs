namespace RimWorld
{
	public class PawnGroupMakerParms
	{
		public PawnGroupKindDef groupKind;

		public int tile = -1;

		public bool inhabitants;

		public float points;

		public Faction faction;

		public TraderKindDef traderKind;

		public bool generateFightersOnly;

		public bool dontUseSingleUseRocketLaunchers;

		public RaidStrategyDef raidStrategy;

		public bool forceOneIncap;

		public int? seed;

		public override string ToString()
		{
			return string.Concat("groupKind=", groupKind, ", tile=", tile, ", inhabitants=", inhabitants.ToString(), ", points=", points, ", faction=", faction, ", traderKind=", traderKind, ", generateFightersOnly=", generateFightersOnly.ToString(), ", dontUseSingleUseRocketLaunchers=", dontUseSingleUseRocketLaunchers.ToString(), ", raidStrategy=", raidStrategy, ", forceOneIncap=", forceOneIncap.ToString(), ", seed=", seed);
		}
	}
}
