using System;
using RimWorld;

namespace Verse.AI
{
	public class PawnDuty : IExposable
	{
		public DutyDef def;

		public LocalTargetInfo focus = LocalTargetInfo.Invalid;

		public LocalTargetInfo focusSecond = LocalTargetInfo.Invalid;

		public float radius = -1f;

		public LocomotionUrgency locomotion;

		public Danger maxDanger;

		public CellRect spectateRect;

		public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;

		public SpectateRectSide spectateRectPreferredSide;

		public bool canDig;

		[Obsolete]
		public PawnsToGather pawnsToGather;

		public int transportersGroup = -1;

		public bool attackDownedIfStarving;

		public float? wanderRadius;

		public PawnDuty()
		{
		}

		public PawnDuty(DutyDef def)
		{
			this.def = def;
		}

		public PawnDuty(DutyDef def, LocalTargetInfo focus, float radius = -1f)
			: this(def)
		{
			this.focus = focus;
			this.radius = radius;
		}

		public PawnDuty(DutyDef def, LocalTargetInfo focus, LocalTargetInfo focusSecond, float radius = -1f)
			: this(def, focus, radius)
		{
			this.focusSecond = focusSecond;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_TargetInfo.Look(ref focus, "focus", LocalTargetInfo.Invalid);
			Scribe_TargetInfo.Look(ref focusSecond, "focusSecond", LocalTargetInfo.Invalid);
			Scribe_Values.Look(ref radius, "radius", -1f);
			Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.None);
			Scribe_Values.Look(ref maxDanger, "maxDanger", Danger.Unspecified);
			Scribe_Values.Look(ref spectateRect, "spectateRect");
			Scribe_Values.Look(ref spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.All);
			Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
			Scribe_Values.Look(ref transportersGroup, "transportersGroup", -1);
			Scribe_Values.Look(ref attackDownedIfStarving, "attackDownedIfStarving", defaultValue: false);
			Scribe_Values.Look(ref wanderRadius, "wanderRadius");
		}

		public override string ToString()
		{
			string text = (focus.IsValid ? focus.ToString() : "");
			string text2 = (focusSecond.IsValid ? (", second=" + focusSecond.ToString()) : "");
			string text3 = ((radius > 0f) ? (", rad=" + radius.ToString("F2")) : "");
			return string.Concat("(", def, " ", text, text2, text3, ")");
		}

		internal void DrawDebug(Pawn pawn)
		{
			if (focus.IsValid)
			{
				GenDraw.DrawLineBetween(pawn.DrawPos, focus.Cell.ToVector3Shifted());
				if (radius > 0f)
				{
					GenDraw.DrawRadiusRing(focus.Cell, radius);
				}
			}
		}
	}
}
