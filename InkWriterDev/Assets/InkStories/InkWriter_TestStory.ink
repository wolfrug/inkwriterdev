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
#clearText
Welcome to Wolfrug's Ink writer dev scene!

This is my personal project to make Ink useable inside Unity, with a bunch of functions and functionalities I've found especially handy. Most of these exist inside the Demo folder - this is merely the most base implementation.

At the center of it all is the InkDialogueLine. This is a structure that picks up text from any ink line, and interprets it as a kind of function call. For example you might write MOVE_TO_ROOM(thisroom, fast), which could then be interpreted and picked up by any other script.

The goal is to let you control things directly using Ink, and very swiftly add new functions in as needed. It also works with Ink tags, although the weakness with tags is they cannot convey information (such as variables).

Choices also have their own InkChoiceLine which works in nearly the same way, and lets you run 'functions' when clicking the option buttons (or when displaying them).

The InkVariables that are looked for are set up in Scriptable Objects, that also contain most of the important code, meaning as long as you have access to them, you can do most of the things this framework is meant to do.

+ [I'd like to know more.]
#clearText
First of all, I used an ink tag here to clear the text. For these things I am using the "Ink Listener" object, which is hooked up to this writer. I have also hooked it up to listen to the variable \PLAYER\, with the arguments "red" and "green". Once it hears those, it will activate either a red or green ball.
->redGreenExample
+ [Close]
PLAYER(end)
->END


=redGreenExample
+ [Activate Red]
PLAYER(red)
+ [Activate Green]
PLAYER(green)
+ [Close]
PLAYER(end)
->END
- 
->redGreenExample


==startAdvanced
PLAYER(left, nohat) This is on the left, also I have a portrait sans hat.

PLAYER(right, hat) And this is on the right. I have a hat portrait.

~trackableVariable++
This is probably also on the right. Also I just increased a variable +1! It is now {trackableVariable}!

PLAYER(left, hat) Then this is on the left again. I added a hat, and also changed the text to a blue one. SET_TEXTBOX(bluetext)

+(option1) [This is an option that is always here.]
PLAYER(left, nohat) Great. Hat off!

PLAYER(right, nohat) Good idea!

+(option2) [{IsInteractable(RANDOM(0,1)>0)}This is an option that is sometimes disabled.]
PLAYER(left, nohat) Lucky us. Off with the hat.

PLAYER(right, nohat) Yass.
+ [SET_BUTTON_GRAPHIC(left, plus) This button has a plus option.]
Yay! Well done!
+ [SET_BUTTON_GRAPHIC(right, warning)SET_BUTTON_GRAPHIC(left, plus) This button has a warning! SET_BUTTON_TEXT(right, Danger!)]
Oh no, why did you click it!
+ [SET_OPTIONBOX(redbutton) And this button is red!!]
Wow, so cool.
- 
->end
==end
The end!
->END

==extraText
This is some extra text that we're showing on the side, demonstrating both flow and how you can use the base inkwriter with no bells and whistles.

This will just display the text as one, with no additional thought put into it.

+ [Options supported!]
Options are also supported, obviously.
++ [Back!]
->extraText

==continue
Hi there. This is set in the other scene, just to show how easily variables and things carry over.

In the previous scene, we increased trackableVariable to {trackableVariable}. Nice.

In the previous scene, we picked option 1 {startAdvanced.option1} times and option 2 {startAdvanced.option2} times. Neat!

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

==exampleInkText
This is an example of an Ink Text object, which can be quite powerful and useful. Just add it to any textmeshprougui object and hook it up to a knot.

This one also shows the use of the storyoverlay, which listens to events in this text and changes images based on them. In this case, randomly switching between two possible corner images.

{~SET_STORY_IMAGE(corner, dude)|SET_STORY_IMAGE(corner, mail)}
SET_STORY_TEXT(title, Visit Count: {exampleInkText})
->END
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