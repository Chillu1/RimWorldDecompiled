namespace Verse;

public interface IThingHolderEvents<in T> where T : Thing
{
	void Notify_ItemAdded(T item);

	void Notify_ItemRemoved(T item);
}
