using Verse;

namespace RimWorld;

public class Thought_Memory : Thought
{
	public float moodPowerFactor = 1f;

	public int moodOffset;

	public Pawn otherPawn;

	public bool permanent;

	public int age;

	private int forcedStage;

	public int durationTicksOverride = -1;

	private string cachedLabelCap;

	private Pawn cachedLabelCapForOtherPawn;

	private int cachedLabelCapForStageIndex = -1;

	public override bool VisibleInNeedsTab
	{
		get
		{
			if (base.VisibleInNeedsTab)
			{
				return !ShouldDiscard;
			}
			return false;
		}
	}

	public override int DurationTicks
	{
		get
		{
			if (durationTicksOverride < 0)
			{
				return def.DurationTicks;
			}
			return durationTicksOverride;
		}
	}

	public override int CurStageIndex => forcedStage;

	public virtual bool ShouldDiscard
	{
		get
		{
			if (!permanent)
			{
				return age > DurationTicks;
			}
			return false;
		}
	}

	public override string LabelCap
	{
		get
		{
			if (cachedLabelCap == null || cachedLabelCapForOtherPawn != otherPawn || cachedLabelCapForStageIndex != CurStageIndex)
			{
				if (otherPawn != null)
				{
					cachedLabelCap = base.CurStage.label.Formatted(otherPawn.LabelShort, otherPawn).CapitalizeFirst();
					if (def.Worker != null)
					{
						cachedLabelCap = def.Worker.PostProcessLabel(pawn, cachedLabelCap);
					}
				}
				else
				{
					cachedLabelCap = base.LabelCap;
				}
				cachedLabelCapForOtherPawn = otherPawn;
				cachedLabelCapForStageIndex = CurStageIndex;
			}
			return cachedLabelCap;
		}
	}

	public override string LabelCapSocial
	{
		get
		{
			string text = ((base.CurStage.labelSocial == null) ? base.LabelCapSocial : ((string)base.CurStage.LabelSocialCap.Formatted(pawn.Named("PAWN"), otherPawn.Named("OTHERPAWN"))));
			if (sourcePrecept != null)
			{
				text += " (" + "Ideo".Translate() + ")";
			}
			return text;
		}
	}

	public virtual bool Save => true;

	public void SetForcedStage(int stageIndex)
	{
		forcedStage = stageIndex;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref otherPawn, "otherPawn", saveDestroyedThings: true);
		Scribe_Values.Look(ref moodPowerFactor, "moodPowerFactor", 1f);
		Scribe_Values.Look(ref moodOffset, "moodOffset", 0);
		Scribe_Values.Look(ref age, "age", 0);
		Scribe_Values.Look(ref forcedStage, "stageIndex", 0);
		Scribe_Values.Look(ref durationTicksOverride, "durationTicksOverride", -1);
		Scribe_Values.Look(ref permanent, "permanent", defaultValue: false);
	}

	public virtual void ThoughtInterval()
	{
		age += 150;
	}

	public void Renew()
	{
		age = 0;
	}

	public virtual bool TryMergeWithExistingMemory(out bool showBubble)
	{
		ThoughtHandler thoughts = pawn.needs.mood.thoughts;
		if (thoughts.memories.NumMemoriesInGroup(this) >= def.stackLimit)
		{
			Thought_Memory thought_Memory = thoughts.memories.OldestMemoryInGroup(this);
			if (thought_Memory != null)
			{
				showBubble = thought_Memory.age > thought_Memory.DurationTicks / 2;
				thought_Memory.Renew();
				return true;
			}
		}
		showBubble = true;
		return false;
	}

	public override bool GroupsWith(Thought other)
	{
		if (!(other is Thought_Memory thought_Memory))
		{
			return false;
		}
		if (base.GroupsWith(other))
		{
			if (otherPawn != thought_Memory.otherPawn)
			{
				return LabelCap == thought_Memory.LabelCap;
			}
			return true;
		}
		return false;
	}

	public override float MoodOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		float num = base.MoodOffset();
		num *= moodPowerFactor;
		num += (float)moodOffset;
		if (def.lerpMoodToZero)
		{
			num *= 1f - (float)age / (float)DurationTicks;
		}
		return num;
	}

	public virtual void Notify_NewThoughtInGroupAdded(Thought_Memory memory)
	{
	}

	public override string ToString()
	{
		return "(" + def.defName + ", moodPowerFactor=" + moodPowerFactor + ", moodOffset=" + moodOffset + ", age=" + age + ")";
	}

	public virtual void CopyFrom(Thought_Memory m)
	{
		age = m.age;
		pawn = m.pawn;
		sourcePrecept = m.sourcePrecept;
		moodOffset = m.moodOffset;
		otherPawn = m.otherPawn;
		durationTicksOverride = m.durationTicksOverride;
		moodPowerFactor = m.moodPowerFactor;
		SetForcedStage(m.CurStageIndex);
	}
}
