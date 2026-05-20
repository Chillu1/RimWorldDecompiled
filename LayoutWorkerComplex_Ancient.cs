using RimWorld;
using Verse;

public class LayoutWorkerComplex_Ancient : LayoutWorkerComplex
{
	public LayoutWorkerComplex_Ancient(LayoutDef def)
		: base(def)
	{
	}

	public override Faction GetFixedHostileFactionForThreats()
	{
		if (!Rand.Chance(base.Def.fixedHostileFactionChance))
		{
			return null;
		}
		if (Faction.OfInsects != null && Faction.OfMechanoids != null)
		{
			if (!Rand.Bool)
			{
				return Faction.OfMechanoids;
			}
			return Faction.OfInsects;
		}
		if (Faction.OfInsects != null)
		{
			return Faction.OfInsects;
		}
		return Faction.OfMechanoids;
	}
}
