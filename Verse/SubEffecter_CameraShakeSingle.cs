namespace Verse;

public class SubEffecter_CameraShakeSingle : SubEffecter_CameraShake
{
	public SubEffecter_CameraShakeSingle(SubEffecterDef subDef, Effecter parent)
		: base(subDef, parent)
	{
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		DoShake(A);
	}
}
