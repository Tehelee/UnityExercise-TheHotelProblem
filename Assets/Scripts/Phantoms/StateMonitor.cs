using System.Collections.Generic;
using UnityEngine;

// UniTask: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
using Cysharp.Threading.Tasks;

using StateHistoryDictionary = System.Collections.Generic.Dictionary<PlayerState, System.Collections.Generic.Dictionary<PlayerState, System.Collections.Generic.HashSet<PlayerState>>>;

public static class StateMonitor
{
	private static Queue<int> currentPlayers = new Queue<int>();

	private static Dictionary<int, Queue<PlayerState>> currentPlayerStates = new Dictionary<int, Queue<PlayerState>>();

	private static StateHistoryDictionary stateHistory = new StateHistoryDictionary();

	public delegate void OnPlayer( int playerId );

	public static event OnPlayer OnPlayerReady;
	public static event OnPlayer OnPlayerCleanup;

	[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded )]
	private static void Initialize()
	{
		NetworkedPlayerStates.OnPlayerJoin += OnPlayerJoin;
		NetworkedPlayerStates.OnPlayerLeave += OnPlayerLeave;

		foreach( int playerId in NetworkedPlayerStates.GetCurrentPlayers() )
			OnPlayerJoin( playerId );

		_ = UpdatePlayerStates();
	}

	private static void OnPlayerJoin( int playerId )
	{
		currentPlayers.Enqueue( playerId );

		PlayerState currentPlayerState = NetworkedPlayerStates.GetCurrentPlayerState( playerId );

		currentPlayerStates[ playerId ] = new Queue<PlayerState>( new[] { currentPlayerState } );

		OnPlayerReady?.Invoke( playerId );
	}

	private static void OnPlayerLeave( int playerId )
	{
		OnPlayerCleanup?.Invoke( playerId );

		Queue<int> remainingPlayers = new Queue<int>();

		int[] currentPlayersArray = currentPlayers.ToArray();

		for( int i = 0, iC = currentPlayersArray.Length; i < iC; i++ )
		{
			int currentPlayerId = currentPlayersArray[ i ];

			if( currentPlayerId != playerId )
				remainingPlayers.Enqueue( currentPlayerId );
		}

		currentPlayers = remainingPlayers;

		if( currentPlayerStates.ContainsKey( playerId ) )
			currentPlayerStates.Remove( playerId );
	}

	private static async UniTaskVoid UpdatePlayerStates()
	{
		while( true )
		{
			await UniTask.SwitchToTaskPool();

			if( currentPlayers.Count == 0 )
				await UniTask.WaitUntil( () => currentPlayers.Count > 0 );
			
			await UniTask.Delay( 60000 / currentPlayers.Count );

			await UniTask.SwitchToMainThread();

			if( currentPlayers.Count == 0 )
				continue;

			int playerId = currentPlayers.Dequeue();
			currentPlayers.Enqueue( playerId );
			
			currentPlayerStates[ playerId ].Enqueue( NetworkedPlayerStates.GetCurrentPlayerState( playerId ) );
		}
	}

	public static int[] GetCurrentPlayers() => currentPlayers.ToArray();

	public static PlayerState? GetLatestPlayerState( int playerId )
	{
		if( !currentPlayerStates.ContainsKey( playerId ) )
			return null;

		if( currentPlayerStates[ playerId ].Count == 0 )
			return null;

		return currentPlayerStates[ playerId ].Dequeue();
	}

	public static void RecordPlayerStateProgression( PlayerState first, PlayerState second, PlayerState? third = null )
	{
		if( third.HasValue && second == third && first == second )
			return; // Prevent loops

		if( !stateHistory.ContainsKey( first ) )
			stateHistory[ first ] = new Dictionary<PlayerState, HashSet<PlayerState>>();

		if( !stateHistory[ first ].ContainsKey( second ) )
			stateHistory[ first ][ second ] = new HashSet<PlayerState>();

		if( third.HasValue )
		{
			stateHistory[ first ][ second ].Add( third.Value );

			RecordPlayerStateProgression( second, third.Value );
		}
	}

	public static PlayerState? GetPotentialPlayerState( PlayerState first, PlayerState? second = null )
	{
		if( stateHistory.ContainsKey( first ) )
		{
			if( second.HasValue )
			{
				if( !stateHistory[ first ].ContainsKey( second.Value ) || stateHistory[ first ][ second.Value ].Count == 0 )
				{
					return GetPotentialPlayerState( second.Value );
				}
				else
				{
					var potentialStates = stateHistory[ first ][ second.Value ];
					var enumarator = potentialStates.GetEnumerator();

					if( potentialStates.Count > 1 )
					{
						for( int i = 0, iC = Random.Range( 0, potentialStates.Count ); i < iC; i++ )
						{
							if( !enumarator.MoveNext() )
								break;
						}
					}

					return enumarator.Current;
				}
			}
			else
			{
				var potentialStates = stateHistory[ first ].Keys;
				var enumarator = potentialStates.GetEnumerator();

				if( potentialStates.Count > 1 )
				{
					for( int i = 0, iC = Random.Range( 0, potentialStates.Count ); i < iC; i++ )
					{
						if( !enumarator.MoveNext() )
							break;
					}
				}

				return enumarator.Current;
			}
		}

		return null;
	}
}
