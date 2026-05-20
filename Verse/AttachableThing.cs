using UnityEngine;

namespace Verse;

public abstract class AttachableThing : ThingWithComps, IThingGlower
{
	public Thing parent;

	public override Vector3 DrawPos
	{
		get
		{
			if (parent != null)
			{
				return parent.DrawPos + Vector3.up * 0.03292683f;
			}
			return base.DrawPos;
		}
	}

	public abstract string InspectStringAddon { get; }

	public virtual bool ShouldBeLitNow()
	{
		return parent != null;
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

	public virtual void AttachTo(Thing newParent)
	{
		parent = newParent;
		if (!newParent.TryGetComp(out CompAttachBase comp))
		{
			Log.Error("Cannot attach " + this?.ToString() + " to " + newParent?.ToString() + ": parent has no CompAttachBase.");
		}
		else
		{
			comp.AddAttachment(this);
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
