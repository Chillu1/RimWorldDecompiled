using Verse;

namespace RimWorld;

public class PsychicRitualRoleDef_DeathRefusalTarget : PsychicRitualRoleDef
{
	public enum DeathRefusalTargetReason
	{
		None,
		MaxDeathRefusal
	}

	[MustTranslate]
	public string psychicRitualLeaveReason_MaxDeathRefusal;

	protected override bool PawnCanDo(Context context, Pawn pawn, TargetInfo target, out AnyEnum reason)
	{
		if (!base.PawnCanDo(context, pawn, target, out reason))
		{
			return false;
		}
		Hediff_DeathRefusal firstHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
		if (firstHediff != null && firstHediff.UsesLeft >= firstHediff.MaxUses)
		{
			reason = AnyEnum.FromEnum(DeathRefusalTargetReason.MaxDeathRefusal);
			return false;
		}
		return true;
	}

	public override TaggedString PawnCannotDoReason(AnyEnum reason, Context context, Pawn pawn, TargetInfo target)
	{
		DeathRefusalTargetReason? deathRefusalTargetReason = reason.As<DeathRefusalTargetReason>();
		if (deathRefusalTargetReason.HasValue)
		{
			deathRefusalTargetReason.GetValueOrDefault();
			return psychicRitualLeaveReason_MaxDeathRefusal.Formatted(pawn.Named("PAWN"));
		}
		return base.PawnCannotDoReason(reason, context, pawn, target);
	}
}
