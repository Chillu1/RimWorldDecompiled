using Verse;

namespace RimWorld;

public struct MeditationSpotAndFocus
{
	public LocalTargetInfo spot;

	public LocalTargetInfo focus;

	public bool IsValid => spot.IsValid;

	public MeditationSpotAndFocus(LocalTargetInfo spot)
	{
		this.spot = spot;
		focus = LocalTargetInfo.Invalid;
	}

	public MeditationSpotAndFocus(LocalTargetInfo spot, LocalTargetInfo focus)
	{
		this.spot = spot;
		this.focus = focus;
	}
}
