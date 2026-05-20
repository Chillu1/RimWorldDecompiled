namespace Verse.AI.Group;

public enum TriggerSignalType : byte
{
	Undefined,
	Tick,
	Memo,
	PawnDamaged,
	PawnArrestAttempted,
	PawnLost,
	BuildingDamaged,
	BuildingLost,
	FactionRelationsChanged,
	DormancyWakeup,
	Clamor,
	MechClusterDefeated,
	Signal,
	CorpseLost,
	AcquiredTarget
}
