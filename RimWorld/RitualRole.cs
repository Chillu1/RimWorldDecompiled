using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualRole : ILordJobRole, IExposable, ILoadReferenceable
{
	[MustTranslate]
	protected string label;

	[MustTranslate]
	protected string categoryLabel;

	[MustTranslate]
	public string missingDesc;

	[MustTranslate]
	public string noCandidatesGizmoDesc;

	[MustTranslate]
	public string customChildDisallowMessage;

	[NoTranslate]
	public string id;

	public PreceptDef precept;

	public int maxCount;

	[NoTranslate]
	public string mergeId;

	public bool required;

	public bool substitutable;

	public bool ignoreBleeding;

	public bool countsAsParticipant = true;

	public bool addToLord = true;

	public bool allowNonAggroMentalState;

	public bool defaultForSelectedColonist;

	public bool allowOtherIdeos;

	public bool allowDowned;

	public bool allowKeepLayingDown;

	public bool allowChild = true;

	public bool allowBaby;

	public bool removeFromAssignmentsOnDeath = true;

	public bool endJobOnRitualCleanup = true;

	public bool mustBeAbleToReachTarget;

	public bool blocksSocialInteraction;

	private int loadID = -1;

	public virtual bool Animal => false;

	public int MaxCount => maxCount;

	public int MinCount
	{
		get
		{
			if (!required)
			{
				return 0;
			}
			return 1;
		}
	}

	public TaggedString Label => label;

	public TaggedString LabelCap => label.CapitalizeFirst();

	public TaggedString CategoryLabel => categoryLabel;

	public TaggedString CategoryLabelCap => (categoryLabel ?? label).CapitalizeFirst();

	public RitualRole()
	{
		if (Find.UniqueIDsManager != null && loadID == -1)
		{
			loadID = Find.UniqueIDsManager.GetNextRitualRoleID();
		}
	}

	public virtual bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
	{
		if (!AppliesIfChild(p, out reason, skipReason))
		{
			return false;
		}
		return AppliesToRole(p.Ideo?.GetRole(p), out reason, ritual?.Ritual ?? assignments?.Ritual ?? precept, p, skipReason);
	}

	public Precept_Role FindInstance(Ideo ideo)
	{
		foreach (Precept_Role item in ideo.RolesListForReading)
		{
			if (item.def == precept)
			{
				return item;
			}
		}
		return null;
	}

	protected bool AppliesIfChild(Pawn p, out string reason, bool skipReason = false)
	{
		reason = null;
		if (ModsConfig.BiotechActive && !allowChild && p.DevelopmentalStage.Juvenile())
		{
			if (!skipReason)
			{
				reason = customChildDisallowMessage ?? ((string)"MessageRitualRoleCannotBeChild".Translate(Label).CapitalizeFirst());
			}
			return false;
		}
		return true;
	}

	public abstract bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn pawn = null, bool skipReason = false);

	protected virtual int PawnDesirability(Pawn pawn)
	{
		if (precept == null)
		{
			return 0;
		}
		if (pawn.Ideo?.GetRole(pawn)?.def == precept)
		{
			return 1;
		}
		return 0;
	}

	public virtual void OrderByDesirability(List<Pawn> pawns)
	{
		pawns.Sort((Pawn lhs, Pawn rhs) => PawnDesirability(rhs).CompareTo(PawnDesirability(lhs)));
	}

	public virtual string ExtraInfoForDialog(IEnumerable<Pawn> selected)
	{
		return null;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref label, "label");
		Scribe_Values.Look(ref missingDesc, "missingDesc");
		Scribe_Values.Look(ref customChildDisallowMessage, "customChildDisallowMessage");
		Scribe_Values.Look(ref id, "id");
		Scribe_Values.Look(ref maxCount, "maxCount", 0);
		Scribe_Values.Look(ref required, "required", defaultValue: false);
		Scribe_Values.Look(ref substitutable, "substitutable", defaultValue: false);
		Scribe_Values.Look(ref countsAsParticipant, "countsAsParticipant", defaultValue: true);
		Scribe_Values.Look(ref loadID, "loadID", -1);
		Scribe_Values.Look(ref defaultForSelectedColonist, "defaultForSelectedColonist", defaultValue: false);
		Scribe_Defs.Look(ref precept, "precept");
		Scribe_Values.Look(ref mergeId, "mergeId");
		Scribe_Values.Look(ref allowOtherIdeos, "allowOtherIdeos", defaultValue: false);
		Scribe_Values.Look(ref allowDowned, "allowDowned", defaultValue: false);
		Scribe_Values.Look(ref allowKeepLayingDown, "allowKeepLayingDown", defaultValue: false);
		Scribe_Values.Look(ref allowChild, "allowChild", defaultValue: false);
		Scribe_Values.Look(ref endJobOnRitualCleanup, "endJobOnRitualCleanup", defaultValue: false);
		Scribe_Values.Look(ref mustBeAbleToReachTarget, "mustBeAbleToReachTarget", defaultValue: false);
		Scribe_Values.Look(ref removeFromAssignmentsOnDeath, "removeFromAssignmentsOnDeath", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && loadID == -1)
		{
			loadID = Find.UniqueIDsManager.GetNextRitualRoleID();
		}
	}

	public string GetUniqueLoadID()
	{
		return "RitualRole_" + loadID;
	}
}
