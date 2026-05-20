using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class CompDryadHolder : ThingComp, IThingHolder, ISuspendableThingHolder
{
	protected int tickComplete = -1;

	protected ThingOwner innerContainer;

	protected Thing tree;

	private CompTreeConnection cachedTreeComp;

	private Material cachedFrontMat;

	protected const int ExpiryDurationTicks = 600;

	public CompProperties_DryadCocoon Props => (CompProperties_DryadCocoon)props;

	public bool IsContentsSuspended => true;

	protected CompTreeConnection TreeComp
	{
		get
		{
			if (cachedTreeComp == null)
			{
				cachedTreeComp = tree?.TryGetComp<CompTreeConnection>();
			}
			return cachedTreeComp;
		}
	}

	private Material FrontMat
	{
		get
		{
			if (cachedFrontMat == null)
			{
				cachedFrontMat = MaterialPool.MatFrom("Things/Building/Misc/DryadSphere/DryadSphereFront", ShaderDatabase.Cutout);
			}
			return cachedFrontMat;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}
	}

	public override void CompTick()
	{
		if (tickComplete >= 0)
		{
			if (tree == null || tree.Destroyed)
			{
				parent.Destroy();
			}
			else if (Find.TickManager.TicksGame >= tickComplete)
			{
				Complete();
			}
		}
	}

	public override void PostDraw()
	{
		if (Props.drawContents)
		{
			for (int i = 0; i < innerContainer.Count; i++)
			{
				innerContainer[i].DrawNowAt(parent.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop));
			}
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 pos = parent.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop.AltitudeFor() + 0.01f);
			Quaternion q = Quaternion.Euler(0f, parent.Rotation.AsAngle, 0f);
			Vector3 s = new Vector3(parent.Graphic.drawSize.x, 1f, parent.Graphic.drawSize.y);
			matrix.SetTRS(pos, q, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, FrontMat, 0);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (tickComplete >= 0 && DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Complete",
				action = Complete
			};
		}
	}

	public virtual void TryAcceptPawn(Pawn p)
	{
		bool num = p.DeSpawnOrDeselect();
		innerContainer.TryAddOrTransfer(p, 1);
		if (num)
		{
			Find.Selector.Select(p, playSound: false, forceDesignatorDeselect: false);
		}
		SoundDefOf.Pawn_EnterDryadPod.PlayOneShot(SoundInfo.InMap(parent));
		if (p.connections == null)
		{
			return;
		}
		foreach (Thing connectedThing in p.connections.ConnectedThings)
		{
			if (connectedThing.TryGetComp<CompTreeConnection>() != null)
			{
				tree = connectedThing;
				break;
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		text += "CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
		if (tickComplete >= 0)
		{
			text = string.Concat(text, "\n", "TimeLeft".Translate().CapitalizeFirst(), ": ", (tickComplete - Find.TickManager.TicksGame).ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
		}
		return text;
	}

	protected abstract void Complete();

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref tickComplete, "tickComplete", -1);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_References.Look(ref tree, "tree");
	}
}
