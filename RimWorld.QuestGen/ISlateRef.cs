namespace RimWorld.QuestGen
{
	public interface ISlateRef
	{
		string SlateRef
		{
			get;
			set;
		}

		bool TryGetConvertedValue<T>(Slate slate, out T value);
	}
}
