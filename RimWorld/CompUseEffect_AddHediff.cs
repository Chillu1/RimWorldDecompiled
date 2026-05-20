using Verse;

namespace RimWorld;

public class CompUseEffect_AddHediff : CompUseEffect
{
	public CompProperties_UseEffectAddHediff Props => (CompProperties_UseEffectAddHediff)props;

	public override void DoEffect(Pawn user)
	{
		user.health.AddHediff(Props.hediffDef);
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (!Props.allowRepeatedUse && p.health.hediffSet.HasHediff(Props.hediffDef))
		{
			return "AlreadyHasHediff".Translate(Props.hediffDef.label);
		}
		return true;
	}
}
