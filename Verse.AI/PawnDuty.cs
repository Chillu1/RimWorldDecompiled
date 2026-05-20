using RimWorld;

namespace Verse.AI;

public class PawnDuty : IExposable
{
	public DutyDef def;

	public LocalTargetInfo focus = LocalTargetInfo.Invalid;

	public LocalTargetInfo focusSecond = LocalTargetInfo.Invalid;

	public LocalTargetInfo focusThird = LocalTargetInfo.Invalid;

	public float radius = -1f;

	public LocomotionUrgency locomotion;

	public Danger maxDanger;

	public CellRect spectateRect;

	public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;

	public SpectateRectSide spectateRectPreferredSide;

	public IntRange spectateDistance = new IntRange(2, 5);

	public bool canDig;

	public int transportersGroup = -1;

	public bool attackDownedIfStarving;

	public float? wanderRadius;

	public int? ropeeLimit;

	public bool pickupOpportunisticWeapon;

	public Rot4 overrideFacing = Rot4.Invalid;

	public ILoadReferenceable source;

	public string tag;

	public RandomSocialMode? socialModeMaxOverride;

	public RandomSocialMode SocialModeMax => socialModeMaxOverride ?? def.socialModeMax;

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

	public PawnDuty(DutyDef def, LocalTargetInfo focus, LocalTargetInfo focusSecond, LocalTargetInfo focusThird, float radius = -1f)
		: this(def, focus, radius)
	{
		this.focusSecond = focusSecond;
		this.focusThird = focusThird;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_TargetInfo.Look(ref focus, saveDestroyedThings: true, "focus", LocalTargetInfo.Invalid);
		Scribe_TargetInfo.Look(ref focusSecond, saveDestroyedThings: true, "focusSecond", LocalTargetInfo.Invalid);
		Scribe_TargetInfo.Look(ref focusThird, saveDestroyedThings: true, "focusThird", LocalTargetInfo.Invalid);
		Scribe_Values.Look(ref radius, "radius", -1f);
		Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.None);
		Scribe_Values.Look(ref maxDanger, "maxDanger", Danger.Unspecified);
		Scribe_Values.Look(ref spectateRect, "spectateRect");
		Scribe_Values.Look(ref spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.All);
		Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
		Scribe_Values.Look(ref transportersGroup, "transportersGroup", -1);
		Scribe_Values.Look(ref attackDownedIfStarving, "attackDownedIfStarving", defaultValue: false);
		Scribe_Values.Look(ref wanderRadius, "wanderRadius");
		Scribe_Values.Look(ref spectateDistance, "spectateDistance");
		Scribe_Values.Look(ref ropeeLimit, "ropeeLimit");
		Scribe_Values.Look(ref pickupOpportunisticWeapon, "pickupOpportunisticWeapon", defaultValue: false);
		Scribe_Values.Look(ref overrideFacing, "overrideFacing", Rot4.Invalid);
		Scribe_References.Look(ref source, "source");
		Scribe_Values.Look(ref tag, "tag");
		Scribe_Values.Look(ref socialModeMaxOverride, "socialModeMaxOverride");
	}

	public override string ToString()
	{
		string text = (focus.IsValid ? focus.ToString() : "");
		string text2 = (focusSecond.IsValid ? (", second=" + focusSecond.ToString()) : "");
		string text3 = (focusThird.IsValid ? (", third=" + focusThird.ToString()) : "");
		string text4 = ((radius > 0f) ? (", rad=" + radius.ToString("F2")) : "");
		return "(" + def?.ToString() + " " + text + text2 + text3 + text4 + ")";
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
