===function IsInteractable(b)
{b:
INTERACTABLE(true)
-else:
INTERACTABLE(false)
}


==start
PLAYER(left) This is on the left.

PLAYER(right) And this is on the right.

This is probably also on the right.

PLAYER(left) Then this is on the left again.

+ [Cool story bro.]
PLAYER(left) I mean really lame haha.

PLAYER(right) Lol owned.

+ [{IsInteractable(RANDOM(0,1)>0)}Lame story bro...]
PLAYER(left) Just telling it how I see it bro.

PLAYER(right) Lolol.

- And they gathered everything and lived happily ever after.
->end
==end
The end!
->END