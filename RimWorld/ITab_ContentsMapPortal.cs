using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ITab_ContentsMapPortal : ITab_ContentsBase
{
	private List<Thing> tmpContainer = new List<Thing>();

	private Vector2 scroll;

	private float lastHeight;

	public override IList<Thing> container => tmpContainer;

	public override bool UseDiscardMessage => false;

	public MapPortal Portal => base.SelThing as MapPortal;

	public override bool IsVisible
	{
		get
		{
			if (Portal != null)
			{
				return Portal.LoadInProgress;
			}
			return false;
		}
	}

	public override IntVec3 DropOffset => IntVec3.Zero;

	public ITab_ContentsMapPortal()
	{
		labelKey = "TabMapPortalContents";
		containedItemsKey = "";
	}

	protected override void DoItemsLists(Rect inRect, ref float curY)
	{
		MapPortal portal = Portal;
		Text.Font = GameFont.Small;
		bool flag = false;
		float curY2 = 0f;
		Widgets.BeginGroup(inRect);
		Rect viewRect = inRect;
		viewRect.y = 0f;
		viewRect.height = lastHeight;
		if (lastHeight > inRect.height)
		{
			viewRect.width -= 16f;
		}
		Widgets.BeginScrollView(inRect, ref scroll, viewRect);
		Widgets.ListSeparator(ref curY2, inRect.width, Portal.EnteringString);
		if (portal.leftToLoad != null)
		{
			for (int i = 0; i < portal.leftToLoad.Count; i++)
			{
				TransferableOneWay t = portal.leftToLoad[i];
				if (t.CountToTransfer > 0 && t.HasAnyThing)
				{
					flag = true;
					DoThingRow(t.ThingDef, t.CountToTransfer, t.things, viewRect.width, ref curY2, delegate(int x)
					{
						OnDropToLoadThing(t, x);
					});
				}
			}
		}
		lastHeight = curY2;
		Widgets.EndScrollView();
		if (!flag)
		{
			Widgets.NoneLabel(ref curY2, inRect.width);
		}
		Widgets.EndGroup();
	}

	protected override void OnDropThing(Thing t, int count)
	{
		base.OnDropThing(t, count);
		if (t is Pawn pawn)
		{
			RemovePawnFromLoadLord(pawn);
		}
	}

	private void RemovePawnFromLoadLord(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null && lord.LordJob is LordJob_LoadAndEnterTransporters)
		{
			lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
		}
	}

	private void OnDropToLoadThing(TransferableOneWay t, int count)
	{
		t.ForceTo(t.CountToTransfer - count);
		EndJobForEveryoneHauling(t);
		foreach (Thing thing in t.things)
		{
			if (thing is Pawn pawn)
			{
				RemovePawnFromLoadLord(pawn);
			}
		}
	}

	private void EndJobForEveryoneHauling(TransferableOneWay t)
	{
		IReadOnlyList<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
			{
				JobDriver_HaulToPortal jobDriver_HaulToPortal = (JobDriver_HaulToPortal)allPawnsSpawned[i].jobs.curDriver;
				if (jobDriver_HaulToPortal.MapPortal == Portal && jobDriver_HaulToPortal.ThingToCarry != null && jobDriver_HaulToPortal.ThingToCarry.def == t.ThingDef)
				{
					allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
	}
}
