using System;

public class Container
{
	private class Node
	{
		public Node Next;
		public Node Prev;
		public bool Value;

		public Node( Node prev )
		{
			// This should really be static / consistent, or constructions of Node() on the same millisecond will result in identical 'random' values due to seeding.
			var randomGen = new Random( DateTime.Now.Millisecond );

			// Original code has error ; in statement.
			//   Value = randomGen.Next( 2 ); < 1;
			// Revised code, sans ;
			Value = randomGen.Next( 2 ) < 1; // Max is exclusive, result is either 0 or 1, 0 being true
			Prev = prev;
		}
	}

	private Node current;

	public Container( int count = 0 )
	{
		if( count < 1 )
		{
			var randomGen = new Random( DateTime.Now.Millisecond );
			count = randomGen.Next( 1, 9999 ); //Could be up to Int32.MaxValue, reduced for sake of test memory
		}

		Node prev = null;
		for( int i = 0; i < count; i++ )
		{
			// create a new node, assigning prev
			var currentNode = new Node( prev );

			// update prev node's next node if prev exists
			if( prev != null )
			{
				prev.Next = currentNode;
			}

			// assign 'first current' value for end looping
			if( current == null )
			{
				current = currentNode;
			}

			// update prev to new node
			prev = currentNode;
		}

		// Plug ends to loop
		prev.Next = current; // last => first
		current.Prev = prev; // first => last
	}

	public bool Value
	{
		get { return current.Value; }
		set { current.Value = value; }
	}

	public void MoveForward()
	{
		current = current.Next;
	}

	public void MoveBackward()
	{
		current = current.Prev;
	}
}