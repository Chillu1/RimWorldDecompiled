using UnityEngine;

namespace Verse
{
	public abstract class AttachableThing : Thing
	{
		public Thing parent;

		public override Vector3 DrawPos
		{
			get
			{
				if (parent != null)
				{
					return parent.DrawPos + Vector3.up * (3f / 70f) * 0.9f;
				}
				return base.DrawPos;
			}
		}

		public abstract string InspectStringAddon
		{
			get;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref parent, "parent");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && parent != null)
			{
				AttachTo(parent);
			}
		}

		public virtual void AttachTo(Thing parent)
		{
			this.parent = parent;
			CompAttachBase compAttachBase = parent.TryGetComp<CompAttachBase>();
			if (compAttachBase == null)
			{
				Log.Error(string.Concat("Cannot attach ", this, " to ", parent, ": parent has no CompAttachBase."));
			}
			else
			{
				compAttachBase.AddAttachment(this);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			if (parent != null)
			{
				parent.TryGetComp<CompAttachBase>().RemoveAttachment(this);
			}
		}
	}
}
