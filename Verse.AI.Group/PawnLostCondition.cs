namespace Verse.AI.Group;

public enum PawnLostCondition : byte
{
	Undefined,
	Vanished,
	Incapped,
	Killed,
	MadePrisoner,
	ChangedFaction,
	ExitedMap,
	LeftVoluntarily,
	Drafted,
	ForcedToJoinOtherLord,
	ForcedByPlayerAction,
	ForcedByQuest,
	NoLongerEnteringTransportPods,
	MadeSlave,
	InMentalState,
	LordRejected
}
