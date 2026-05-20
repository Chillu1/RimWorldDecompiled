using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoyalTitlePermitDef : Def
{
	public Type workerClass = typeof(RoyalTitlePermitWorker);

	public RoyalAid royalAid;

	public float cooldownDays;

	public RoyalTitleDef minTitle;

	public int permitPointCost;

	public FactionDef faction;

	public bool usableOnWorldMap;

	public RoyalTitlePermitDef prerequisite;

	public Vector2 uiPosition;

	public List<PlanetLayerDef> layerBlacklist = new List<PlanetLayerDef>();

	private RoyalTitlePermitWorker worker;

	public int CooldownTicks => (int)(cooldownDays * 60000f);

	public RoyalTitlePermitWorker Worker
	{
		get
		{
			if (worker == null)
			{
				worker = (RoyalTitlePermitWorker)Activator.CreateInstance(workerClass);
				worker.def = this;
			}
			return worker;
		}
	}

	public bool AvailableForPawn(Pawn pawn, Faction faction)
	{
		if (pawn.royalty == null)
		{
			return false;
		}
		if (pawn.royalty.HasPermit(this, faction))
		{
			return false;
		}
		if (prerequisite != null && !pawn.royalty.HasPermit(prerequisite, faction))
		{
			return false;
		}
		if (pawn.royalty.GetPermitPoints(faction) < permitPointCost)
		{
			return false;
		}
		RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(faction);
		if (currentTitle == null && minTitle == null)
		{
			return true;
		}
		if (currentTitle == null && minTitle != null)
		{
			return false;
		}
		return currentTitle.seniority >= minTitle.seniority;
	}

	public bool IsPrerequisiteOfHeldPermit(Pawn pawn, Faction faction)
	{
		List<FactionPermit> allFactionPermits = pawn.royalty.AllFactionPermits;
		for (int i = 0; i < allFactionPermits.Count; i++)
		{
			if (allFactionPermits[i].Permit.prerequisite == this && allFactionPermits[i].Faction == faction)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (!typeof(RoyalTitlePermitWorker).IsAssignableFrom(workerClass))
		{
			yield return $"RoyalTitlePermitDef {defName} has worker class {workerClass}, which is not deriving from {typeof(RoyalTitlePermitWorker).FullName}";
		}
		if (royalAid == null)
		{
			yield break;
		}
		if (royalAid.pawnKindDef != null && royalAid.pawnCount <= 0)
		{
			yield return "pawnCount should be greater than 0, if you specify pawnKindDef";
		}
		if (!royalAid.itemsToDrop.NullOrEmpty())
		{
			for (int i = 0; i < royalAid.itemsToDrop.Count; i++)
			{
				if (royalAid.itemsToDrop[i].count <= 0)
				{
					yield return "item count should be greater than 0.";
				}
				if (royalAid.itemsToDrop[i].thingDef == null)
				{
					yield return "thingDef not defined.";
				}
			}
		}
		if (royalAid.favorCost <= 0)
		{
			yield return "favor cost should be greater than 0.";
		}
	}
}
