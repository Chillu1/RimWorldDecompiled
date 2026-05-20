namespace RimWorld;

public class MusicTransition
{
	public MusicTransitionDef def { get; private set; }

	public MusicManagerPlay musicManager { get; private set; }

	public void InitializeWorker(MusicTransitionDef def, MusicManagerPlay musicManager)
	{
		this.def = def;
		this.musicManager = musicManager;
	}

	public virtual bool IsTransitionSatisfied()
	{
		if (def.dangerRequirement == MusicDangerRequirement.RequiresDanger && !musicManager.DangerMusicMode)
		{
			return false;
		}
		if (def.dangerRequirement == MusicDangerRequirement.RequiresNoDanger && musicManager.DangerMusicMode)
		{
			return false;
		}
		return true;
	}
}
