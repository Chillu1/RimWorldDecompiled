using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDamageAmountTrap : StatWorker_MeleeDamageAmount
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef != null && thingDef.category == ThingCategory.Building)
			{
				return thingDef.building.isTrap;
			}
			return false;
		}

		protected override DamageArmorCategoryDef CategoryOfDamage(ThingDef def)
		{
			return def.building.trapDamageCategory;
		}
	}
}
