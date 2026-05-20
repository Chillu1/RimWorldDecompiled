namespace RimWorld;

public interface IExpirableWorldObject
{
	int ExpireAtTicks { get; set; }
}
