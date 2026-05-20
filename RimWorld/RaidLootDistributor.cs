using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

internal class RaidLootDistributor
{
	private readonly IncidentParms parms;

	private readonly List<Pawn> allPawns;

	private readonly List<Thing> loot;

	private readonly List<Pawn> unusedPawns;

	private Pawn recipient;

	private float recipientMassGiven;

	public RaidLootDistributor(IncidentParms parms, List<Pawn> allPawns, List<Thing> loot)
	{
		this.parms = parms;
		this.allPawns = allPawns;
		this.loot = loot;
		unusedPawns = new List<Pawn>(allPawns.Where((Pawn x) => !x.RaceProps.Animal));
	}

	public void DistributeLoot()
	{
		recipient = unusedPawns.MaxBy((Pawn p) => p.kindDef.combatPower);
		recipientMassGiven = 0f;
		foreach (IGrouping<ThingDef, Thing> item in from t in loot
			group t by t.def)
		{
			foreach (Thing item2 in item)
			{
				DistributeItem(item2);
			}
			NextRecipient();
		}
	}

	private void DistributeItem(Thing item)
	{
		int num = item.stackCount;
		int num2 = 0;
		while (num > 0 && num2++ < 5)
		{
			num -= TryGiveToRecipient(item, num);
			if (num > 0)
			{
				NextRecipient();
			}
		}
		if (num > 0)
		{
			NextRecipient();
			TryGiveToRecipient(item, num, force: true);
		}
	}

	private int TryGiveToRecipient(Thing item, int count, bool force = false)
	{
		float num = 10f * Mathf.Max(1f, recipient.BodySize) - recipientMassGiven;
		float statValue = item.GetStatValue(StatDefOf.Mass);
		int num2 = (force ? count : Mathf.RoundToInt(Mathf.Clamp(num / statValue, 0f, count)));
		if (num2 > 0)
		{
			int num3 = recipient.inventory.innerContainer.TryAdd(item, num2);
			recipientMassGiven += (float)num3 * statValue;
			return num3;
		}
		return 0;
	}

	private void NextRecipient()
	{
		recipientMassGiven = 0f;
		if (unusedPawns.Any())
		{
			recipient = unusedPawns.Pop();
		}
		else
		{
			recipient = allPawns.RandomElement();
		}
	}
}
