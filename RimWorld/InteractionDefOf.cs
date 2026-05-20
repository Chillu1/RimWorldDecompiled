namespace RimWorld;

[DefOf]
public static class InteractionDefOf
{
	public static InteractionDef Chitchat;

	public static InteractionDef DeepTalk;

	public static InteractionDef Insult;

	public static InteractionDef RomanceAttempt;

	public static InteractionDef MarriageProposal;

	public static InteractionDef BuildRapport;

	public static InteractionDef RecruitAttempt;

	public static InteractionDef SparkJailbreak;

	[MayRequireIdeology]
	public static InteractionDef ReduceWill;

	[MayRequireIdeology]
	public static InteractionDef EnslaveAttempt;

	[MayRequireIdeology]
	public static InteractionDef ConvertIdeoAttempt;

	[MayRequireAnomaly]
	public static InteractionDef PrisonerStudyAnomaly;

	[MayRequireAnomaly]
	public static InteractionDef InterrogateIdentity;

	[MayRequireIdeology]
	public static InteractionDef Suppress;

	[MayRequireIdeology]
	public static InteractionDef SparkSlaveRebellion;

	public static InteractionDef AnimalChat;

	public static InteractionDef TrainAttempt;

	public static InteractionDef TameAttempt;

	public static InteractionDef Nuzzle;

	public static InteractionDef ReleaseToWild;

	[MayRequireIdeology]
	public static InteractionDef Counsel_Success;

	[MayRequireIdeology]
	public static InteractionDef Counsel_Failure;

	[MayRequireIdeology]
	public static InteractionDef Convert_Success;

	[MayRequireIdeology]
	public static InteractionDef Convert_Failure;

	[MayRequireIdeology]
	public static InteractionDef Reassure;

	[MayRequireIdeology]
	public static InteractionDef Trial_Accuse;

	[MayRequireIdeology]
	public static InteractionDef Trial_Defend;

	[MayRequireIdeology]
	public static InteractionDef Speech_AcceptRole;

	[MayRequireIdeology]
	public static InteractionDef Speech_RemoveRole;

	[MayRequireBiotech]
	public static InteractionDef BabyPlay;

	[MayRequireBiotech]
	public static InteractionDef LessonGeneric;

	[MayRequireAnomaly]
	public static InteractionDef InhumanRambling;

	[MayRequireAnomaly]
	public static InteractionDef OccultTeaching;

	[MayRequireAnomaly]
	public static InteractionDef DisturbingChat;

	[MayRequireAnomaly]
	public static InteractionDef CreepyWords;

	static InteractionDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(InteractionDefOf));
	}
}
