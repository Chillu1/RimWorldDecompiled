using Verse;

namespace RimWorld;

public class FloorEtchingRambling : Building
{
	private CompFloorEtchingRambling comp;

	public override string DescriptionFlavor
	{
		get
		{
			if (comp.deciphered)
			{
				return comp.message;
			}
			return base.DescriptionFlavor;
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		comp = GetComp<CompFloorEtchingRambling>();
	}
}
