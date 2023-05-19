# Unity Exercise - The Hotel Problem

Below is "The Hotel Problem", a metaphor I've used to describe this problem to non-engineers as I worked through it myself.

Let's say you're given the chance to win the hotel you're staying in, but all you have to do is figure out how many rooms it has.
Now I should mention, it's a rather weird hotel.

Each room is on a separate floor, and due to a flagrant disregard of fire code, there's only a single elevator to get to them all.
Inside this elevator, there are only four buttons: one to go up one floor, one to go down one floor, one with a cat symbol, and one with a dog.

Every time you arrive on a new floor, either the cat symbol or dog symbol will light up.
This designated if it's a cat themed room or dog themed.
By pressing the other button, you can change the theme of the room you're at.

For clarificiation, there are no floor numbers, nor any windows you can see.
There is only you, the elevator, and the four buttons, and the elevator doors won't open to let you out until you make your guess.

_Good luck!_


## Original Engineering Instructions

___Container.cs___ is a class with the looped double linked list of random bool values (see implementation below).
It supplies an API to get or set the value of the current element, or move forward or backward in the list.

### Task 1:

Write an algorithm in C# to find the number of nodes in the list.
After the search, all values should be in their original state.
Don’t modify the Container class or use reflection; find an algorithmic way to do it using the API supplied by Container.
Be aware of performance and memory allocation. Please comment your code extensively.

### Task 2:

In Unity, create an Editor Window that displays an instance of the Container class as an infinitely scrollable list of true/false values.
Include a button in the window that creates a new instance of Container with random node count to serve as the data source for the scrollable list.
The visualization should be robust to resizing the window. It’s fine to turn in just the code; we don’t need the whole Unity project.
