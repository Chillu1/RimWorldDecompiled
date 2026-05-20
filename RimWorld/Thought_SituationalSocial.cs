using Verse;

namespace RimWorld;

public class Thought_SituationalSocial : Thought_Situational, ISocialThought
{
	public Pawn otherPawn;

	public override bool VisibleInNeedsTab
	{
		get
		{
			if (base.VisibleInNeedsTab)
			{
				return MoodOffset() != 0f;
			}
			return false;
		}
	}

	public override string LabelCap
	{
		get
		{
			if (!reason.NullOrEmpty())
			{
				TaggedString taggedString = base.CurStage.label.Formatted(reason.Named("REASON"), pawn.Named("PAWN"), otherPawn.Named("OTHERPAWN")).CapitalizeFirst();
				if (def.Worker != null)
				{
					taggedString = def.Worker.PostProcessLabel(pawn, taggedString);
				}
				return taggedString;
			}
			if (def.Worker == null)
			{
				return base.CurStage.LabelCap.Formatted(pawn.Named("PAWN"), otherPawn.Named("OTHERPAWN")).CapitalizeFirst();
			}
			return base.LabelCap;
		}
	}

	public override string LabelCapSocial
	{
		get
		{
			if (base.CurStage.labelSocial != null)
			{
				return base.CurStage.LabelSocialCap.Formatted(pawn.Named("PAWN"), otherPawn.Named("OTHERPAWN"), reason.Named("REASON"));
			}
			return base.LabelCapSocial;
		}
	}

	public Pawn OtherPawn()
	{
		return otherPawn;
	}

	public virtual float OpinionOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		float num = base.CurStage.baseOpinionOffset;
		if (def.effectMultiplyingStat != null)
		{
			num *= pawn.GetStatValue(def.effectMultiplyingStat) * otherPawn.GetStatValue(def.effectMultiplyingStat);
		}
		return num;
	}

	public override bool GroupsWith(Thought other)
	{
		if (!(other is Thought_SituationalSocial thought_SituationalSocial))
		{
			return false;
		}
		if (base.GroupsWith(other))
		{
			return otherPawn == thought_SituationalSocial.otherPawn;
		}
		return false;
	}

	protected override ThoughtState CurrentStateInternal()
	{
		return def.Worker.CurrentSocialState(pawn, otherPawn);
	}
}
