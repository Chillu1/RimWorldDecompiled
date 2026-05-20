using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Crater : Building
{
	protected virtual EffecterDef FilledInEffecter => EffecterDefOf.ImpactSmallDustCloud;

	protected virtual SoundDef FilledInSound => SoundDefOf.Crater_FilledIn;

	public virtual void FillIn()
	{
		FilledInEffecter?.Spawn(base.Position, base.Map).Cleanup();
		FilledInSound?.PlayOneShot(this);
		Destroy(DestroyMode.Deconstruct);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				action = FillIn,
				defaultLabel = "DEV: Fill in"
			};
		}
	}
}
