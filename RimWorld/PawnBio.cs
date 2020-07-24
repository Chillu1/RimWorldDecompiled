using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	[CaseInsensitiveXMLParsing]
	public class PawnBio
	{
		public GenderPossibility gender;

		public NameTriple name;

		public Backstory childhood;

		public Backstory adulthood;

		public bool pirateKing;

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

		public void PostLoad()
		{
			if (childhood != null)
			{
				childhood.PostLoad();
			}
			if (adulthood != null)
			{
				adulthood.PostLoad();
			}
		}

		public void ResolveReferences()
		{
			if (adulthood.spawnCategories.Count == 1 && adulthood.spawnCategories[0] == "Trader")
			{
				adulthood.spawnCategories.Add("Civil");
			}
			if (childhood != null)
			{
				childhood.ResolveReferences();
			}
			if (adulthood != null)
			{
				adulthood.ResolveReferences();
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (childhood != null)
			{
				foreach (string item in childhood.ConfigErrors(ignoreNoSpawnCategories: true))
				{
					yield return string.Concat(name, ", ", childhood.title, ": ", item);
				}
			}
			if (adulthood == null)
			{
				yield break;
			}
			foreach (string item2 in adulthood.ConfigErrors(ignoreNoSpawnCategories: false))
			{
				yield return string.Concat(name, ", ", adulthood.title, ": ", item2);
			}
		}

		public override string ToString()
		{
			return string.Concat("PawnBio(", name, ")");
		}
	}
}
