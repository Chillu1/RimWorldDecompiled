using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MultiPawnGotoController
{
	private const float PawnLabelOffsetY = 5f;

	private const float PawnLabelAlpha = 0.5f;

	private static readonly Color FeedbackColor = GenColor.FromBytes(153, 207, 135);

	private const float GotoCircleScale = 1.7f;

	private const float GotoCircleAlpha = 0.4f;

	private static readonly Color GotoCircleColor = FeedbackColor * new Color(1f, 1f, 1f, 0.4f);

	private const string GotoCircleTexPath = "UI/Overlays/Circle75Solid";

	private static readonly Material GotoCircleMaterial = MaterialPool.MatFrom("UI/Overlays/Circle75Solid", ShaderDatabase.Transparent, GotoCircleColor);

	private const float GotoBetweenLineWidth = 0.9f;

	private const float GotoBetweenLineAlpha = 0.18f;

	private const string GotoBetweenLineTexPath = "UI/Overlays/ThickLine";

	private static readonly Color GotoBetweenLineColor = FeedbackColor * new Color(1f, 1f, 1f, 0.18f);

	private static readonly Material GotoBetweenLineMaterial = MaterialPool.MatFrom("UI/Overlays/ThickLine", ShaderDatabase.Transparent, GotoBetweenLineColor);

	private const float RecomputeDestinationsFrequencyTicks = 10f;

	private bool active;

	private IntVec3 start;

	private IntVec3 end;

	private int? lastUpdateTicks;

	private readonly List<Pawn> pawns = new List<Pawn>();

	private readonly List<IntVec3> dests = new List<IntVec3>();

	public bool Active => active;

	public void Deactivate()
	{
		active = false;
		pawns.Clear();
		dests.Clear();
		lastUpdateTicks = null;
	}

	public void StartInteraction(IntVec3 mouseCell)
	{
		Deactivate();
		start = (end = mouseCell);
		lastUpdateTicks = null;
	}

	public void FinalizeInteraction()
	{
		RecomputeDestinations();
		IssueGotoJobs();
		Deactivate();
	}

	public void AddPawn(Pawn pawn)
	{
		active = true;
		pawns.Add(pawn);
		dests.Add(IntVec3.Invalid);
		lastUpdateTicks = null;
	}

	public void ProcessInputEvents()
	{
		IntVec3 intVec = UI.MouseCell();
		if (!Active || !pawns.Any() || !pawns[0].Spawned || !intVec.InBounds(pawns[0].Map))
		{
			return;
		}
		int ticksGame = Find.TickManager.TicksGame;
		if (intVec != end || !lastUpdateTicks.HasValue || (float)ticksGame > (float?)lastUpdateTicks + 10f)
		{
			if (intVec != end)
			{
				SoundDefOf.DragGoto.PlayOneShotOnCamera();
			}
			end = intVec;
			lastUpdateTicks = ticksGame;
			RecomputeDestinations();
		}
	}

	public void RecomputeDestinations()
	{
		for (int i = 0; i < dests.Count; i++)
		{
			dests[i] = IntVec3.Invalid;
		}
		float num = ((pawns.Count <= 1) ? 1 : (pawns.Count - 1));
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn = pawns[j];
			if (pawn.Spawned)
			{
				IntVec3 root;
				if (pawn.Map.exitMapGrid.IsExitCell(end))
				{
					root = end;
				}
				else
				{
					float num2 = (float)j / num;
					root = (start.ToVector3() + (end.ToVector3() - start.ToVector3()) * num2).ToIntVec3();
				}
				IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(root, pawn, (IntVec3 c) => CanGoTo(pawn, c));
				if (ModsConfig.BiotechActive && pawn.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn, intVec))
				{
					intVec = IntVec3.Invalid;
				}
				dests[j] = intVec;
			}
		}
		bool CanGoTo(Pawn pawn2, IntVec3 c)
		{
			if (dests.Contains(c))
			{
				return false;
			}
			if (ModsConfig.BiotechActive && pawn2.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn2, c))
			{
				return false;
			}
			return true;
		}
	}

	private void IssueGotoJobs()
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			IntVec3 gotoLoc = dests[i];
			if (gotoLoc.IsValid)
			{
				FloatMenuOptionProvider_DraftedMove.PawnGotoAction(end, pawn, gotoLoc);
			}
		}
		SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
		if (start == end)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.GroupGotoHereDragging, OpportunityType.GoodToKnow);
		}
		else if ((float)start.DistanceToSquared(end) > 1.9f)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GroupGotoHereDragging, KnowledgeAmount.SpecificInteraction);
		}
	}

	public void Draw()
	{
		if (!active)
		{
			return;
		}
		Vector3 s = new Vector3(1.7f, 1f, 1.7f);
		float num = AltitudeLayer.MetaOverlays.AltitudeFor();
		float addedAltitude = num + 0.03658537f;
		float addedAltitude2 = num - 0.03658537f;
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			IntVec3 c = dests[i];
			if (c.IsValid && pawn.Spawned && !c.Fogged(pawn.Map))
			{
				Vector3 drawLoc = c.ToVector3ShiftedWithAltitude(num);
				pawn.Drawer.renderer.RenderPawnAt(drawLoc, Rot4.South);
				Vector3 pos = c.ToVector3ShiftedWithAltitude(addedAltitude);
				Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.identity, s), GotoCircleMaterial, 0);
			}
		}
		Vector3 a = start.ToVector3ShiftedWithAltitude(addedAltitude2);
		Vector3 b = end.ToVector3ShiftedWithAltitude(addedAltitude2);
		GenDraw.DrawLineBetween(a, b, GotoBetweenLineMaterial, 0.9f);
	}

	public void OnGUI()
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			IntVec3 c = dests[i];
			if (c.IsValid && pawn.Spawned && !c.Fogged(pawn.Map))
			{
				Rect rect = c.ToUIRect();
				Vector2 pos = new Vector2(rect.center.x, rect.yMax + 5f);
				GenMapUI.DrawPawnLabel(pawn, pos, 0.5f);
			}
		}
	}
}
