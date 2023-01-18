VAR trackableVariable = 0

===function IsInteractable(b)
{b:
INTERACTABLE(true)
-else:
INTERACTABLE(false)
}


==start
PLAYER(left, nohat) This is on the left, also I have a portrait sans hat.

PLAYER(right, hat) And this is on the right. I have a hat portrait.

~trackableVariable++
This is probably also on the right. Also I just increased a variable +1! It is now {trackableVariable}!

PLAYER(left, hat) Then this is on the left again. I added a hat.

+(option1) [This is an option that is always here.]
PLAYER(left, nohat) Great. Hat off!

PLAYER(right, nohat) Good idea!

+(option2) [{IsInteractable(RANDOM(0,1)>0)}This is an option that is sometimes disabled.]
PLAYER(left, nohat) Lucky us. Off with the hat.

PLAYER(right, nohat) Yass.

- 
->end
==end
The end!
->END

==continue
Hi there. This is set in the other scene, just to show how easily variables and things carry over.

In the previous scene, we increased trackableVariable to {trackableVariable}. Nice.

In the previous scene, we picked option 1 {start.option1} times and option 2 {start.option2} times. Neat!

Welp, that was that. Let's load the other scene back.

+ [Load me back, Scotty! LOAD_SCENE(SampleScene)]
[This text won't be visible.]
+ [Don't load me back.]
Okely, fine by me.
- 
->END

==exampleStringTable
PLAYER_BARK() This is a player bark.

OTHER_BARK() Example of another bark.

PLAYER_BARK() More player bark.

PLAYER_BARK() Even more player bark.

OTHER_BARK() And other bark.

PLAYER_BARK(good) This is an example of a bark with a variable - good.

PLAYER_BARK(good) Which could for example be used as a unit.

PLAYER_BARK(bad) Another example of an argument bark.

PLAYER_BARK(bad) These will also show up though in the generic list of player barks.
->END