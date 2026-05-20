using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GravshipLandingMarker : Thing
{
	public Gravship gravship;

	private CellRect selectorRect;

	private readonly List<IntVec3> gravshipCells = new List<IntVec3>();

	private readonly List<IntVec3> thrusterCells = new List<IntVec3>();

	private readonly List<IntVec3> doorCells = new List<IntVec3>();

	private static readonly Color diagonalsColor = new Color(1f, 1f, 1f, 0.5f);

	private List<IntVec3> tmpExclusionZoneCells = new List<IntVec3>();

	private readonly List<IntVec3> tmpGravshipCells = new List<IntVec3>();

	private readonly List<IntVec3> tmpThrusterCells = new List<IntVec3>();

	public IEnumerable<IntVec3> GravshipCells => gravshipCells;

	public IEnumerable<IntVec3> ThrusterCells => thrusterCells;

	public IEnumerable<IntVec3> DoorCells => doorCells;

	protected bool Visible
	{
		get
		{
			if (base.Spawned && gravship != null)
			{
				return !(Find.DesignatorManager.SelectedDesignator is Designator_MoveGravship);
			}
			return false;
		}
	}

	public Rot4 GravshipRotation
	{
		get
		{
			return gravship.Rotation;
		}
		set
		{
			if (!(gravship.Rotation == value))
			{
				gravship.Rotation = value;
				CacheCells();
				Notify_Moved();
			}
		}
	}

	public override CellRect? CustomRectForSelector
	{
		get
		{
			if (Visible)
			{
				return selectorRect;
			}
			return null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref gravship, "gravship");
		Scribe_Values.Look(ref selectorRect, "selectorRect");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			Current.Game.Gravship = gravship;
			CacheCells();
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckOdyssey("Gravship"))
		{
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		if (Find.CurrentMap != map || Find.World.renderer.wantedMode == WorldRenderMode.Planet)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				CameraJumper.TryJump(this, CameraJumper.MovementMode.Cut);
			});
		}
		CacheCells();
		Notify_Moved();
		if (!respawningAfterLoad)
		{
			Find.GravshipController.Notify_LandingAreaConfirmationStarted(this);
		}
	}

	public void CacheCells()
	{
		gravshipCells.Clear();
		gravshipCells.AddRange(gravship.Foundations.Keys);
		doorCells.Clear();
		thrusterCells.Clear();
		tmpExclusionZoneCells.Clear();
		foreach (var (thing2, data2) in gravship.ThrusterPlacements)
		{
			thing2.def.GetCompProperties<CompProperties_GravshipThruster>().GetExclusionZone(data2.local, data2.rotation, ref tmpExclusionZoneCells);
			thrusterCells.AddRange(tmpExclusionZoneCells);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (!Visible)
		{
			return;
		}
		tmpGravshipCells.Clear();
		tmpGravshipCells.AddRange(GravshipCells);
		IntVec3 position = base.Position;
		for (int i = 0; i < tmpGravshipCells.Count; i++)
		{
			tmpGravshipCells[i] += position;
		}
		tmpThrusterCells.Clear();
		tmpThrusterCells.AddRange(ThrusterCells);
		for (int j = 0; j < tmpThrusterCells.Count; j++)
		{
			tmpThrusterCells[j] += position;
		}
		GenDraw.DrawFieldEdges(tmpGravshipCells);
		GenDraw.DrawDiagonalStripes(tmpGravshipCells, diagonalsColor);
		GenDraw.DrawFieldEdges(tmpThrusterCells, ColorLibrary.Orange);
		foreach (var (thing2, data2) in gravship.ExteriorDoorPlacements)
		{
			GhostUtility.GhostGraphicFor(thing2.Graphic, thing2.def, Color.white).DrawFromDef(GenThing.TrueCenter(position + data2.local, data2.rotation, thing2.def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), data2.rotation, thing2.def);
		}
	}

	public void Notify_Moved()
	{
		if (gravship == null)
		{
			return;
		}
		selectorRect = CellRect.CenteredOn(base.Position, 1);
		foreach (IntVec3 key in gravship.Foundations.Keys)
		{
			selectorRect = selectorRect.Encapsulate(key + base.Position);
		}
	}

	public void BeginLanding(WorldComponent_GravshipController gravshipController)
	{
		Map map = base.Map;
		CameraJumper.TryJump(this);
		Destroy();
		gravshipController.InitiateLanding(gravship, map, base.Position, GravshipRotation);
	}

	public override string GetInspectString()
	{
		return "ConfirmLandingToContinue".Translate();
	}
}
