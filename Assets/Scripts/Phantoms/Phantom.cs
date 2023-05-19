using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// UniTask: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
using Cysharp.Threading.Tasks;

[RequireComponent( typeof( ActivityAnimator ) )]
public class Phantom : MonoBehaviour
{
	public int mimeStepMS = 1000;

	public Location location = new Location();
	
	private ActivityAnimator activityAnimator;

	private int playerId = -1;

	private CancellationTokenSource mimicPlayerTask;

	private List<PlayerState> stateHistory;

	private void Awake()
	{
		activityAnimator = GetComponent<ActivityAnimator>();
	}

	private void OnDisable()
	{
		if( mimicPlayerTask != null )
		{
			mimicPlayerTask.Cancel();
			mimicPlayerTask.Dispose();
			mimicPlayerTask = null;
		}

		playerId = -1;
	}

	public void Setup( int playerId )
	{
		this.playerId = playerId;

		mimicPlayerTask = new CancellationTokenSource();
		MimicPlayer().Preserve().AttachExternalCancellation( mimicPlayerTask.Token );
	}

	public async UniTask MoveAndMimeState( PlayerState playerState )
	{
		if( location != playerState.location )
		{
			// Move
			while( location.point != playerState.location.point )
			{
				await UniTask.Delay( mimeStepMS );

				Heading? heading = WorldNavigation.GetNextStepTowardsPoint( location.point, playerState.location.point );

				if( !heading.HasValue )
				{
					// If we can't get there, bail out of matching this state.
					// If it's acceptable to simply warp to the goal when stuck, this should be a "break;" instead

					return;
				}

				location.heading = heading.Value;
				location.point.Translate( location.heading, 1 );
			}

			// Force set the location to match heading and any potential point discrepancy
			location = playerState.location;
		}

		int actionDuration = activityAnimator.PlayAction( playerState.action, false );

		if( actionDuration > 0 )
			await UniTask.Delay( actionDuration );

		await UniTask.Delay( mimeStepMS );

		// Cache state history

		stateHistory.Add( playerState );

		if( stateHistory.Count > 3 )
			stateHistory.RemoveAt( 0 );

		switch( stateHistory.Count )
		{
			case 1:
				break;
			case 2:
				StateMonitor.RecordPlayerStateProgression( stateHistory[ 0 ], stateHistory[ 1 ] );
				break;
			case 3:
				StateMonitor.RecordPlayerStateProgression( stateHistory[ 0 ], stateHistory[ 1 ], stateHistory[ 2 ] );
				break;
		}

		// In an ideal scenario with a method of isolating code to be server side only, this code would live on the players there, not on the client phantoms
		
		switch( stateHistory.Count )
		{
			case 1:
				break;
			case 2:
				ActionOracle.RecordActionProgression( stateHistory[ 0 ].action, stateHistory[ 1 ].action );
				break;
			case 3:
				ActionOracle.RecordActionProgression( stateHistory[ 0 ].action, stateHistory[ 1 ].action, stateHistory[ 2 ].action );
				break;
		}
	}

	public async UniTask MimicPlayer()
	{
		bool hasMatchedInitialState = false;

		while( true )
		{
			// Latest Player State
			PlayerState? playerState = StateMonitor.GetLatestPlayerState( playerId );

			// StateMonitor History
			if( !playerState.HasValue && stateHistory.Count > 0 )
			{
				playerState = stateHistory.Count > 1 ?
					StateMonitor.GetPotentialPlayerState( stateHistory[ 0 ], stateHistory[ 1 ] ) :
					StateMonitor.GetPotentialPlayerState( stateHistory[ 0 ] );	
			}

			// ActionOracle Guesser
			if( !playerState.HasValue )
			{
				Action? action = stateHistory.Count > 1 ?
					ActionOracle.GetPotentialAction( stateHistory[ 0 ].action, stateHistory[ 1 ].action ) :
					ActionOracle.GetPotentialAction( stateHistory[ 0 ].action );

				if( action.HasValue )
				{
					Location target = WorldNavigation.GetNearestActionPoint( location.point, 0, action.Value );

					playerState = new PlayerState( action.Value, target );
				}
			}

			// Mime Player State
			if( playerState.HasValue )
			{
				if( !hasMatchedInitialState )
				{
					location = playerState.Value.location;

					activityAnimator.PlayAction( playerState.Value.action, true );

					hasMatchedInitialState = true;
				}
				else
				{
					await MoveAndMimeState( playerState.Value );
				}
			}

			await UniTask.Delay( mimeStepMS );
		}
	}
}
