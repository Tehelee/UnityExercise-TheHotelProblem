using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ContainerUtils
{
	// The number of times we should check for a pattern before we assume it's endless.
	private const int MinimumReoccuringMatches = 3;
	
	// The minimum number of entries to read before we can reasonably make a decision on a potentially single-length container.
	private const int MinimumUnpackedToMatch = 100;

	// Count()

	// This function determines the count of a container that could contain:
	// 1. A singular entry
	// 2. Multiple entries of identical values
	// 3. Multiples entries up to int.MaxValue ( theoretically )
	
	// The main function of this method is to handle the first two cases,
	// while the meat of most evaluations will happen in CountForPatternSize()
	// It accomplishes this by counting twice, but for the second the current value is inverted, to impart a difference in the results.
	// Once the second count is complete, the modified current value is restored, and the larger of the two counts is returned.

	public static int Count( this Container container )
	{
		// Perform an initial count on the container
		int firstCount = container.CountForPatternSize( 0 );

		// If a container is all one value, or contains a repeating dividable pattern,
		// this can be detected by flipping the current value, recounting, and then taking the maximum of the two.

		container.Value = !container.Value;

		int secondCount = container.CountForPatternSize( 0 );

		container.Value = !container.Value;

		return Mathf.Max( firstCount, secondCount );
	}

	// CountForPatternSize()

	// This function determines the count by reading from the container until a pattern of bools is identified
	// Once the pattern has been found, it is re-evaluated by reading more and detecting repetitions of said pattern
	// Once the repetitions are verified, and enough booleans have been read from the container, the size of the pattern is assumed to be the count.
	// This function, called with a pattern size of 0, invokes itself when it identifies a pattern
	// It passes the size of said pattern to the nested call as patternSize.

	private static int CountForPatternSize( this Container container, int patternSize )
	{
		// Indexable list Of values read from the container
		List<bool> unpackedValues = new List<bool>();

		// Method to scan container's current node forward and back based on the amount read into unpackedValues
		void scanContainer( bool forward )
		{
			for( int i = 0, iC = unpackedValues.Count; i < iC; i++ )
			{
				if( forward )
				{
					container.MoveForward();
				}
				else
				{
					container.MoveBackward();
				}
			}
		}

		// The while check ends at max integer in case the container does in-fact contain int.MaxValue entries.
		while( unpackedValues.Count < int.MaxValue )
		{
			// Read the current value from the container into the unpackedValues list, then move the container forward
			unpackedValues.Add( container.Value );
			container.MoveForward();
			
			// Cache the current number of unpacked entries
			int unpackedCount = unpackedValues.Count;

			// If the current unpacked entries total to a number divisible by two, we can compare each half
			if( unpackedCount % 2 == 0 )
			{
				// If it's the root run, we're still looking for a pattern
				bool isSearchingForPattern = patternSize == 0;

				// If it's not a root run, we should check if the currently unpacked count is a factor,
				// we do this by checking if the following are all true:

				// 1. The unpacked count is more than the potential pattern size
				// 2. The unpacked count is a factor of the potential pattern size
				// 3. The unpacked count is an evenly divisible factor of the potential pattern size

				// These are included in the if statement as-is to ensure they're not evaluated if the prior condition fails out and save on processing time
				// This could have been written alternatively as separate variables, but that alternative would have needed additional logic and memory to lay out for the same result.

				if( isSearchingForPattern || ( unpackedCount > patternSize && unpackedCount % patternSize == 0 && ( unpackedCount / patternSize ) % 2 == 0 ) )
				{
					// We use the following for loop below to "read" half of the unpacked values into one integer, and half into the other.

					int firstHalfHash = 0;
					int secondHalfHash = 0;

					int halfUnpackCount = Mathf.FloorToInt( unpackedCount / 2f );

					for( int i = 0; i < unpackedCount; i++ )
					{
						// For each entry, we add or subtract the index from a single number based on the value of the entry to perform a rudimentary hash

						int valueToAssign = unpackedValues[ i ] ? -1 : 1;

						if( i < halfUnpackCount )
						{
							// Because we're multiplying -1 or 1 by the index, we actually increment the index by one to ensure we don't dismiss the initial 0 index value
							firstHalfHash += valueToAssign * ( i + 1 );
						}
						else
						{
							// For the second half, we have to subtract half the total indecies from the current index, and add 1, or subtract one less than half the total.
							secondHalfHash += valueToAssign * ( i - ( halfUnpackCount - 1 ) );
						}
					}

					// If the two half's hashes are equal, we've found a pattern
					if( firstHalfHash == secondHalfHash )
					{
						if( isSearchingForPattern )
						{
							// Since we've found a potential pattern to match against, we're going to rewind the container back to the same node we started with.
							scanContainer( false );

							// Then we'll re-run a count evaluation with a designated pattern size.
							int recount = container.CountForPatternSize( halfUnpackCount );

							// If the recount found a result, and it's a factor of the pattern size we're checking for,
							// we found a valid match and we'll return that as the actual count from this search.
							if( recount != 0 )
							{
								return recount;
							}

							// If it's not a valid result, we un-rewind the container back to the node we left on beforehand.
							scanContainer( true );
						}
						else
						{
							// Arriving here means we're seeing a pattern, but in a potentially infinite list, we can only assume, we can't know for sure.
							// This means we need to dictate a cut-off, or a reasonable best-guess, at some point that's reliable but not excessively intensive.
							// Because this is somewhat subjective, the thresholds to meet are const values that can be modified to fit your project's performance needs.

							// This checks if we've seen the same pattern enough times to assume we won't see any deviations after.
							// This applies mainly towards large containers, as it can err on the side of true if the pattern size ( potential container count ) is small.
							bool hasMinimumReoccuringMatches = unpackedCount / patternSize >= MinimumReoccuringMatches;
							
							// This checks we've read enough entries at a minimum to assume we won't see any deviations either.
							// This complements the previous check and mainly targets small containers, this effectively drives a minimum pattern size.
							bool hasMinimumUnpackedToMatch = unpackedCount >= MinimumUnpackedToMatch;
							
							if( hasMinimumReoccuringMatches && hasMinimumUnpackedToMatch )
							{
								scanContainer( false );

								return patternSize;
							}
						}
					}
					else if( !isSearchingForPattern )
					{
						// If we're not searching, but rather evaluating, and we didn't match, the potential pattern we're checking is invalid.

						scanContainer( false );

						return 0;
					}
				}
			}
		}

		// Not that any reasonable computer would get here any time soon, theoretically under the notes of the task this check needs to be made.
		// If we've unpacked the maximum number of entries practically available to us, and there's still no match, that means our container must have int.MaxValue entries.

		return unpackedValues.Count == int.MaxValue ? int.MaxValue : 0;
	}

	[MenuItem( "PlayQ/Run Container Count Tests" )]
	public static void TestContainerCounts( MenuCommand menuCommand )
	{
		int testCount = 100;
		int failCount = 0;

		for( int i = 0; i < testCount; i++ )
		{
			int containerSize = i >= 10 ? Random.Range( 1, 9999 ) : i + 1;

			EditorUtility.DisplayProgressBar( "Testing Container Counts", $"Container ( {i + 1} / {testCount} )", i / ( float ) testCount );

			Container container = new Container( containerSize );
			
			int containerCount = container.Count();

			if( containerCount != containerSize )
			{
				System.Text.StringBuilder printout = new System.Text.StringBuilder();
				for( int k = 0; k < containerSize; k++ )
				{
					printout.Append( container.Value ? "1" : "0" );
					container.MoveForward();
				}

				failCount++;

				Debug.LogWarning( $"Container Count Failed! ( #{failCount}: {containerCount} != {containerSize} )\n{printout}" );
			}
		}

		EditorUtility.ClearProgressBar();

		if( failCount > 0 )
		{
			Debug.LogWarning( $"Container Count Test Had {failCount} Failures Out Of {testCount} Tests." );
		}
		else
		{
			Debug.Log( $"All {testCount} Tests Resulted In Correct Container Counts." );
		}
	}
}
