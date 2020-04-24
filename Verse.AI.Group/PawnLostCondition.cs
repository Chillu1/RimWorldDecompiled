namespace Verse.AI.Group
{
	public enum PawnLostCondition : byte
	{
		Undefined,
		Vanished,
		IncappedOrKilled,
		MadePrisoner,
		ChangedFaction,
		ExitedMap,
		LeftVoluntarily,
		Drafted,
		ForcedToJoinOtherLord,
		ForcedByPlayerAction,
		ForcedByQuest,
		NoLongerEnteringTransportPods
	}
}
