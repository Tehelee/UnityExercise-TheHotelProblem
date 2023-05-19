public struct Location
{
	public Location( WorldPoint point, Heading heading )
	{
		this.point = point;
		this.heading = heading;
	}

	public WorldPoint point;
	public Heading heading;

	public override bool Equals( object obj )
	{
		if( obj == null || obj.GetType() != typeof( Location ) )
			return false;

		return Equals( ( Location ) obj );
	}

	public bool Equals( Location other ) =>
		point == other.point &&
		heading == other.heading;

	public static bool operator ==( Location a, Location b ) => a.Equals( b );

	public static bool operator !=( Location a, Location b ) => !a.Equals( b );

	public override int GetHashCode() =>
		17 * ( 31 + point.GetHashCode() ) * ( 31 + heading.GetHashCode() );

	public override string ToString() =>
		$"{point}, {heading}";
}