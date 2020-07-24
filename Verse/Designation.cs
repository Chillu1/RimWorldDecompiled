using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Designation : IExposable
	{
		public DesignationManager designationManager;

		public DesignationDef def;

		public LocalTargetInfo target;

		public const float ClaimedDesignationDrawAltitude = 15f;

		private Map Map => designationManager.map;

		public float DesignationDrawAltitude => AltitudeLayer.MetaOverlays.AltitudeFor();

		public Designation()
		{
		}

		public Designation(LocalTargetInfo target, DesignationDef def)
		{
			this.target = target;
			this.def = def;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_TargetInfo.Look(ref target, "target");
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

		public virtual void DesignationDraw()
		{
			if (!target.HasThing || target.Thing.Spawned)
			{
				Vector3 position = default(Vector3);
				if (target.HasThing)
				{
					position = target.Thing.DrawPos;
					position.y = DesignationDrawAltitude;
				}
				else
				{
					position = target.Cell.ToVector3ShiftedWithAltitude(DesignationDrawAltitude);
				}
				Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, def.iconMat, 0);
			}
		}

		public void Delete()
		{
			Map.designationManager.RemoveDesignation(this);
		}

		public override string ToString()
		{
			return string.Format(string.Concat("(", def.defName, " target=", target, ")"));
		}
	}
}
