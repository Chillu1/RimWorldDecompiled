using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetPawnKind : QuestNode
{
	public class Option
	{
		public PawnKindDef kindDef;

		public float weight;

		public bool anyAnimal;

		public bool mustBeAbleToHandleAnimal;

		public FleshTypeDef onlyAllowedFleshType;
	}

	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<List<Option>> options;

	public bool failIfNotFound;

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		if (failIfNotFound && GetKindDef(slate) == null)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		slate.Set(storeAs.GetValue(slate), GetKindDef(slate));
	}

	private PawnKindDef GetKindDef(Slate slate)
	{
		Option option = options.GetValue(slate).RandomElementByWeight((Option x) => x.weight);
		if (option.kindDef != null)
		{
			return option.kindDef;
		}
		int highestAnimalSkill;
		if (option.anyAnimal)
		{
			highestAnimalSkill = ((!option.mustBeAbleToHandleAnimal) ? int.MaxValue : 0);
			if (option.mustBeAbleToHandleAnimal)
			{
				Map map = slate.Get<Map>("map");
				if (map != null)
				{
					foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
					{
						highestAnimalSkill = Mathf.Max(highestAnimalSkill, item.skills.GetSkill(SkillDefOf.Animals).Level);
					}
				}
				else
				{
					foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_FreeColonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
					{
						highestAnimalSkill = Mathf.Max(highestAnimalSkill, allMapsCaravansAndTravellingTransporters_Alive_FreeColonist.skills.GetSkill(SkillDefOf.Animals).Level);
					}
				}
			}
			return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Animal && !x.RaceProps.neverIncludeInQuests && !x.RaceProps.Dryad && (option.onlyAllowedFleshType == null || x.RaceProps.FleshType == option.onlyAllowedFleshType) && CanHandle(x)).RandomElementWithFallback();
		}
		return null;
		bool CanHandle(PawnKindDef animal)
		{
			if (!option.mustBeAbleToHandleAnimal)
			{
				return true;
			}
			return animal.race.GetStatValueAbstract(StatDefOf.MinimumHandlingSkill) <= (float)highestAnimalSkill;
		}
	}
}
