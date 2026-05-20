using System.Text;
using RimWorld;

namespace Verse;

public static class TooltipUtility
{
	public static string ShotCalculationTipString(Thing target)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Find.Selector.SingleSelectedThing != null)
		{
			Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
			Verb verb = null;
			if (singleSelectedThing is Pawn pawn && pawn != target && pawn.equipment?.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile && (!pawn.IsPlayerControlled || pawn.Drafted))
			{
				verb = pawn.equipment.PrimaryEq.PrimaryVerb;
			}
			else if (singleSelectedThing is Building_TurretGun building_TurretGun && building_TurretGun != target)
			{
				verb = building_TurretGun.AttackVerb;
			}
			if (verb != null)
			{
				stringBuilder.Append("ShotBy".Translate(Find.Selector.SingleSelectedThing.LabelShort, Find.Selector.SingleSelectedThing) + ": ");
				if (verb.CanHitTarget(target))
				{
					stringBuilder.Append(ShotReport.HitReportFor(verb.caster, verb, target).GetTextReadout());
				}
				else
				{
					stringBuilder.AppendLine("CannotHit".Translate());
				}
				if (target is Pawn { Faction: null, InAggroMentalState: false } pawn2 && pawn2.AnimalOrWildMan())
				{
					float num = ((!verb.IsMeleeAttack) ? PawnUtility.GetManhunterOnDamageChance(pawn2, singleSelectedThing) : PawnUtility.GetManhunterOnDamageChance(pawn2, singleSelectedThing, 0f));
					if (num > 0f)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine(string.Format("{0}: {1}", "ManhunterPerHit".Translate(), num.ToStringPercent()));
					}
				}
			}
		}
		return stringBuilder.ToString();
	}
}
