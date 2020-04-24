using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompAssignableToPawn : ThingComp
	{
		protected List<Pawn> assignedPawns = new List<Pawn>();

		public CompProperties_AssignableToPawn Props => (CompProperties_AssignableToPawn)props;

		public int MaxAssignedPawnsCount => Props.maxAssignedPawnsCount;

		public bool PlayerCanSeeAssignments
		{
			get
			{
				if (parent.Faction == Faction.OfPlayer)
				{
					return true;
				}
				for (int i = 0; i < assignedPawns.Count; i++)
				{
					if (assignedPawns[i].Faction == Faction.OfPlayer || assignedPawns[i].HostFaction == Faction.OfPlayer)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				if (!parent.Spawned)
				{
					return Enumerable.Empty<Pawn>();
				}
				return parent.Map.mapPawns.FreeColonists;
			}
		}

		public List<Pawn> AssignedPawnsForReading => assignedPawns;

		public IEnumerable<Pawn> AssignedPawns => assignedPawns;

		public bool HasFreeSlot => assignedPawns.Count < Props.maxAssignedPawnsCount;

		protected virtual bool CanDrawOverlayForPawn(Pawn pawn)
		{
			return true;
		}

		public override void DrawGUIOverlay()
		{
			if (Props.drawAssignmentOverlay && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && PlayerCanSeeAssignments)
			{
				Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
				if (!assignedPawns.Any())
				{
					GenMapUI.DrawThingLabel(parent, "Unowned".Translate(), defaultThingLabelColor);
				}
				if (assignedPawns.Count == 1 && CanDrawOverlayForPawn(assignedPawns[0]))
				{
					GenMapUI.DrawThingLabel(parent, assignedPawns[0].LabelShort, defaultThingLabelColor);
				}
			}
		}

		protected virtual void SortAssignedPawns()
		{
			assignedPawns.SortBy((Pawn x) => x.thingIDNumber);
		}

		public virtual void ForceAddPawn(Pawn pawn)
		{
			if (!assignedPawns.Contains(pawn))
			{
				assignedPawns.Add(pawn);
			}
			SortAssignedPawns();
		}

		public virtual void ForceRemovePawn(Pawn pawn)
		{
			if (assignedPawns.Contains(pawn))
			{
				assignedPawns.Remove(pawn);
			}
			SortAssignedPawns();
		}

		public virtual AcceptanceReport CanAssignTo(Pawn pawn)
		{
			return AcceptanceReport.WasAccepted;
		}

		public virtual void TryAssignPawn(Pawn pawn)
		{
			if (!assignedPawns.Contains(pawn))
			{
				assignedPawns.Add(pawn);
				SortAssignedPawns();
			}
		}

		public virtual void TryUnassignPawn(Pawn pawn, bool sort = true)
		{
			if (assignedPawns.Contains(pawn))
			{
				assignedPawns.Remove(pawn);
				if (sort)
				{
					SortAssignedPawns();
				}
			}
		}

		public virtual bool AssignedAnything(Pawn pawn)
		{
			return assignedPawns.Contains(pawn);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref assignedPawns, "assignedPawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				assignedPawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void PostDeSpawn(Map map)
		{
			for (int num = assignedPawns.Count - 1; num >= 0; num--)
			{
				TryUnassignPawn(assignedPawns[num], sort: false);
			}
		}
	}
}
