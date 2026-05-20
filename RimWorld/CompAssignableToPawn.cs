using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAssignableToPawn : ThingComp
{
	protected List<Pawn> assignedPawns = new List<Pawn>();

	protected List<Pawn> uninstalledAssignedPawns = new List<Pawn>();

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

	public int TotalSlots => Props.maxAssignedPawnsCount;

	protected virtual bool CanDrawOverlayForPawn(Pawn pawn)
	{
		return true;
	}

	public override void DrawGUIOverlay()
	{
		if (!Props.drawAssignmentOverlay || (!Props.drawUnownedAssignmentOverlay && !assignedPawns.Any()) || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest || !PlayerCanSeeAssignments)
		{
			return;
		}
		Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
		if (!assignedPawns.Any())
		{
			GenMapUI.DrawThingLabel(parent, "Unowned".Translate(), defaultThingLabelColor);
		}
		if (assignedPawns.Count == 1)
		{
			Pawn pawn = assignedPawns[0];
			if (CanDrawOverlayForPawn(pawn) && (!pawn.RaceProps.Animal || Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn)))
			{
				GenMapUI.DrawThingLabel(parent, assignedPawns[0].LabelShort, defaultThingLabelColor);
			}
		}
	}

	protected virtual void SortAssignedPawns()
	{
		assignedPawns.RemoveAll((Pawn x) => x == null);
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
		uninstalledAssignedPawns.Remove(pawn);
		SortAssignedPawns();
	}

	public virtual AcceptanceReport CanAssignTo(Pawn pawn)
	{
		return AcceptanceReport.WasAccepted;
	}

	public virtual bool IdeoligionForbids(Pawn pawn)
	{
		return false;
	}

	public virtual void TryAssignPawn(Pawn pawn)
	{
		uninstalledAssignedPawns.Remove(pawn);
		if (!assignedPawns.Contains(pawn))
		{
			assignedPawns.Add(pawn);
			SortAssignedPawns();
		}
	}

	public virtual void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
	{
		if (assignedPawns.Contains(pawn))
		{
			assignedPawns.Remove(pawn);
			if (uninstall && pawn != null && !uninstalledAssignedPawns.Contains(pawn))
			{
				uninstalledAssignedPawns.Add(pawn);
			}
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

	protected virtual bool ShouldShowAssignmentGizmo()
	{
		return parent.Faction == Faction.OfPlayer;
	}

	protected virtual string GetAssignmentGizmoLabel()
	{
		return "CommandThingSetOwnerLabel".Translate();
	}

	protected virtual string GetAssignmentGizmoDesc()
	{
		return "";
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (ShouldShowAssignmentGizmo())
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = GetAssignmentGizmoLabel();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
			command_Action.defaultDesc = GetAssignmentGizmoDesc();
			command_Action.action = delegate
			{
				Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
			};
			command_Action.hotKey = KeyBindingDefOf.Misc4;
			if (!Props.noAssignablePawnsDesc.NullOrEmpty() && !AssigningCandidates.Any())
			{
				command_Action.Disable(Props.noAssignablePawnsDesc);
			}
			yield return command_Action;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref assignedPawns, "assignedPawns", LookMode.Reference);
		Scribe_Collections.Look(ref uninstalledAssignedPawns, "uninstalledAssignedPawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			assignedPawns.RemoveAll((Pawn x) => x == null);
			uninstalledAssignedPawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			for (int num = assignedPawns.Count - 1; num >= 0; num--)
			{
				TryUnassignPawn(assignedPawns[num], sort: false, !parent.DestroyedOrNull());
			}
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		for (int num = uninstalledAssignedPawns.Count - 1; num >= 0; num--)
		{
			Pawn pawn = uninstalledAssignedPawns[num];
			if (CanSetUninstallAssignedPawn(pawn))
			{
				TryAssignPawn(pawn);
			}
		}
		uninstalledAssignedPawns.Clear();
	}

	public override void PostSwapMap()
	{
		for (int num = assignedPawns.Count - 1; num >= 0; num--)
		{
			if (assignedPawns[num].DestroyedOrNull() || !assignedPawns[num].SpawnedOrAnyParentSpawned)
			{
				TryUnassignPawn(assignedPawns[num]);
			}
		}
	}

	protected virtual bool CanSetUninstallAssignedPawn(Pawn pawn)
	{
		return false;
	}
}
