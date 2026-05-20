using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class Thought : IExposable
{
	public Pawn pawn;

	public ThoughtDef def;

	public Precept sourcePrecept;

	[Unsaved(false)]
	public float cachedMoodOffsetOfGroup;

	[Unsaved(false)]
	private string cachedBabyText;

	private static readonly Texture2D DefaultGoodIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericGood");

	private static readonly Texture2D DefaultBadIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericBad");

	public abstract int CurStageIndex { get; }

	public virtual int DurationTicks => def.DurationTicks;

	public ThoughtStage CurStage => def.stages[CurStageIndex];

	public virtual bool VisibleInNeedsTab => CurStage.visible;

	public virtual string LabelCap
	{
		get
		{
			if (def.Worker == null)
			{
				return CurStage.LabelCap.Formatted(pawn.Named("PAWN")).CapitalizeFirst();
			}
			return def.Worker.PostProcessLabel(pawn, CurStage.LabelCap).CapitalizeFirst();
		}
	}

	protected virtual float BaseMoodOffset => CurStage.baseMoodEffect;

	public virtual string LabelCapSocial
	{
		get
		{
			if (CurStage.labelSocial != null)
			{
				return CurStage.LabelSocialCap.Formatted(pawn.Named("PAWN"));
			}
			return LabelCap;
		}
	}

	public virtual string Description
	{
		get
		{
			string description = CurStage.description;
			if (description == null)
			{
				description = def.description;
			}
			description = ((def.Worker != null) ? def.Worker.PostProcessDescription(pawn, description) : ((!(this is Thought_Memory { otherPawn: not null } thought_Memory)) ? ((!(this is ISocialThought socialThought) || socialThought.OtherPawn() == null) ? description.Formatted(pawn.Named("PAWN")).Resolve() : description.Formatted(pawn.Named("PAWN"), socialThought.OtherPawn().Named("OTHERPAWN")).Resolve()) : description.Formatted(pawn.Named("PAWN"), thought_Memory.otherPawn.Named("OTHERPAWN")).Resolve()));
			if (ModsConfig.IdeologyActive && sourcePrecept != null && !Find.IdeoManager.classicMode)
			{
				description += CausedByBeliefInPrecept;
			}
			string text = ThoughtUtility.ThoughtNullifiedMessage(pawn, def);
			if (!string.IsNullOrEmpty(text))
			{
				description = description + "\n\n(" + text + ")";
			}
			return description;
		}
	}

	public virtual string BabyTalk => cachedBabyText ?? (cachedBabyText = ThoughtUtility.GenerateBabyTalk(Description.Split('\n')[0]));

	public string CausedByBeliefInPrecept => "\n\n" + "CausedBy".Translate() + ": " + "BeliefInIdeo".Translate() + " " + sourcePrecept.ideo.name.Colorize(sourcePrecept.ideo.TextColor) + (sourcePrecept.def.label.NullOrEmpty() ? string.Empty : (" (" + sourcePrecept.def.issue.LabelCap.ToString() + ": " + sourcePrecept.def.LabelCap.ToString() + ")"));

	public Texture2D Icon
	{
		get
		{
			if (def.Icon != null)
			{
				return def.Icon;
			}
			if (MoodOffset() > 0f)
			{
				return DefaultGoodIcon;
			}
			return DefaultBadIcon;
		}
	}

	public virtual void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving && sourcePrecept?.ideo != null && !Find.IdeoManager.IdeosListForReading.Contains(sourcePrecept.ideo))
		{
			sourcePrecept = null;
		}
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref sourcePrecept, "sourcePrecept");
	}

	public virtual float MoodOffset()
	{
		if (CurStage == null)
		{
			Log.Error("CurStage is null while ShouldDiscard is false on " + def.defName + " for " + pawn);
			return 0f;
		}
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		float num = BaseMoodOffset;
		if (def.effectMultiplyingStat != null)
		{
			num = ((def.effectMultiplyingStatCurve == null) ? (num * pawn.GetStatValue(def.effectMultiplyingStat)) : (num * def.effectMultiplyingStatCurve.Evaluate(pawn.GetStatValue(def.effectMultiplyingStat))));
		}
		if (def.Worker != null)
		{
			num *= def.Worker.MoodMultiplier(pawn);
		}
		return num;
	}

	public virtual bool GroupsWith(Thought other)
	{
		if (def == other.def)
		{
			if (CurStageIndex != other.CurStageIndex)
			{
				return def.stagesStack;
			}
			return true;
		}
		return false;
	}

	public virtual void Init()
	{
	}

	public override string ToString()
	{
		return "(" + def.defName + ")";
	}
}
