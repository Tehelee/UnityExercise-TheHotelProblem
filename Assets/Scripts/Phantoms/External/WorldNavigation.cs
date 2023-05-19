public static class WorldNavigation
{
	public static Location GetNearestActionPoint( WorldPoint location, float maxDistance, Action action ) => default;

	public static Location[] GetNearestActionPoints( WorldPoint location, float maxDistance, params Action[] actions ) => default;

	public static Heading? GetNextStepTowardsPoint( WorldPoint current, WorldPoint destination ) => default;
}
