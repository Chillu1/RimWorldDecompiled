using RimWorld.Planet;

namespace RimWorld;

public class PawnGroupMakerParms
{
	public PawnGroupKindDef groupKind;

	public PlanetTile tile = PlanetTile.Invalid;

	public bool inhabitants;

	public float points;

	public Faction faction;

	public Ideo ideo;

	public TraderKindDef traderKind;

	public bool generateFightersOnly;

	public bool dontUseSingleUseRocketLaunchers;

	public RaidStrategyDef raidStrategy;

	public bool forceOneDowned;

	public int? seed;

	public RaidAgeRestrictionDef raidAgeRestriction;

	public bool ignoreGroupCommonality;

	public override string ToString()
	{
		string[] obj = new string[26]
		{
			"groupKind=",
			groupKind?.ToString(),
			", tile=",
			tile.ToString(),
			", inhabitants=",
			inhabitants.ToString(),
			", points=",
			points.ToString(),
			", faction=",
			faction?.ToString(),
			", ideo=",
			ideo?.name,
			", traderKind=",
			traderKind?.ToString(),
			", generateFightersOnly=",
			generateFightersOnly.ToString(),
			", dontUseSingleUseRocketLaunchers=",
			dontUseSingleUseRocketLaunchers.ToString(),
			", raidStrategy=",
			raidStrategy?.ToString(),
			", forceOneDowned=",
			forceOneDowned.ToString(),
			", seed=",
			null,
			null,
			null
		};
		int? num = seed;
		obj[23] = num.ToString();
		obj[24] = ", raidAgeRestriction=";
		obj[25] = raidAgeRestriction?.ToString();
		return string.Concat(obj);
	}
}
