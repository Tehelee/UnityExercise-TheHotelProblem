using System.Collections.Generic;
using System.IO;
using UnityEngine;

// UniTask: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
using Cysharp.Threading.Tasks;

using ActionHistoryDictionary = System.Collections.Generic.Dictionary<Action, System.Collections.Generic.Dictionary<Action, System.Collections.Generic.Dictionary<Action, int>>>;

public static class ActionOracle
{
	private const int SaveDelayMS = 60000;

	private static string JsonFilePath => Path.Combine( Application.persistentDataPath, "ActionOracleHistory.json" );

	private static ActionHistoryDictionary history = new ActionHistoryDictionary();

	private static bool hasUnsavedModifications = false;

	[System.Serializable]
	private class HistoryJson
	{
		public First[] firsts;

		[System.Serializable]
		public class First
		{
			public Action action;

			public Second[] seconds;
		}

		[System.Serializable]
		public class Second
		{
			public Action action;

			public Third[] thirds;
		}

		[System.Serializable]
		public class Third
		{
			public Action action;

			public int weight;
		}
		
		public void ImportFromDictionary( in ActionHistoryDictionary history )
		{
			List<First> firsts = new List<First>();

			foreach( var firstDictionary in history )
			{
				List<Second> seconds = new List<Second>();

				foreach( var secondDictionary in firstDictionary.Value )
				{
					List<Third> thirds = new List<Third>();
					
					foreach( var thirdDictionary in secondDictionary.Value )
					{
						thirds.Add( new Third() { action = thirdDictionary.Key, weight = thirdDictionary.Value } );
					}

					seconds.Add( new Second() { action = secondDictionary.Key, thirds = thirds.ToArray() } );
				}

				firsts.Add( new First() { action = firstDictionary.Key, seconds = seconds.ToArray() } );
			}

			this.firsts = firsts.ToArray();
		}

		public void ExportToDictionary( ref ActionHistoryDictionary history )
		{
			foreach( First first in firsts )
			{
				if( !history.ContainsKey( first.action ) )
					history[ first.action ] = new Dictionary<Action, Dictionary<Action, int>>();

				foreach( Second second in first.seconds )
				{
					if( !history[ first.action ].ContainsKey( second.action ) )
						history[ first.action ][ second.action ] = new Dictionary<Action, int>();

					foreach( Third third in second.thirds )
					{
						history[ first.action ][ second.action ][ third.action ] = third.weight;
					}
				}
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		LoadJson();
		
		_ = SaveWhenDirtyTask();
	}

	private static void LoadJson()
	{
		string json = File.ReadAllText( JsonFilePath );

		if( string.IsNullOrWhiteSpace( json ) )
			return;

		HistoryJson historyJson = JsonUtility.FromJson<HistoryJson>( json );

		if( historyJson == null )
		{
			Debug.LogError( $"Corrupted ActionOracle History Cache, Cleaned: \n{JsonFilePath}" );

			return;
		}

		historyJson.ExportToDictionary( ref history );

		hasUnsavedModifications = false;
	}

	private static async UniTask SaveJson()
	{
		HistoryJson historyJson = new HistoryJson();

		historyJson.ImportFromDictionary( in history );

		hasUnsavedModifications = false;

		await UniTask.SwitchToTaskPool();

		if( File.Exists( JsonFilePath ) )
		{
			try
			{
				File.Delete( JsonFilePath );
			}
			catch( System.Exception e )
			{
				Debug.LogError( $"Failed to save ActionOracle History Cache, could not clean up existing file on disk!\nPath: {JsonFilePath}\nError: {e.Message}" );
				return;
			}
		}

		string json = JsonUtility.ToJson( historyJson, true );

		File.WriteAllText( JsonFilePath, json );
	}

	private static async UniTaskVoid SaveWhenDirtyTask()
	{
		while( true )
		{
			await UniTask.Delay( SaveDelayMS );

			if( hasUnsavedModifications )
				await SaveJson();
		}
	}

	public static void RecordActionProgression( Action first, Action second, Action? third = null )
	{
		if( third.HasValue && second == third && first == second )
			return; // Prevent loops
		
		if( !history.ContainsKey( first ) )
			history[ first ] = new Dictionary<Action, Dictionary<Action, int>>();

		if( !history[ first ].ContainsKey( second ) )
			history[ first ][ second ] = new Dictionary<Action, int>();

		if( third.HasValue )
		{
			if( history[ first ][ second ].ContainsKey( third.Value ) )
				history[ first ][ second ][ third.Value ]++;
			else
				history[ first ][ second ][ third.Value ] = 1;

			RecordActionProgression( second, third.Value );
		}
	}

	public static Action? GetPotentialAction( Action first, Action? second = null )
	{
		if( history.ContainsKey( first ) )
		{
			if( second.HasValue )
			{
				if( !history[ first ].ContainsKey( second.Value ) || history[ first ][ second.Value ].Count == 0 )
				{
					return GetPotentialAction( second.Value );
				}
				else
				{
					var potentialActions = history[ first ][ second.Value ];

					Action bestAction = Action.Idle;
					int highestWeight = 0;

					foreach( var potentialAction in potentialActions )
					{
						if( potentialAction.Value >= highestWeight )
						{
							bestAction = potentialAction.Key;
							highestWeight = potentialAction.Value;
						}
					}

					return bestAction;
				}
			}
			else
			{
				var potentialActions = history[ first ].Keys;
				var enumarator = potentialActions.GetEnumerator();

				if( potentialActions.Count > 1 )
				{
					for( int i = 0, iC = Random.Range( 0, potentialActions.Count ); i < iC; i++ )
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
