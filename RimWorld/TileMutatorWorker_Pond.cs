namespace RimWorld;

public class TileMutatorWorker_Pond : TileMutatorWorker_Lake
{
	protected override float LakeRadius => 0.3f;

	protected override bool GenerateDeepWater => false;

	public TileMutatorWorker_Pond(TileMutatorDef def)
		: base(def)
	{
	}
}
