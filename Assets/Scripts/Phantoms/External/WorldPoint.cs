public struct WorldPoint
{
	public WorldPoint( int x, int y )
	{
		this.x = x;
		this.y = y;
	}

	public int x;
	public int y;

	public void Translate( Heading heading, int amount )
	{
		switch( heading )
		{
			case Heading.North:
				y += amount;
				break;
			case Heading.South:
				y -= amount;
				break;
			case Heading.East:
				x += amount;
				break;
			case Heading.West:
				x -= amount;
				break;
		}
	}

	public override bool Equals( object obj )
	{
		if( obj == null || obj.GetType() != typeof( WorldPoint ) )
			return false;

		return Equals( ( WorldPoint ) obj );
	}

	public bool Equals( WorldPoint other ) =>
		x == other.x &&
		y == other.y;

	public static bool operator ==( WorldPoint a, WorldPoint b ) => a.Equals( b );

	public static bool operator !=( WorldPoint a, WorldPoint b ) => !a.Equals( b );

	public override int GetHashCode() =>
		17 * ( 31 + x.GetHashCode() ) * ( 31 + y.GetHashCode() );

	public override string ToString() =>
		$"{x}, {y}";
}
