using Verse;

namespace RimWorld;

public class CompUseEffect_FixWorstHealthCondition : CompUseEffect
{
	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (!HealthUtility.TryGetWorstHealthCondition(p, out var _, out var _))
		{
			return "AbilityCannotCastNoHealableInjury".Translate(p.Named("PAWN")).Resolve().StripTags() ?? "";
		}
		return true;
	}

	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		TaggedString taggedString = HealthUtility.FixWorstHealthCondition(usedBy);
		if (PawnUtility.ShouldSendNotificationAbout(usedBy))
		{
			Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}
}
