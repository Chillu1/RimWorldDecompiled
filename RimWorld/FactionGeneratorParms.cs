namespace RimWorld
{
	public struct FactionGeneratorParms
	{
		public FactionDef factionDef;

		public IdeoGenerationParms ideoGenerationParms;

		public bool? hidden;

		public FactionGeneratorParms(FactionDef factionDef, IdeoGenerationParms ideoGenerationParms = default(IdeoGenerationParms), bool? hidden = null)
		{
			this.factionDef = factionDef;
			this.ideoGenerationParms = ideoGenerationParms;
			this.hidden = hidden;
		}
	}
}
