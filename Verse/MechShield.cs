using UnityEngine;

namespace Verse;

public class MechShield : ThingWithComps
{
	private Thing target;

	public override Vector3 DrawPos
	{
		get
		{
			if (target == null)
			{
				return base.DrawPos;
			}
			return target.DrawPos;
		}
	}

	public void SetTarget(Thing target)
	{
		this.target = target;
	}

	public bool IsTargeting(Thing target)
	{
		return this.target == target;
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		Comps_PostDraw();
	}

	protected override void Tick()
	{
		base.Tick();
		if (target != null)
		{
			base.Position = target.Position;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref target, "target");
	}
}
