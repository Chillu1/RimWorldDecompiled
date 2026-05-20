namespace KTrie
{
	public struct StringEntry<TValue>
	{
		public string Key { get; }

		public TValue Value { get; }

		public StringEntry(string key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}
}
