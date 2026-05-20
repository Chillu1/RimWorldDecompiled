using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompAnalyzableUnlockResearch : CompAnalyzable
{
	private List<ResearchProjectDef> researchUnlocked;

	public new CompProperties_CompAnalyzableUnlockResearch Props => (CompProperties_CompAnalyzableUnlockResearch)props;

	public List<ResearchProjectDef> ResearchUnlocked
	{
		get
		{
			if (researchUnlocked == null)
			{
				researchUnlocked = new List<ResearchProjectDef>();
				foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
				{
					if (!allDef.requiredAnalyzed.NullOrEmpty() && allDef.requiredAnalyzed.Contains(parent.def))
					{
						researchUnlocked.Add(allDef);
					}
				}
			}
			return researchUnlocked;
		}
	}

	public override NamedArgument? ExtraNamedArg => ResearchUnlocked.Select((ResearchProjectDef r) => r.label).ToCommaList(useAnd: true).Named("RESEARCH");

	public override int AnalysisID => Props.analysisID;

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		if (activateBy != null && Props.requiresMechanitor && !MechanitorUtility.IsMechanitor(activateBy))
		{
			return "RequiresMechanitor".Translate();
		}
		return true;
	}
}
