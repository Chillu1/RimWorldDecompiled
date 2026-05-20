using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class StatWorker_MeleeAverageArmorPenetration : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (req.Def is ThingDef { IsWeapon: not false } thingDef)
		{
			return !thingDef.tools.NullOrEmpty();
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
		if (req.Thing != null)
		{
			Pawn attacker = StatWorker_MeleeAverageDPS.GetCurrentWeaponUser(req.Thing);
			return (from x in VerbUtility.GetAllVerbProperties(thingDef.Verbs, thingDef.tools)
				where x.verbProps.IsMeleeAttack
				select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedArmorPenetration(x.tool, attacker, req.Thing, null));
		}
		return (from x in VerbUtility.GetAllVerbProperties(thingDef.Verbs, thingDef.tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, null, thingDef, req.StuffDef, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedArmorPenetration(x.tool, null, thingDef, req.StuffDef, null));
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		if (!(req.Def is ThingDef thingDef))
		{
			return null;
		}
		Pawn currentWeaponUser = StatWorker_MeleeAverageDPS.GetCurrentWeaponUser(req.Thing);
		IEnumerable<VerbUtility.VerbPropertiesWithSource> enumerable = from x in VerbUtility.GetAllVerbProperties(thingDef.Verbs, thingDef.tools)
			where x.verbProps.IsMeleeAttack
			select x;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (VerbUtility.VerbPropertiesWithSource item in enumerable)
		{
			float f = item.verbProps.AdjustedArmorPenetration(item.tool, currentWeaponUser, req.Thing, null);
			if (item.tool != null)
			{
				stringBuilder.AppendLine($"  {item.tool.LabelCap} ({item.ToolCapacity.label})");
			}
			else
			{
				stringBuilder.AppendLine(string.Format("  {0}:", "StatsReport_NonToolAttack".Translate()));
			}
			stringBuilder.AppendLine("    " + f.ToStringPercent());
		}
		return stringBuilder.ToString();
	}
}
