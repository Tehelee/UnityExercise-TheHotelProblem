public struct PlayerState
{
	public PlayerState( Action action, Location location )
	{
		this.action = action;
		this.location = location;
	}

	public Action action;
	public Location location;

	public override bool Equals(object obj)
	{
		if( obj == null || obj.GetType() != typeof( PlayerState ) )
			return false;

		return Equals( ( PlayerState ) obj );
	}

	public bool Equals( PlayerState other ) =>
		action == other.action &&
		location == other.location;

	public static bool operator ==( PlayerState a, PlayerState b ) => a.Equals( b );

	public static bool operator !=( PlayerState a, PlayerState b ) => !a.Equals( b );

	public override int GetHashCode() =>
		17 * ( 31 + action.GetHashCode() ) * ( 31 + location.GetHashCode() );

	public override string ToString() =>
		$"{action} @ {location}";
}