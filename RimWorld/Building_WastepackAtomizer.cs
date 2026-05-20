using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_WastepackAtomizer : Building
{
	private CompAtomizer atomizer;

	private CompPowerTrader powerTrader;

	private Effecter operatingEffecter;

	private Graphic contentsGraphic;

	public CompAtomizer Atomizer
	{
		get
		{
			if (atomizer == null)
			{
				atomizer = GetComp<CompAtomizer>();
			}
			return atomizer;
		}
	}

	private CompPowerTrader PowerTrader
	{
		get
		{
			if (powerTrader == null)
			{
				powerTrader = GetComp<CompPowerTrader>();
			}
			return powerTrader;
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (Atomizer.Empty || !PowerTrader.PowerOn)
		{
			operatingEffecter?.Cleanup();
			operatingEffecter = null;
			return;
		}
		if (operatingEffecter == null)
		{
			operatingEffecter = def.building.wastepackAtomizerOperationEffecter.Spawn();
			operatingEffecter.Trigger(this, new TargetInfo(InteractionCell, base.Map));
		}
		operatingEffecter.EffectTick(this, new TargetInfo(InteractionCell, base.Map));
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		Vector3 loc = drawLoc;
		loc.y -= 0.054878052f;
		def.building.wastepackAtomizerBottomGraphic.Graphic.Draw(loc, base.Rotation, this);
		Vector3 loc2 = drawLoc;
		loc2.y -= 0.018292684f;
		def.building.wastepackAtomizerWindowGraphic.Graphic.Draw(loc2, base.Rotation, this);
	}
}
