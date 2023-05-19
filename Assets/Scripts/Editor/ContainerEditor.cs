using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class ContainerEditor : EditorWindow
{
	private const float WindowBorder = 10f;
	private const float ButtonHeight = 20f;
	private const float RowHeight = 20f;

	private const float MinWidth = 160f;
	private const float MinHeight = WindowBorder * 3f + ButtonHeight + RowHeight * 3f;

	private const float ScrollbarWidth = 13f;

	private static readonly Color ScrollviewColor = new Color( 1f, 1f, 1f, 0.1f );
	private static readonly Color AlternatingRowColor = new Color( 0f, 0f, 0f, 0.1f );
	
	private Container container;
	private int containerCount;

	private Vector2 scrollPosition;

	[MenuItem( "PlayQ/Open Container Editor" )]
	private static void Init()
	{
		ContainerEditor window = EditorWindow.GetWindow<ContainerEditor>( true, "PlayQ Container Editor", true );
		window.minSize = new Vector2( MinWidth, MinHeight );
		window.Show();
	}

	private void OnEnable()
	{
		scrollPosition = Vector2.zero;
		RegenerateContainer();
	}

	private void OnDisable()
	{
		container = null;
	}

	private void RegenerateContainer()
	{
		container = new Container();
		containerCount = container.Count();

		scrollPosition.y = containerCount * RowHeight;
	}

	private void OnGUI()
	{
		// Allocate GUILayout Space
		EditorGUILayout.GetControlRect( GUILayout.Width( position.width ), GUILayout.Height( position.height ) );

		Rect window = new Rect
		(
			WindowBorder,
			WindowBorder,
			position.width - WindowBorder * 2f,
			position.height - WindowBorder * 2f
		);

		void ConsumeWindowHeight( float height )
		{
			height += WindowBorder;

			window.y += height;
			window.height -= height;
		}
		
		Rect regenerateRect = new Rect( window.x, window.y, window.width, ButtonHeight );

		if( GUI.Button( new Rect( window.x, window.y, window.width, ButtonHeight ), new GUIContent( "Regenerate Container" ) ) )
		{
			RegenerateContainer();
		}

		ConsumeWindowHeight( ButtonHeight );

		EditorGUI.DrawRect( window, ScrollviewColor );

		Rect scrollRect = new Rect( 0, 0, window.width - ScrollbarWidth, containerCount * RowHeight * 3f );
		
		scrollPosition = GUI.BeginScrollView( window, scrollPosition, scrollRect, false, true );

		int startIndex = Mathf.FloorToInt( scrollPosition.y / RowHeight );
		int endIndex = Mathf.FloorToInt( ( scrollPosition.y + window.height ) / RowHeight );

		for( int i = 0; i < startIndex; i++ )
		{
			container.MoveForward();
		}

		Rect rowRect = new Rect( 0f, startIndex * RowHeight, scrollRect.width, RowHeight );

		for( int i = startIndex; i <= endIndex; i++ )
		{
			if( i % 2 == 0 )
			{
				EditorGUI.DrawRect( rowRect, AlternatingRowColor );
			}

			EditorGUI.LabelField( new Rect( 2f, rowRect.y, rowRect.width - 2f, RowHeight ), new GUIContent( $"{i % containerCount}" ) );

			bool value = container.Value;

			bool newValue = EditorGUI.Toggle( new Rect( rowRect.width - RowHeight, rowRect.y, RowHeight, RowHeight ), value );

			if( newValue != value )
			{
				container.Value = newValue;

				EditorGUI.FocusTextInControl( string.Empty );
			}

			rowRect.y += RowHeight;

			container.MoveForward();
		}

		for( int i = 0; i <= endIndex; i++ )
		{
			container.MoveBackward();
		}

		float scrollPositionNormalized = scrollPosition.y / scrollRect.height;

		if( scrollPositionNormalized < 0.166f )
		{
			scrollPosition.y += RowHeight * containerCount * 2f;
		}

		if( scrollPositionNormalized > 0.833f )
		{
			scrollPosition.y -= RowHeight * containerCount * 2f;
		}

		GUI.EndScrollView();
	}
}
