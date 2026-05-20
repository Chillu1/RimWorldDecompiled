using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class StatWorker_MeleeDPS : StatWorker
{
	public override bool IsDisabledFor(Thing thing)
	{
		if (!base.IsDisabledFor(thing))
		{
			return StatDefOf.MeleeHitChance.Worker.IsDisabledFor(thing);
		}
		return true;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		if (req.Thing == null)
		{
			Log.Error("Getting MeleeDPS stat for " + req.Def?.ToString() + " without concrete pawn. This always returns 0.");
		}
		return GetMeleeDamage(req, applyPostProcess) * GetMeleeHitChance(req, applyPostProcess) / GetMeleeCooldown(req, applyPostProcess);
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("StatsReport_MeleeDPSExplanation".Translate());
		stringBuilder.AppendLine("StatsReport_MeleeDamage".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
		stringBuilder.AppendLine("  " + GetMeleeDamage(req).ToString("0.##"));
		stringBuilder.AppendLine("StatsReport_Cooldown".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
		stringBuilder.AppendLine("  " + "StatsReport_CooldownFormat".Translate(GetMeleeCooldown(req).ToString("0.##")));
		stringBuilder.AppendLine("StatsReport_MeleeHitChance".Translate());
		stringBuilder.AppendLine(StatDefOf.MeleeHitChance.Worker.GetExplanationUnfinalized(req, StatDefOf.MeleeHitChance.toStringNumberSense).TrimEndNewlines().Indented());
		stringBuilder.Append(StatDefOf.MeleeHitChance.Worker.GetExplanationFinalizePart(req, StatDefOf.MeleeHitChance.toStringNumberSense, GetMeleeHitChance(req)).Indented());
		return stringBuilder.ToString();
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		return string.Format("{0} ( {1} x {2} / {3} )", value.ToStringByStyle(stat.toStringStyle, numberSense), GetMeleeDamage(optionalReq).ToString("0.##"), StatDefOf.MeleeHitChance.ValueToString(GetMeleeHitChance(optionalReq)), GetMeleeCooldown(optionalReq).ToString("0.##"));
	}

	private float GetMeleeDamage(StatRequest req, bool applyPostProcess = true)
	{
		if (!(req.Thing is Pawn pawn))
		{
			return 0f;
		}
		List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false);
		if (updatedAvailableVerbsList.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
		{
			if (updatedAvailableVerbsList[i].IsMeleeAttack)
			{
				num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
			}
		}
		if (num == 0f)
		{
			return 0f;
		}
		float num2 = 0f;
		for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
		{
			if (updatedAvailableVerbsList[j].IsMeleeAttack)
			{
				num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * updatedAvailableVerbsList[j].verb.verbProps.AdjustedMeleeDamageAmount(updatedAvailableVerbsList[j].verb, pawn);
			}
		}
		return num2;
	}

	private float GetMeleeHitChance(StatRequest req, bool applyPostProcess = true)
	{
		if (req.HasThing)
		{
			return req.Thing.GetStatValue(StatDefOf.MeleeHitChance, applyPostProcess);
		}
		return req.BuildableDef.GetStatValueAbstract(StatDefOf.MeleeHitChance);
	}

	private float GetMeleeCooldown(StatRequest req, bool applyPostProcess = true)
	{
		if (!(req.Thing is Pawn pawn))
		{
			return 1f;
		}
		List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false);
		if (updatedAvailableVerbsList.Count == 0)
		{
			return 1f;
		}
		float num = 0f;
		for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
		{
			if (updatedAvailableVerbsList[i].IsMeleeAttack)
			{
				num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
			}
		}
		if (num == 0f)
		{
			return 1f;
		}
		float num2 = 0f;
		for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
		{
			if (updatedAvailableVerbsList[j].IsMeleeAttack)
			{
				num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * (float)updatedAvailableVerbsList[j].verb.verbProps.AdjustedCooldownTicks(updatedAvailableVerbsList[j].verb, pawn);
			}
		}
		return num2 / 60f;
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		if (base.ShouldShowFor(req))
		{
			return req.Thing is Pawn;
		}
		return false;
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
	{
		if (statRequest.Thing is Pawn { equipment: not null } pawn && pawn.equipment.Primary != null)
		{
			yield return new Dialog_InfoCard.Hyperlink(pawn.equipment.Primary);
		}
	}
}
