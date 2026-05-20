using Verse;

namespace RimWorld;

public class Building_ResearchBench : Building
{
	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		ResearchProjectDef project = Find.ResearchManager.GetProject();
		if (project != null)
		{
			return text + string.Format("{0}: {1}\n{2}: {3:F0} / {4:F0} ({5})", "CurrentProject".Translate(), project.LabelCap, "ResearchProgress".Translate(), project.ProgressApparent, project.CostApparent, project.ProgressPercent.ToStringPercent("0.#"));
		}
		return text + string.Format("{0}: {1}", "CurrentProject".Translate(), "None".Translate());
	}
}
