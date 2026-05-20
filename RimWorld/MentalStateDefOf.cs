using Verse;

namespace RimWorld;

[DefOf]
public static class MentalStateDefOf
{
	public static MentalStateDef Berserk;

	public static MentalStateDef BerserkPermanent;

	public static MentalStateDef BerserkMechanoid;

	public static MentalStateDef CocoonDisturbed;

	[MayRequireBiotech]
	public static MentalStateDef BerserkWarcall;

	[MayRequireAnomaly]
	public static MentalStateDef HumanityBreak;

	[MayRequireAnomaly]
	public static MentalStateDef EntityKiller;

	public static MentalStateDef Wander_Psychotic;

	public static MentalStateDef Wander_Sad;

	public static MentalStateDef Wander_OwnRoom;

	public static MentalStateDef PanicFlee;

	public static MentalStateDef Manhunter;

	public static MentalStateDef ManhunterPermanent;

	public static MentalStateDef SocialFighting;

	public static MentalStateDef Roaming;

	[MayRequireIdeology]
	public static MentalStateDef Rebellion;

	[MayRequireAnomaly]
	public static MentalStateDef ManhunterBloodRain;

	[MayRequireAnomaly]
	public static MentalStateDef CubeSculpting;

	[MayRequireOdyssey]
	public static MentalStateDef Terror;

	[MayRequireBiotech]
	public static MentalStateDef PanicFleeFire;

	static MentalStateDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
	}
}
