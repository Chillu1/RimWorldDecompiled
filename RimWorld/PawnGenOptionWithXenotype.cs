namespace RimWorld
{
	public struct PawnGenOptionWithXenotype
	{
		private PawnGenOption option;

		private XenotypeDef xenotype;

		private float selectionWeight;

		public PawnGenOption Option => option;

		public XenotypeDef Xenotype => xenotype;

		public float SelectionWeight => selectionWeight;

		public float Cost
		{
			get
			{
				if (xenotype == null)
				{
					return option.Cost;
				}
				return option.Cost * xenotype.combatPowerFactor;
			}
		}

		public PawnGenOptionWithXenotype(PawnGenOption option, XenotypeDef xenotype, float selectionWeight)
		{
			this.option = option;
			this.xenotype = xenotype;
			this.selectionWeight = selectionWeight;
		}
	}
}
