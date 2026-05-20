using RimWorld;

namespace Verse;

public class HediffComp_VatGrowing : HediffComp
{
	public override bool CompShouldRemove
	{
		get
		{
			if (!base.Pawn.Spawned)
			{
				return !(base.Pawn.ParentHolder is Building_GrowthVat);
			}
			return true;
		}
	}

	public override string CompTipStringExtra => string.Concat("AgingSpeed".Translate() + ": x", 20.ToString());
}
