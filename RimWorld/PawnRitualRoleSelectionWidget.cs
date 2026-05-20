using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class PawnRitualRoleSelectionWidget : PawnRoleSelectionWidgetBase<RitualRole>
{
	protected RitualRoleAssignments ritualAssignments;

	protected Precept_Ritual ritual;

	protected TargetInfo target;

	protected RitualOutcomeEffectDef outcome;

	private StringBuilder sb = new StringBuilder();

	private HashSet<StatDef> tmpRelevantStats = new HashSet<StatDef>();

	public override string SpectatorsLabel()
	{
		return ritual?.behavior?.def.spectatorsLabel ?? base.SpectatorsLabel();
	}

	public PawnRitualRoleSelectionWidget(RitualRoleAssignments assignments, Precept_Ritual ritual, TargetInfo target, RitualOutcomeEffectDef outcome)
		: base((ILordJobCandidatePool)assignments, (ILordJobAssignmentsManager<RitualRole>)assignments)
	{
		ritualAssignments = assignments;
		this.ritual = ritual;
		this.target = target;
		this.outcome = outcome;
		showIdeoIcon = ritual?.showIdeoIconsInDialog ?? false;
	}

	public override bool ShouldDrawHighlight(RitualRole highlightedRole, Pawn pawn)
	{
		string reason;
		if (highlightedRole != null && !DragAndDropWidget.Dragging && !assignments.AssignedPawns(highlightedRole).Contains(pawn))
		{
			return highlightedRole.AppliesToPawn(pawn, out reason, target, null, ritualAssignments, ritual, skipReason: true);
		}
		return false;
	}

	public override void Notify_AssignmentsChanged()
	{
		if (outcome == null)
		{
			return;
		}
		foreach (RitualOutcomeComp comp in outcome.comps)
		{
			comp.Notify_AssignmentsChanged(ritualAssignments, ritual?.outcomeEffect?.DataForComp(comp));
		}
	}

	public override string ExtraInfoForRole(RitualRole role, Pawn pawnToBeAssigned, IEnumerable<Pawn> currentlyAssigned)
	{
		string text = ((pawnToBeAssigned == null) ? role.ExtraInfoForDialog(currentlyAssigned) : null);
		Pawn pawn = pawnToBeAssigned ?? currentlyAssigned.FirstOrDefault();
		PreceptDef preceptDef = pawn?.Ideo?.GetRole(pawn)?.def;
		if (pawn != null && preceptDef != role.precept && role.substitutable && role.precept != null)
		{
			if (text != null)
			{
				text += "\n\n";
			}
			text += ExtraRoleInfo(role);
			if (!Find.IdeoManager.classicMode)
			{
				text = text + ": " + BonusExplanationInfo(role).CapitalizeFirst().Resolve();
			}
		}
		return text;
	}

	public override bool ShouldGrayOut(Pawn pawn, out TaggedString reason)
	{
		if (base.ShouldGrayOut(pawn, out reason))
		{
			return true;
		}
		if (ritual?.obligationTargetFilter != null)
		{
			return ritual.obligationTargetFilter.ShouldGrayOut(pawn, assignments, out reason);
		}
		return false;
	}

	protected override string ExtraTipContents(Pawn pawn)
	{
		sb.Clear();
		tmpRelevantStats.Clear();
		foreach (RitualRole item in ritualAssignments.AllRolesForReading)
		{
			if (item is RitualRoleColonist { usedStat: not null } ritualRoleColonist)
			{
				tmpRelevantStats.Add(ritualRoleColonist.usedStat);
			}
		}
		if (tmpRelevantStats.Count == 0)
		{
			return null;
		}
		foreach (StatDef tmpRelevantStat in tmpRelevantStats)
		{
			if (tmpRelevantStat.Worker.IsDisabledFor(pawn))
			{
				sb.AppendLine("  - " + (tmpRelevantStat.LabelCap + ": " + "Disabled".Translate()).Colorize(ColorLibrary.RedReadable));
			}
			else
			{
				sb.AppendLine("  - " + tmpRelevantStat.LabelCap + ": " + tmpRelevantStat.Worker.ValueToStringFor(pawn));
			}
		}
		return sb.ToString();
	}

	public string ExtraRoleInfo(RitualRole role)
	{
		Precept precept = ritual.ideo.PreceptsListForReading.FirstOrDefault((Precept p) => p.def == role.precept);
		if (precept == null)
		{
			return null;
		}
		return "RitualRoleRequiresSocialRole".Translate(precept.Label);
	}

	public TaggedString BonusExplanationInfo(RitualRole role)
	{
		TaggedString result = "";
		bool flag = false;
		if (ritual.outcomeEffect != null && !ritual.outcomeEffect.def.comps.NullOrEmpty())
		{
			foreach (RitualOutcomeComp comp in ritual.outcomeEffect.def.comps)
			{
				if (comp is RitualOutcomeComp_RolePresentNotSubstituted)
				{
					if (flag)
					{
						result += ", ";
					}
					result += comp.GetBonusDescShort();
					flag = true;
				}
			}
		}
		if (flag)
		{
			return result;
		}
		return "None".Translate();
	}

	public override string SpectatorFilterReason(Pawn pawn)
	{
		if (ritual != null && ritual.behavior.def.spectatorFilter != null && !ritual.behavior.def.spectatorFilter.Allowed(pawn))
		{
			return ritual.behavior.def.spectatorFilter.description;
		}
		return null;
	}
}
