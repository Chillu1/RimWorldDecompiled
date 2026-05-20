namespace RimWorld;

public class ThoughtWorker_Precept_ChildLabor_ChildAssignedWork : ThoughtWorker_Precept_ChildLabor
{
	protected override TimeAssignmentDef AssignmentDef => TimeAssignmentDefOf.Work;
}
