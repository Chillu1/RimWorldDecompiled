using Verse;

namespace RimWorld;

[DefOf]
public static class RulePackDefOf
{
	public static RulePackDef Sentence_SocialFightStarted;

	public static RulePackDef Sentence_RomanceAttemptAccepted;

	public static RulePackDef Sentence_RomanceAttemptRejected;

	public static RulePackDef Sentence_MarriageProposalAccepted;

	public static RulePackDef Sentence_MarriageProposalRejected;

	public static RulePackDef Sentence_MarriageProposalRejectedBrokeUp;

	public static RulePackDef Sentence_RecruitAttemptAccepted;

	public static RulePackDef Sentence_RecruitAttemptRejected;

	[MayRequireIdeology]
	public static RulePackDef Sentence_ConvertIdeoAttemptSuccess;

	[MayRequireIdeology]
	public static RulePackDef Sentence_ConvertIdeoAttemptFail;

	[MayRequireIdeology]
	public static RulePackDef Sentence_ConvertIdeoAttemptFailSocialFight;

	[MayRequireIdeology]
	public static RulePackDef Sentence_ConvertIdeoAttemptFailResentment;

	[MayRequireBiotech]
	public static RulePackDef NamerGenepack;

	[MayRequireBiotech]
	public static RulePackDef NamerXenotype;

	[MayRequireBiotech]
	public static RulePackDef GrowthMomentFlavor;

	public static RulePackDef ArtDescriptionRoot_HasTale;

	public static RulePackDef ArtDescriptionRoot_Taleless;

	public static RulePackDef ArtDescriptionUtility_Global;

	public static RulePackDef GlobalUtility;

	public static RulePackDef TalelessImages;

	public static RulePackDef NamerWorld;

	public static RulePackDef NamerTraderGeneral;

	public static RulePackDef NamerScenario;

	public static RulePackDef NamerQuestDefault;

	public static RulePackDef NamerArtSculpture;

	public static RulePackDef ArtDescription_Sculpture;

	public static RulePackDef NamerArtWeaponMelee;

	public static RulePackDef ArtDescription_WeaponMelee;

	public static RulePackDef NamerArtWeaponGun;

	public static RulePackDef ArtDescription_WeaponGun;

	public static RulePackDef NamerArtFurniture;

	public static RulePackDef ArtDescription_Furniture;

	public static RulePackDef NamerArtSarcophagusPlate;

	public static RulePackDef ArtDescription_SarcophagusPlate;

	public static RulePackDef SeedGenerator;

	public static RulePackDef Combat_RangedFire;

	public static RulePackDef Combat_RangedDamage;

	public static RulePackDef Combat_RangedDeflect;

	public static RulePackDef Combat_RangedMiss;

	public static RulePackDef Combat_ExplosionImpact;

	public static RulePackDef Transition_Downed;

	public static RulePackDef Transition_Died;

	public static RulePackDef Transition_DiedExplosive;

	public static RulePackDef DamageEvent_Ceiling;

	public static RulePackDef DamageEvent_Fire;

	public static RulePackDef DamageEvent_PowerBeam;

	public static RulePackDef DamageEvent_Tornado;

	public static RulePackDef DamageEvent_TrapSpike;

	[MayRequireAnomaly]
	public static RulePackDef DamageEvent_UnnaturalDarkness;

	public static RulePackDef Event_Stun;

	public static RulePackDef Event_AbilityUsed;

	public static RulePackDef Event_ItemUsed;

	public static RulePackDef Battle_Solo;

	public static RulePackDef Battle_Duel;

	public static RulePackDef Battle_Internal;

	public static RulePackDef Battle_War;

	public static RulePackDef Battle_Brawl;

	public static RulePackDef DynamicWrapper;

	[MayRequireAnomaly]
	public static RulePackDef LabyrinthRamblings;

	[MayRequireAnomaly]
	public static RulePackDef NamerBiosignature;

	[MayRequireAnomaly]
	public static RulePackDef NamerArtCubeSculpture;

	[MayRequireAnomaly]
	public static RulePackDef ArtDescription_CubeSculpture;

	[MayRequireAnomaly]
	public static RulePackDef NamerArtVoidSculpture;

	[MayRequireAnomaly]
	public static RulePackDef ArtDescription_VoidSculpture;

	[MayRequireAnomaly]
	public static RulePackDef RevenantNoises;

	[MayRequireAnomaly]
	public static RulePackDef NamerPersonCreepjoiner;

	[MayRequireAnomaly]
	public static RulePackDef Event_UnnaturalCorpseAttack;

	[MayRequireAnomaly]
	public static RulePackDef Event_DevourerConsumeLeap;

	[MayRequireAnomaly]
	public static RulePackDef Event_DevourerDigestionAborted;

	[MayRequireAnomaly]
	public static RulePackDef Event_DevourerDigestionCompleted;

	[MayRequireAnomaly]
	public static RulePackDef Event_MetalhorrorEmerged;

	[MayRequireAnomaly]
	public static RulePackDef Event_Hypnotized;

	[MayRequireOdyssey]
	public static RulePackDef NamerGravship;

	[MayRequireOdyssey]
	public static RulePackDef NamerUniqueWeapon;

	static RulePackDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDefOf));
	}
}
