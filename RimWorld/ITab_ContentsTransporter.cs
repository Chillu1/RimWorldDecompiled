using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ITab_ContentsTransporter : ITab_ContentsBase
	{
		public override IList<Thing> container => Transporter.innerContainer;

		public CompTransporter Transporter => base.SelThing.TryGetComp<CompTransporter>();

		public override bool IsVisible
		{
			get
			{
				if ((base.SelThing.Faction == null || base.SelThing.Faction == Faction.OfPlayer) && Transporter != null)
				{
					if (!Transporter.LoadingInProgressOrReadyToLaunch)
					{
						return Transporter.innerContainer.Any;
					}
					return true;
				}
				return false;
			}
		}

		public ITab_ContentsTransporter()
		{
			labelKey = "TabTransporterContents";
			containedItemsKey = "ContainedItems";
		}

		protected override void DoItemsLists(Rect inRect, ref float curY)
		{
			CompTransporter transporter = Transporter;
			Rect position = new Rect(0f, curY, (inRect.width - 10f) / 2f, inRect.height);
			Text.Font = GameFont.Small;
			bool flag = false;
			float curY2 = 0f;
			GUI.BeginGroup(position);
			Widgets.ListSeparator(ref curY2, position.width, "ItemsToLoad".Translate());
			if (transporter.leftToLoad != null)
			{
				for (int i = 0; i < transporter.leftToLoad.Count; i++)
				{
					TransferableOneWay t = transporter.leftToLoad[i];
					if (t.CountToTransfer > 0 && t.HasAnyThing)
					{
						flag = true;
						DoThingRow(t.ThingDef, t.CountToTransfer, t.things, position.width, ref curY2, delegate(int x)
						{
							OnDropToLoadThing(t, x);
						});
					}
				}
			}
			if (!flag)
			{
				Widgets.NoneLabel(ref curY2, position.width);
			}
			GUI.EndGroup();
			Rect inRect2 = new Rect((inRect.width + 10f) / 2f, curY, (inRect.width - 10f) / 2f, inRect.height);
			float curY3 = 0f;
			base.DoItemsLists(inRect2, ref curY3);
			curY += Mathf.Max(curY2, curY3);
		}

		protected override void OnDropThing(Thing t, int count)
		{
			base.OnDropThing(t, count);
			Pawn pawn = t as Pawn;
			if (pawn != null)
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
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					RemovePawnFromLoadLord(pawn);
				}
			}
		}

		private void EndJobForEveryoneHauling(TransferableOneWay t)
		{
			List<Pawn> allPawnsSpawned = base.SelThing.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
				{
					JobDriver_HaulToTransporter jobDriver_HaulToTransporter = (JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver;
					if (jobDriver_HaulToTransporter.Transporter == Transporter && jobDriver_HaulToTransporter.ThingToCarry != null && jobDriver_HaulToTransporter.ThingToCarry.def == t.ThingDef)
					{
						allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}
	}
}
