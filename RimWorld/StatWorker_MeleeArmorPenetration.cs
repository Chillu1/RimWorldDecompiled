using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatWorker_MeleeArmorPenetration : StatWorker
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
			Log.Error("Getting MeleeArmorPenetration stat for " + req.Def?.ToString() + " without concrete pawn. This always returns 0.");
		}
		return GetArmorPenetration(req, applyPostProcess);
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		if (base.ShouldShowFor(req))
		{
			return req.Thing is Pawn;
		}
		return false;
	}

	private float GetArmorPenetration(StatRequest req, bool applyPostProcess = true)
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
				num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * updatedAvailableVerbsList[j].verb.verbProps.AdjustedArmorPenetration(updatedAvailableVerbsList[j].verb, pawn);
			}
		}
		return num2;
	}
}
