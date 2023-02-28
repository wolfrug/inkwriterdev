VAR trackableVariable = 0
LIST MainInventory = Oval, (Hexagon), Square
VAR Oval_stack = 0

->exampleInventoryUse

===function IsInteractable(b)
{b:
INTERACTABLE(true)
-else:
INTERACTABLE(false)
}

===function alterStack(item, amount)
{item:
    - Oval:
    ~Oval_stack += amount
        {Oval_stack<=0:
            ~Oval_stack = 0
            ~MainInventory-=Oval
        }
    {Oval_stack>10:
        ~Oval_stack = 10
    }
}
{not (MainInventory?item) && amount > 0:
~MainInventory+=item
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
==exampleCustomButtonsStart
->exampleCustomButtons(true)
==exampleCustomButtons(interactable)
Hi there, this is to show how you can rewire existing buttons in the scene to apply to choices in your ink writer, letting you easily map functions in your UI for example to your existing Ink thread!

+ [TEST_USE(one){IsInteractable(interactable)} Number one.]
You clicked the first button! Good work.
+ [TEST_USE(two) Number two.]
You clicked the second button! Nice.
+ [Number three.]
This button wasn't hooked up to anything, cool huh?

- And that's about all she wrote. Let's go again!
+ [Try again.]
->exampleCustomButtons(true)
+ [Try again, but disable one button.]
Okely dokely. The first button will now be disabled.
->exampleCustomButtons(false)

==exampleInventoryUse
Hi there, this is a very simple ink inventory example. To work, it needs a LIST with all the items (their name in the list == ID of the inventory item data). The name of the LIST in turn corresponds to the ID of the inventory.

- (options)
+ {Oval_stack<10} [Add an Oval.]
{alterStack(Oval, 1)}
We have a simple tag set up to tell the inventory to update (using a listener). Also Ovals are stackable, and we have our own function for that! #updateInventory
+ {MainInventory?Oval} [Remove an Oval.]
{alterStack(Oval, -1)}
We have a simple tag set up to tell the inventory to update (using a listener). Also Ovals are stackable, and we have our own function for that! #updateInventory

+ {not (MainInventory?Hexagon)} [Add a Hexagon.]
~MainInventory+=Hexagon
We have a simple tag set up to tell the inventory to update (using a listener). #updateInventory

+ {(MainInventory?Hexagon)} [Remove a Hexagon.]
~MainInventory-=Hexagon
We have a simple tag set up to tell the inventory to update (using a listener). #updateInventory

+ {not (MainInventory?Square)} [Add a Square.]
~MainInventory+=Square
We have a simple tag set up to tell the inventory to update (using a listener). #updateInventory

+ {(MainInventory?Square)} [Remove a Square.]
~MainInventory-=Square
We have a simple tag set up to tell the inventory to update (using a listener). #updateInventory
+ [Try the usable inventory.]
->exampleUseableInventory
- 
->options


==exampleUseableInventory
Let's try to use our inventory items as buttons! Depending on which items we have, they will be available to click now. #showUsableInventory

+ {MainInventory?Square} [USE_ITEM({Square})Click the square.]
#hideUseableInventory
You clicked the square, good work.
+ {MainInventory?Hexagon} [USE_ITEM({Hexagon})Click the hexagon.]
#hideUseableInventory
You clicked the Hexagon. Success.
+ {MainInventory?Oval} [USE_ITEM({Oval})Click the oval.]
{alterStack(Oval, -1)}
#updateInventory #hideUseableInventory
You clicked the oval, and in doing so removed one! Oh no.
+ [Nevermind.]
Fair enough.

- Now you can do whatever you want with this, for example add or remove them, or have someone comment on them, etc.

+ [Back to adding / removing]
#hideUseableInventory
->exampleInventoryUse
+ [Try again.]
->exampleUseableInventory

==exampleStringTable
PLAYER_BARK() This is a player bark.

OTHER_BARK() Example of another bark.

PLAYER_BARK() More player bark.

PLAYER_BARK() Even more player bark.

OTHER_BARK() And other bark.

PLAYER_BARK(good) This is an example of a bark with an argument (good).

PLAYER_BARK(good) Which could for example be used as a unit.

PLAYER_BARK(bad) Another example of an argument bark with the argument bad.

PLAYER_BARK(bad) These will also show up though in the generic list of player barks.
->END