using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Thought : IExposable
	{
		public Pawn pawn;

		public ThoughtDef def;

		private static readonly Texture2D DefaultGoodIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericGood");

		private static readonly Texture2D DefaultBadIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericBad");

		public abstract int CurStageIndex
		{
			get;
		}

		public ThoughtStage CurStage => def.stages[CurStageIndex];

		public virtual bool VisibleInNeedsTab => CurStage.visible;

		public virtual string LabelCap
		{
			get
			{
				if (def.Worker == null)
				{
					return CurStage.LabelCap.Formatted(pawn.Named("PAWN"));
				}
				return def.Worker.PostProcessLabel(pawn, CurStage.LabelCap);
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
				Thought_Memory thought_Memory;
				ISocialThought socialThought;
				description = ((def.Worker != null) ? def.Worker.PostProcessDescription(pawn, description) : (((thought_Memory = this as Thought_Memory) != null && thought_Memory.otherPawn != null) ? ((string)description.Formatted(pawn.Named("PAWN"), thought_Memory.otherPawn.Named("OTHERPAWN"))) : (((socialThought = this as ISocialThought) == null || socialThought.OtherPawn() == null) ? ((string)description.Formatted(pawn.Named("PAWN"))) : ((string)description.Formatted(pawn.Named("PAWN"), socialThought.OtherPawn().Named("OTHERPAWN"))))));
				string text = ThoughtUtility.ThoughtNullifiedMessage(pawn, def);
				if (!string.IsNullOrEmpty(text))
				{
					description = description + "\n\n(" + text + ")";
				}
				return description;
			}
		}

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
			Scribe_Defs.Look(ref def, "def");
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
				num *= pawn.GetStatValue(def.effectMultiplyingStat);
			}
			if (def.Worker != null)
			{
				num *= def.Worker.MoodMultiplier(pawn);
			}
			return num;
		}

		public virtual bool GroupsWith(Thought other)
		{
			return def == other.def;
		}

		public virtual void Init()
		{
		}

		public override string ToString()
		{
			return "(" + def.defName + ")";
		}
	}
}
