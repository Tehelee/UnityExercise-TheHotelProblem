using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomSpawner : MonoBehaviour
{
	private Dictionary<int, Phantom> playerPhantoms = new Dictionary<int, Phantom>();

	private void OnEnable()
	{
		StateMonitor.OnPlayerReady += OnPlayerReady;
		StateMonitor.OnPlayerCleanup += OnPlayerCleanup;

		foreach( int playerId in StateMonitor.GetCurrentPlayers() )
			OnPlayerReady( playerId );
	}

	private void OnDisable()
	{
		StateMonitor.OnPlayerReady -= OnPlayerReady;
		StateMonitor.OnPlayerCleanup -= OnPlayerCleanup;

		foreach( KeyValuePair<int, Phantom> kvp in playerPhantoms )
		{
			GameObject.Destroy( kvp.Value?.gameObject );
		}

		playerPhantoms.Clear();
	}

	private void OnPlayerReady( int playerId )
	{
		// Alternatively, this could be done with a prefab reference, hence why this class is not static.

		Phantom playerPhantom = new GameObject( $"Phantom[ {playerId} ]", typeof( ActivityAnimator ), typeof( Phantom ) ).GetComponent<Phantom>();

		playerPhantom.Setup( playerId );

		playerPhantoms[ playerId ] = playerPhantom;
	}

	private void OnPlayerCleanup( int playerId )
	{
		if( playerPhantoms.ContainsKey( playerId ) )
		{
			GameObject.Destroy( playerPhantoms[ playerId ]?.gameObject );

			playerPhantoms.Remove( playerId );
		}
	}
}
