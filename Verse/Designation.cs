using RimWorld;
using UnityEngine;

namespace Verse;

public class Designation : IExposable
{
	public DesignationManager designationManager;

	public DesignationDef def;

	public LocalTargetInfo target;

	public ColorDef colorDef;

	[Unsaved(false)]
	private Material cachedMaterial;

	public const float ClaimedDesignationDrawAltitude = 15f;

	private Map Map => designationManager.map;

	public float DesignationDrawAltitude => AltitudeLayer.MetaOverlays.AltitudeFor();

	public Material IconMat
	{
		get
		{
			if (cachedMaterial == null)
			{
				if (colorDef != null)
				{
					cachedMaterial = new Material(def.iconMat);
					cachedMaterial.color = colorDef.color;
				}
				else
				{
					cachedMaterial = def.iconMat;
				}
			}
			return cachedMaterial;
		}
	}

	public Designation()
	{
	}

	public Designation(LocalTargetInfo target, DesignationDef def, ColorDef colorDef = null)
	{
		this.target = target;
		this.def = def;
		this.colorDef = colorDef;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_TargetInfo.Look(ref target, "target");
		Scribe_Defs.Look(ref colorDef, "colorDef");
		if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && def == DesignationDefOf.Haul && !target.HasThing)
		{
			Log.Error("Haul designation has no target! Deleting.");
			Delete();
		}
	}

	public void Notify_Added()
	{
		if (def == DesignationDefOf.Haul)
		{
			Map.listerHaulables.HaulDesignationAdded(target.Thing);
		}
	}

	internal void Notify_Removing()
	{
		if (def == DesignationDefOf.Haul && target.HasThing)
		{
			Map.listerHaulables.HaulDesignationRemoved(target.Thing);
		}
	}

	public Vector3 DrawLoc()
	{
		if (target.HasThing)
		{
			Vector3 drawPos = target.Thing.DrawPos;
			drawPos.y = DesignationDrawAltitude;
			if (target.Thing.def.building != null && target.Thing.def.building.isAttachment)
			{
				drawPos += (target.Thing.Rotation.AsVector2 * 0.5f).ToVector3();
			}
			return drawPos;
		}
		return target.Cell.ToVector3ShiftedWithAltitude(DesignationDrawAltitude);
	}

	public virtual void DesignationDraw()
	{
		if (!target.HasThing || target.Thing.Spawned)
		{
			Graphics.DrawMesh(MeshPool.plane10, DrawLoc(), Quaternion.identity, IconMat, 0);
		}
	}

	public void Delete()
	{
		Map.designationManager.RemoveDesignation(this);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "(", def.defName, " target=", null, null };
		LocalTargetInfo localTargetInfo = target;
		obj[3] = localTargetInfo.ToString();
		obj[4] = ")";
		return string.Format(string.Concat(obj));
	}
}
