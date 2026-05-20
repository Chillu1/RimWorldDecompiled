using Verse;

namespace RimWorld
{
	public struct GeneDefWithType
	{
		public GeneDef geneDef;

		public bool isXenogene;

		public bool RandomChosen => geneDef.RandomChosen;

		public GeneDefWithType(GeneDef geneDef, bool xenogene)
		{
			this.geneDef = geneDef;
			isXenogene = xenogene;
		}

		public bool ConflictsWith(GeneDefWithType other)
		{
			return geneDef.ConflictsWith(other.geneDef);
		}

		public bool Overrides(GeneDefWithType other)
		{
			return geneDef.Overrides(other.geneDef, isXenogene, other.isXenogene);
		}
	}
}
