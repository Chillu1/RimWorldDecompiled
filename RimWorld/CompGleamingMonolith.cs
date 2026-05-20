using Verse;

namespace RimWorld;

public class CompGleamingMonolith : CompVoidStructure
{
	private const int TicksToReactivate = 600;

	private const int ActivateStunTicks = 960;

	protected override bool Activatable => Find.Anomaly.LevelDef == MonolithLevelDefOf.Gleaming;

	public override bool Active => false;

	protected override SoundDef AmbientSound => null;

	public override int TicksToActivate
	{
		get
		{
			if (!activated)
			{
				return base.Props.ticksToActivate;
			}
			return 600;
		}
	}

	protected override void OnInteracted(Pawn caster)
	{
		base.OnInteracted(caster);
		progress = 0f;
		caster.stances.stunner.StunFor(960, parent, addBattleLog: true, showMote: false, disableRotation: true);
	}
}
