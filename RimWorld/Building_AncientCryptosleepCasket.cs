using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Building_AncientCryptosleepCasket : Building_CryptosleepCasket
{
	public int groupID = -1;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref groupID, "groupID", 0);
	}

	public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		base.PreApplyDamage(ref dinfo, out absorbed);
		if (absorbed)
		{
			return;
		}
		if (!contentsKnown && innerContainer.Count > 0 && dinfo.Def.harmsHealth && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
		{
			bool flag = false;
			foreach (Thing item in (IEnumerable<Thing>)innerContainer)
			{
				if (item is Pawn)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				EjectContents();
			}
		}
		absorbed = false;
	}

	public override void EjectContents()
	{
		bool num = contentsKnown;
		List<Thing> list = null;
		if (!num)
		{
			list = new List<Thing>();
			list.AddRange(innerContainer);
			list.AddRange(UnopenedCasketsInGroup().SelectMany((Building_AncientCryptosleepCasket c) => c.innerContainer));
			list.RemoveDuplicates();
		}
		base.EjectContents();
		if ((bool)ClaimableBy(Faction.OfPlayer))
		{
			SetFaction(null);
		}
		if (num)
		{
			return;
		}
		ThingDef filth_Slime = ThingDefOf.Filth_Slime;
		FilthMaker.TryMakeFilth(base.Position, base.Map, filth_Slime, Rand.Range(8, 12));
		foreach (Building_AncientCryptosleepCasket item in UnopenedCasketsInGroup())
		{
			item.contentsKnown = true;
			item.EjectContents();
		}
		IEnumerable<Pawn> enumerable = from p in list.OfType<Pawn>().ToList()
			where p.RaceProps.Humanlike && p.GetLord() == null && p.Faction == Faction.OfAncientsHostile
			select p;
		if (enumerable.Any())
		{
			LordMaker.MakeNewLord(Faction.OfAncientsHostile, new LordJob_AssaultColony(Faction.OfAncientsHostile, canKidnap: false, canTimeoutOrFlee: true, sappers: false, useAvoidGridSmart: false, canSteal: false), base.Map, enumerable);
		}
	}

	private IEnumerable<Building_AncientCryptosleepCasket> UnopenedCasketsInGroup()
	{
		yield return this;
		if (groupID == -1)
		{
			yield break;
		}
		foreach (Thing item in base.Map.listerThings.ThingsOfDef(ThingDefOf.AncientCryptosleepCasket))
		{
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = item as Building_AncientCryptosleepCasket;
			if (building_AncientCryptosleepCasket.groupID == groupID && !building_AncientCryptosleepCasket.contentsKnown)
			{
				yield return building_AncientCryptosleepCasket;
			}
		}
	}
}
