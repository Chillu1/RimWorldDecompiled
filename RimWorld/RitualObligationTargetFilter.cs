using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class RitualObligationTargetFilter : IExposable
{
	public Precept_Ritual parent;

	public RitualObligationTargetFilterDef def;

	public RitualObligationTargetFilter()
	{
	}

	public RitualObligationTargetFilter(RitualObligationTargetFilterDef def)
	{
		this.def = def;
	}

	public RitualTargetUseReport CanUseTarget(TargetInfo target, RitualObligation obligation)
	{
		if ((target.Thing != null && !target.Thing.Spawned) || Find.IdeoManager.GetActiveRitualOn(target.Thing) != null)
		{
			return false;
		}
		return CanUseTargetInternal(target, obligation);
	}

	public abstract IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map);

	protected abstract RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation);

	public virtual List<string> MissingTargetBuilding(Ideo ideo)
	{
		return null;
	}

	public virtual bool ObligationTargetsValid(RitualObligation obligation)
	{
		return true;
	}

	public abstract IEnumerable<string> GetTargetInfos(RitualObligation obligation);

	public virtual IEnumerable<string> GetBlockingIssues(TargetInfo target, RitualRoleAssignments assignments)
	{
		foreach (RitualRole item in assignments.AllRolesForReading)
		{
			if (item.mustBeAbleToReachTarget)
			{
				Pawn pawn = assignments.FirstAssignedPawn(item);
				if (pawn != null && !pawn.CanReach((LocalTargetInfo)target, PathEndMode.Touch, pawn.NormalMaxDanger()))
				{
					yield return "RitualTargetUnreachable".Translate(item.LabelCap);
				}
			}
		}
	}

	public virtual string LabelExtraPart(RitualObligation obligation)
	{
		return "";
	}

	public virtual bool ShouldGrayOut(Pawn pawn, ILordJobAssignmentsManager<RitualRole> assignments, out TaggedString reason)
	{
		reason = TaggedString.Empty;
		return false;
	}

	public virtual void CopyTo(RitualObligationTargetFilter other)
	{
		other.parent = parent;
		other.def = def;
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref parent, "parent");
	}
}
