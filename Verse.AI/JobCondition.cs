using System;

namespace Verse.AI;

[Flags]
public enum JobCondition : byte
{
	None = 0,
	Ongoing = 1,
	Succeeded = 2,
	Incompletable = 4,
	InterruptOptional = 8,
	InterruptForced = 0x10,
	QueuedNoLongerValid = 0x20,
	Errored = 0x40,
	ErroredPather = 0x80
}
