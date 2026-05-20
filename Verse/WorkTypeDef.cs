using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class WorkTypeDef : Def
{
	public WorkTags workTags;

	[MustTranslate]
	public string labelShort;

	[MustTranslate]
	public string pawnLabel;

	[MustTranslate]
	public string gerundLabel;

	[MustTranslate]
	public string verb;

	public bool visible = true;

	public bool visibleOnlyWithChildrenInColony;

	public int naturalPriority;

	public bool alwaysStartActive;

	public bool requireCapableColonist;

	public List<SkillDef> relevantSkills = new List<SkillDef>();

	public bool disabledForSlaves;

	[Unsaved(false)]
	public List<WorkGiverDef> workGiversByPriority = new List<WorkGiverDef>();

	[Unsaved(false)]
	private bool cachedVisibleCurrently;

	[Unsaved(false)]
	private int cachedFrameVisibleCurrently = -1;

	public bool VisibleCurrently
	{
		get
		{
			if (cachedFrameVisibleCurrently == -1 || cachedFrameVisibleCurrently < Time.frameCount - 30)
			{
				cachedVisibleCurrently = VisibleNow();
				cachedFrameVisibleCurrently = Time.frameCount;
			}
			return cachedVisibleCurrently;
		}
	}

	public bool VisibleNow(Pawn ignorePawn = null, Pawn alsoCheckPawn = null)
	{
		if (!visible)
		{
			return false;
		}
		if (visibleOnlyWithChildrenInColony)
		{
			bool flag = false;
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
			{
				if (item.RaceProps.Humanlike && item != ignorePawn && item.DevelopmentalStage.Juvenile())
				{
					flag = true;
					break;
				}
			}
			if (alsoCheckPawn != null && alsoCheckPawn.DevelopmentalStage.Juvenile())
			{
				flag = true;
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (naturalPriority < 0 || naturalPriority > 10000)
		{
			yield return "naturalPriority is " + naturalPriority + ", but it must be between 0 and 10000";
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		foreach (WorkGiverDef item in from d in DefDatabase<WorkGiverDef>.AllDefs
			where d.workType == this
			orderby d.priorityInType descending
			select d)
		{
			workGiversByPriority.Add(item);
		}
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(defName.GetHashCode(), gerundLabel);
	}
}
