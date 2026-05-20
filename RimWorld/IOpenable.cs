namespace RimWorld
{
	public interface IOpenable
	{
		bool CanOpen { get; }

		int OpenTicks { get; }

		void Open();
	}
}
