# Unity Exercise - The Hotel Problem

_Below is "The Hotel Problem", a metaphor I've used to describe this problem to non-engineers as I worked through it myself._

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

## Bonus Architecture Task - Phantasms

_In addition to "The Hotel Problem" this engineering exercise also asked me to express my architecture skills with an additional task._

Imagine that we are building a single-player game in which the local player controls their avatar in the foreground in real time. Our Design team has requested that we implement a feature called “Phantasms.” The Phantasm feature requires us to display in the background other AI agents called “phantasms” that have simplified movesets that are a subset of what the local player can do. Even though they are AI agents, the actions of the phantasms should be derived from other human players currently playing the game, with a single phantasm representing a single real human player. The phantasms don’t affect gameplay, and thus do not need to be closely controlled in real time like a true multiplayer game, but we would like them to update their behavior parameters reasonably often as other human players make moves (about every minute or so) and only represent real players currently playing the game. Assume that we have a robust server architecture that the local client can query to learn the current state of other players via a pre-existing middle layer in the client, but that the server tech stack does not support sockets or any other data pushing technology. Also assume that we have animated avatars that can accept commands to perform individual actions from the simplified phantasm moveset.

### Task 3:

Imagine that you are responsible for implementing the Phantasms client feature, in collaboration with a server team. Please describe at a high level how you would design the systems to support this feature, aiming for an architecture that would easily accommodate future feature extension or iteration. Write a set of classes in C# pseudocode that would fulfill your design. Feel free to append diagrams that you feel would help explain the architecture.
