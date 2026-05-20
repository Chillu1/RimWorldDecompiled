using Verse;

namespace RimWorld;

public class StatWorker_MeleeDamageAmountTrap : StatWorker_MeleeDamageAmount
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (req.HasThing && req.Thing is Building_Trap { ShouldShowTrapDamageStat: false })
		{
			return false;
		}
		if (req.Def is ThingDef { category: ThingCategory.Building } thingDef)
		{
			return thingDef.building.isTrap;
		}
		return false;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return base.GetValueUnfinalized(req, applyPostProcess) / 5f;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		float num = 5f;
		return base.GetExplanationUnfinalized(req, numberSense) + "Stat_TrapDamageHitCount".Translate(num) + ": x" + (1f / num).ToStringPercent();
	}

	protected override DamageArmorCategoryDef CategoryOfDamage(ThingDef def)
	{
		return def.building.trapDamageCategory;
	}
}
