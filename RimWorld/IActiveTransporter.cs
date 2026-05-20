using Verse;

namespace RimWorld;

public interface IActiveTransporter : IThingHolder
{
	ActiveTransporterInfo Contents { get; }
}
