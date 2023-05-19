public static class NetworkedPlayerStates
{
	public delegate void OnPlayer( int playerId );

	public static event OnPlayer OnPlayerJoin;
	public static event OnPlayer OnPlayerLeave;

	public static int[] GetCurrentPlayers() => default;

	public static PlayerState GetCurrentPlayerState( int playerId ) => default;
}
