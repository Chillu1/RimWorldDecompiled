using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

[StaticConstructorOnStartup]
public class PawnPsychicRitualRoleSelectionWidget : PawnRoleSelectionWidgetBase<PsychicRitualRoleDef>
{
	protected PsychicRitualRoleAssignments psychicRitualAssignments;

	protected PsychicRitualDef ritualDef;

	private StringBuilder sb = new StringBuilder();

	public PawnPsychicRitualRoleSelectionWidget(PsychicRitualDef ritualDef, PsychicRitualCandidatePool candidatePool, PsychicRitualRoleAssignments assignments)
		: base((ILordJobCandidatePool)candidatePool, (ILordJobAssignmentsManager<PsychicRitualRoleDef>)assignments)
	{
		this.ritualDef = ritualDef;
		psychicRitualAssignments = assignments;
	}

	public override bool ShouldDrawHighlight(PsychicRitualRoleDef highlightedRole, Pawn pawn)
	{
		PsychicRitualRoleDef.Reason reason;
		if (highlightedRole != null && !DragAndDropWidget.Dragging && !assignments.AssignedPawns(highlightedRole).Contains(pawn))
		{
			return highlightedRole.PawnCanDo(PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, pawn, psychicRitualAssignments.Target, out reason);
		}
		return false;
	}

	protected override string ExtraTipContents(Pawn pawn)
	{
		sb.Clear();
		sb.AppendLine("  - " + StatDefOf.PsychicSensitivity.LabelCap + ": " + StatDefOf.PsychicSensitivity.Worker.ValueToStringFor(pawn));
		if (ModsConfig.IdeologyActive && !Find.IdeoManager.classicMode && pawn.Ideo != null)
		{
			sb.AppendLineTagged("  - " + "Ideo".Translate().CapitalizeFirst() + ": " + pawn.Ideo.name.Colorize(pawn.Ideo.Color));
		}
		foreach (string pawnTooltipExtra in ritualDef.GetPawnTooltipExtras(pawn))
		{
			sb.AppendLine("  - " + pawnTooltipExtra);
		}
		return sb.ToString().TrimEndNewlines();
	}
}
