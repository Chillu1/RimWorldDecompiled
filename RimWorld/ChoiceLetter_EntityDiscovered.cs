using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_EntityDiscovered : ChoiceLetter
{
	public EntityCodexEntryDef codexEntry;

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (codexEntry != null)
			{
				foreach (ResearchProjectDef project in codexEntry.discoveredResearchProjects)
				{
					yield return new DiaOption("ViewHyperlink".Translate(project.label))
					{
						action = delegate
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
							if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research mainTabWindow_Research)
							{
								mainTabWindow_Research.Select(project);
							}
						},
						resolveTree = true
					};
				}
				yield return Option_OpenEntityCodex;
			}
			yield return base.Option_Close;
		}
	}

	protected DiaOption Option_OpenEntityCodex => new DiaOption("ViewEntityCodex".Translate())
	{
		action = delegate
		{
			if (codexEntry != null)
			{
				Find.WindowStack.Add(new Dialog_EntityCodex(codexEntry));
			}
		},
		resolveTree = true
	};

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref codexEntry, "codexEntry");
	}
}
