using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class StatWorker_MeleeAverageDPS : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!(req.Def is ThingDef thingDef))
		{
			return false;
		}
		if (!thingDef.IsWeapon && !thingDef.isTechHediff)
		{
			return false;
		}
		GetVerbsAndTools(thingDef, out var verbs, out var tools);
		if (!tools.NullOrEmpty())
		{
			return true;
		}
		if (verbs != null)
		{
			for (int i = 0; i < verbs.Count; i++)
			{
				if (verbs[i].IsMeleeAttack)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		ThingDef thingDef = req.Def as ThingDef;
		if (thingDef == null)
		{
			return 0f;
		}
		GetVerbsAndTools(thingDef, out var verbs, out var tools);
		if (req.Thing != null)
		{
			Pawn attacker = GetCurrentWeaponUser(req.Thing);
			float num = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
				where x.verbProps.IsMeleeAttack
				select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeDamageAmount(x.tool, attacker, req.Thing, null));
			float num2 = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
				where x.verbProps.IsMeleeAttack
				select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedCooldown(x.tool, attacker, req.Thing));
			if (num2 == 0f)
			{
				return 0f;
			}
			return num / num2;
		}
		float num3 = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, null, thingDef, req.StuffDef, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeDamageAmount(x.tool, null, thingDef, req.StuffDef, null));
		float num4 = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, null, thingDef, req.StuffDef, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedCooldown(x.tool, null, thingDef, req.StuffDef));
		if (num4 == 0f)
		{
			return 0f;
		}
		return num3 / num4;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		if (!(req.Def is ThingDef thingDef))
		{
			return null;
		}
		GetVerbsAndTools(thingDef, out var verbs, out var tools);
		Pawn currentWeaponUser = GetCurrentWeaponUser(req.Thing);
		IEnumerable<VerbUtility.VerbPropertiesWithSource> enumerable = from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (VerbUtility.VerbPropertiesWithSource item in enumerable)
		{
			float num = 0f;
			float num2 = 0f;
			if (req.HasThing)
			{
				num = item.verbProps.AdjustedMeleeDamageAmount(item.tool, currentWeaponUser, req.Thing, null);
				num2 = item.verbProps.AdjustedCooldown(item.tool, currentWeaponUser, req.Thing);
			}
			else
			{
				num = item.verbProps.AdjustedMeleeDamageAmount(item.tool, null, thingDef, req.StuffDef, null);
				num2 = item.verbProps.AdjustedCooldown(item.tool, null, thingDef, req.StuffDef);
			}
			if (item.tool != null)
			{
				stringBuilder.AppendLine($"  {item.tool.LabelCap} ({item.ToolCapacity.label})");
			}
			else
			{
				stringBuilder.AppendLine(string.Format("  {0}:", "StatsReport_NonToolAttack".Translate()));
			}
			stringBuilder.AppendLine(string.Format("    {0} {1}", num.ToString("F1"), "DamageLower".Translate()));
			stringBuilder.AppendLine(string.Format("    {0} {1}", num2.ToString("F2"), "SecondsPerAttackLower".Translate()));
		}
		return stringBuilder.ToString();
	}

	public static Pawn GetCurrentWeaponUser(Thing weapon)
	{
		if (weapon == null)
		{
			return null;
		}
		if (weapon.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker)
		{
			return pawn_EquipmentTracker.pawn;
		}
		if (weapon.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker)
		{
			return pawn_ApparelTracker.pawn;
		}
		return null;
	}

	private void GetVerbsAndTools(ThingDef def, out List<VerbProperties> verbs, out List<Tool> tools)
	{
		verbs = def.Verbs;
		tools = def.tools;
		if (!def.isTechHediff)
		{
			return;
		}
		HediffDef hediffDef = FindTechHediffHediff(def);
		if (hediffDef != null)
		{
			HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = hediffDef.CompProps<HediffCompProperties_VerbGiver>();
			if (hediffCompProperties_VerbGiver != null)
			{
				verbs = hediffCompProperties_VerbGiver.verbs;
				tools = hediffCompProperties_VerbGiver.tools;
			}
		}
	}

	private HediffDef FindTechHediffHediff(ThingDef techHediff)
	{
		List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (allDefsListForReading[i].addsHediff != null && allDefsListForReading[i].IsIngredient(techHediff))
			{
				return allDefsListForReading[i].addsHediff;
			}
		}
		return null;
	}
}
