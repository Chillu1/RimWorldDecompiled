using Verse;

namespace RimWorld;

[CaseInsensitiveXMLParsing]
public class PawnBio
{
	public GenderPossibility gender;

	public NameTriple name;

	public BackstoryDef childhood;

	public BackstoryDef adulthood;

	public bool pirateKing;

	public bool rare;

	public PawnBioType BioType
	{
		get
		{
			if (pirateKing)
			{
				return PawnBioType.PirateKing;
			}
			if (adulthood != null)
			{
				return PawnBioType.BackstoryInGame;
			}
			return PawnBioType.Undefined;
		}
	}

	public void ResolveReferences()
	{
		if (adulthood.spawnCategories.Count == 1 && adulthood.spawnCategories[0] == "Trader")
		{
			adulthood.spawnCategories.Add("Civil");
		}
	}

	public override string ToString()
	{
		return "PawnBio(" + name?.ToString() + ")";
	}
}
