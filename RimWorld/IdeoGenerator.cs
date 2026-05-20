using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class IdeoGenerator
{
	public static Ideo GenerateIdeo(IdeoGenerationParms parms)
	{
		Ideo ideo = MakeIdeo(DefDatabase<IdeoFoundationDef>.AllDefs.RandomElement());
		ideo.foundation.Init(parms);
		return ideo;
	}

	public static Ideo MakeIdeo(IdeoFoundationDef foundationDef)
	{
		Ideo ideo = new Ideo();
		ideo.id = Find.UniqueIDsManager.GetNextIdeoID();
		ideo.foundation = MakeFoundation(foundationDef);
		ideo.foundation.ideo = ideo;
		return ideo;
	}

	public static Ideo MakeFixedIdeo(IdeoGenerationParms parms)
	{
		Ideo ideo = MakeIdeo(DefDatabase<IdeoFoundationDef>.AllDefs.RandomElement());
		ideo.foundation.Init(parms);
		if (parms.deities != null && ideo.foundation is IdeoFoundation_Deity ideoFoundation_Deity)
		{
			List<IdeoFoundation_Deity.Deity> list = new List<IdeoFoundation_Deity.Deity>();
			foreach (DeityPreset deity in parms.deities)
			{
				IdeoFoundation_Deity.Deity item = new IdeoFoundation_Deity.Deity
				{
					name = deity.nameType.name,
					type = deity.nameType.type,
					gender = deity.gender,
					iconPath = deity.iconPath
				};
				list.Add(item);
			}
			ideoFoundation_Deity.SetDeities(list);
		}
		if (parms.name != "")
		{
			ideo.name = parms.name;
		}
		if (parms.styles != null)
		{
			ideo.thingStyleCategories.Clear();
			for (int i = 0; i < parms.styles.Count; i++)
			{
				StyleCategoryDef category = parms.styles[i];
				ideo.thingStyleCategories.Add(new ThingStyleCategoryWithPriority(category, 3 - i));
			}
			ideo.SortStyleCategories();
		}
		if (parms.description != "")
		{
			ideo.description = parms.description;
		}
		ideo.hidden = parms.hidden;
		ideo.solid = true;
		return ideo;
	}

	public static IdeoFoundation MakeFoundation(IdeoFoundationDef foundationDef)
	{
		IdeoFoundation obj = (IdeoFoundation)Activator.CreateInstance(foundationDef.foundationClass);
		obj.def = foundationDef;
		return obj;
	}

	public static Ideo InitLoadedIdeo(Ideo ideo)
	{
		ideo.id = Find.UniqueIDsManager.GetNextIdeoID();
		ideo.style.ideo = ideo;
		return ideo;
	}

	public static Ideo GenerateClassicIdeo(CultureDef culture, IdeoGenerationParms genParms, bool noExpansionIdeo)
	{
		Ideo ideo = new Ideo();
		ideo.id = Find.UniqueIDsManager.GetNextIdeoID();
		ideo.culture = culture;
		ideo.createdFromNoExpansionGame = noExpansionIdeo;
		ideo.name = ComputeIdeoName();
		ideo.memberName = FactionDefOf.PlayerColony.basicMemberKind.label;
		ideo.classicMode = !noExpansionIdeo;
		if (ModsConfig.IdeologyActive)
		{
			IdeoFoundationDef ideoFoundationDef = DefDatabase<IdeoFoundationDef>.AllDefs.RandomElement();
			ideo.foundation = (IdeoFoundation)Activator.CreateInstance(ideoFoundationDef.foundationClass);
			ideo.foundation.def = ideoFoundationDef;
			ideo.foundation.ideo = ideo;
			ideo.SetIcon(IdeoFoundation.GetRandomIconDef(ideo), IdeoFoundation.GetRandomColorDef(ideo));
		}
		List<PreceptDef> allDefsListForReading = DefDatabase<PreceptDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			PreceptDef preceptDef = allDefsListForReading[i];
			if (preceptDef.classic && (genParms.disallowedPrecepts == null || !genParms.disallowedPrecepts.Contains(preceptDef)))
			{
				int num = 1;
				if (preceptDef.preceptInstanceCountCurve != null)
				{
					num = Mathf.CeilToInt(preceptDef.preceptInstanceCountCurve.Evaluate(Rand.Value));
				}
				for (int j = 0; j < num; j++)
				{
					ideo.AddPrecept(PreceptMaker.MakePrecept(preceptDef), init: true, null, preceptDef.ritualPatternBase);
				}
			}
		}
		return ideo;
		TaggedString ComputeIdeoName()
		{
			int num2 = 1;
			string text;
			while (true)
			{
				text = culture.LabelCap;
				if (num2 > 1)
				{
					text = text + " " + num2;
				}
				if (!NameUsed(text))
				{
					break;
				}
				num2++;
			}
			return text;
		}
		static bool NameUsed(string name)
		{
			foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
			{
				if (item.name == name)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static Ideo GenerateNoExpansionIdeo(CultureDef culture, IdeoGenerationParms genParms)
	{
		return GenerateClassicIdeo(culture, genParms, noExpansionIdeo: true);
	}

	public static Ideo GenerateTutorialIdeo()
	{
		IdeoGenerationParms parms = new IdeoGenerationParms(Find.Scenario.playerFaction.factionDef);
		parms.disallowedPrecepts = DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.impact == PreceptImpact.High).ToList();
		parms.disallowedMemes = DefDatabase<MemeDef>.AllDefs.Where((MemeDef x) => !x.allowDuringTutorial).ToList();
		return GenerateIdeo(parms);
	}
}
