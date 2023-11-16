# SDL2 Bindings for Gforth
A complete set of bindings for `SDL2`, `SDL_image`, `SDL_mixer`, and `SDL_ttf`. Note that these are simply the SDL2 Gforth bindings. SDL2 with headerfiles needs to be installed and added to your library and include paths. For example on ArchLinux simply installing sdl2, sdl2_image, sdl2_mixer, sdl2_ttf packages are all you need.

## SDL Event Enum name collisions.
Because Forth is not case-sensitive, there are some naming collisions with the Event Enums. For example, the function `SDL_Quit` and the Event Enum `SDL_QUIT` are viewed as the same. For the 6 Event Enums, _ENUM has been added to their names. The `SDL_Quit` function will still close SDL, but for checking if an `SDL_QUIT` event has occurred, `SDL_QUIT_ENUM` is used.

    SDL_QUIT_ENUM
    SDL_DISPLAYEVENT_ENUM
    SDL_WINDOWEVENT_ENUM
    SDL_SYSWMEVENT_ENUM
    SDL_SENSORUPDATE_ENUM
    SDL_USEREVENT_ENUM

## SDL_Color struct in SDL_ttf fuctions.
Many functions in `SDL_ttf` take an `SDL_Color` struct directly instead of a pointer to a struct. Pointers to C structs are easily passed to functions in Forth but not structs directly. There are wrapper functions so that a pointer to a struct can be passed instead. The bindings use the original `SDL_ttf` function names that point to the wrapper functions so the only change is `SDL_Color` structs are passed as pointers like all others.

## Take care storing and retrieving from SDL structs and unions.
Gforth stores numbers on a 64bit system as a 64bit two's compliment signed int. However, SDL uses different data types in their structs and unions. When storing into a struct, you must make sure to store the right number of bytes or it will overwrite the next memory blocks. However, simply using `L@` to write to 4 bytes instead of 8 will only solve part of the problem. `@L` will not cast the number but simply truncate it.

    CREATE test SDL_Rect ALLOT  ok
    -1 test SDL_Rect-x L!  ok
    test SDL_Rect-x L@ . 4294967295  ok

You will need to think about the size of the data you are writing and also the type. specifically for 32bit signed int in Rects i have will use a `int32<@` and `int32>!` that will be shown below. For 1 byte events simply `C@` will work.

## Accessing members of C Structs and Unions
https://wiki.libsdl.org/SDL2/SDL_Rect \
SDL uses structs and unions that have members. An example is the `SDL_Rect` struct, which has 4 members: x, y, w, h. Each member of an `SDL_Rect` is of type 32-bit signed int. In C, `SDL_Rect myrect;` creates the variable myrect, and to access x, we would use `myrect.x`. This is just a fancy way of offsetting the address to the position of its member. The bindings in Forth will have an `SDL_Rect` for allocating the size of the struct, but it also has `SDL_Rect-x`, `SDL_Rect-y`, `SDL_Rect-w`, and `SDL_Rect-h` for use as offsets. So in Gforth, `myrect SDL_Rect-x int32<@` will put on the stack the address to the x member. As I said above, you need to constrain the fetch to only 4 bytes and convert it. Remember, all the bindings structs and unions will have the type-member to get the offset. Here is an example of a fetch and store using `int32>!` and `int32<@`.

    // C SDL2 assignment and retrieve
    SDL_Rect myrect;
    myrect.x = 10
    int getx = myrect.x

    \ Forth SDL2 store and fetch
    CREATE myrect SDL_Rect ALLOT
    VARIABLE getx
    10 myrect SDL_Rect-x int32>!
    myrect SDL_Rect-x int32<@ getx !

## C style strings.
Gforth uses a string and length pair but C requires only the string but there must be a `NULL` terminator at the end. For sending, you can simply use `s\” c string\0" DROP` but for converting back, I have also shown below `>c-str` and `c-str>` words to help out.

# How to use SDL2 Bindings.
Using the SDL2 bindings for Gforth is just as it is in C. The function names are exactly the same. All arguments are placed on the stack in the same order as the C version. The return value is placed on the stack afterward. All the SDL2 Constants and Enums are included. NULL is simply 0, below I will create a CONSTANT for convenience. Below is an example.

C version from https://wiki.libsdl.org/SDL2/SDL_CreateWindow

    // SDL_Window * SDL_CreateWindow(const char *title, int x, int y, int w, int h, Uint32 flags);
    SDL_Window *window = SDL_CreateWindow("Window Title", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600, 0);

Gforth version

    S\"Window Title\0" DROP SDL_WINDOWPOS_CENTERED SDL_WINDOWPOS_CENTERED 800 600 0 SDL_CreateWindow TO window

## To help working with C and SDL2
`NULL` is used often it’s simply 0 but I have added a constant. To get strings back from SDL such as `SDL_GetError c-str>` will return a string length pair. To send strings into C functions, `>c-str` will take a string length pair and return a C style `NULL` terminated string. To convert Gforth’s 64bit signed to a 32bit size and store it in 4 bytes, `int32<!` this is used for setting `SDL_Rect` x, y, w, and h. To pull a number out, `int32<@` will put onto the stack a 64bit version.

    \ Helpers for C
    0 CONSTANT NULL
    : c-str> ( c-str -- string u ) 0 BEGIN 2DUP + C@ WHILE 1+ REPEAT ;
    : >c-str ( string u -- c-str )
        1+ DUP ALLOCATE
        DROP ROT OVER 3 PICK 1- MOVE
        DUP ROT 1- + 0 SWAP C!
    ;
    : int32>! ( 64bit signed -- 32bit signed ) DUP 0< IF 0x100000000 + THEN L! ;
    : int32<@ ( 32bit signed -- 64bit signed ) L@ DUP 0x7FFFFFFF > IF 0x100000000 - THEN ;

# Creating a Window
## Importing SDL2 bindings.
Just like in C or C++, to import all the basic SDL2 bindings, we simply need to require `SDL.fs`.

    \ Include all basic SDL2 functionality
    require SDL2/SDL.fs

## C Helpers.
As shown above, we will add in some helpers for working with C.

    \ Helpers for C
    0 CONSTANT NULL
    : c-str> ( c-str -- string u ) 0 BEGIN 2DUP + C@ WHILE 1+ REPEAT ;
    : >c-str ( string u -- c-str )
        1+ DUP ALLOCATE
        DROP ROT OVER 3 PICK 1- MOVE
        DUP ROT 1- + 0 SWAP C!
    ;

## Window and Renderer Pointers
We will need 2 variables that will hold the window and renderer pointers.

    \ Pointers for SDL window and renderer.
    NULL VALUE window
    NULL VALUE renderer

## A Word to Gracefully Cleanup and Exit the Game
https://wiki.libsdl.org/SDL2/SDL_DestroyRenderer \
https://wiki.libsdl.org/SDL2/SDL_DestroyWindow \
https://wiki.libsdl.org/SDL2/SDL_Quit \
SDL allocates memory on the heap that needs to be manually freed. We will make a word to free any memory and properly shut down SDL. We can use the cleanup functions on the pointers even if they are 0/`NULL`, so it is safe to call this at any point after the VARIABLEs have been created as mentioned above.

    \ Release allocated memory for pointers and shutdown SDL correctly.
    : game-cleanup ( -- )
        renderer SDL_DestroyRenderer
        window SDL_DestroyWindow
        SDL_Quit
        BYE
    ;

## Initializing SDL
https://wiki.libsdl.org/SDL2/SDL_Init \
https://wiki.libsdl.org/SDL2/SDL_GetError \
The `SDL_Init` function takes in a FLAG and returns a 0 on success. If it returns anything else, we will give an error message, then get the `SDL_GetError` c-string, convert it, and then print it. Finally it will call the `game-cleanup` word to gracefully shut down.

    : initialize-sdl ( -- )
        \ initialize SDL2. 0 is returned on success.
        SDL_INIT_EVERYTHING SDL_Init IF
            ." Error initializing SDL: " SDL_GetError c-str> TYPE CR
            game-cleanup
        THEN
    ;

## Creating the SDL Window
https://wiki.libsdl.org/SDL2/SDL_CreateWindow \
The `SDL_CreateWindow` function takes a C string, x and y window positions, w and h window sizes, and an optional window flag. An address to the window is placed on the stack, or a 0/`NULL`. We will store that and then check if it’s 0 to shut down.

    \ Create the SDL2 Window and store the pointer in window. NULL/0 is returned if failed.
    : create-window ( -- )
        WINDOW_TITLE SDL_WINDOWPOS_CENTERED SDL_WINDOWPOS_CENTERED SCREEN_WIDTH SCREEN_HEIGHT 0
        SDL_CreateWindow TO window
        window 0= IF 
            ." Error creating  window: " SDL_GetError c-str> TYPE CR
            game-cleanup
        THEN
    ;

## Create the SDL Renderer
https://wiki.libsdl.org/SDL2/SDL_CreateRenderer \
The `SDL_CreateRenderer` takes in the window pointer and an optional flag. Like above it puts the address to the renderer on the stack. We will do the same error checking.

    \ Create the SDL Renderer and store the pointer in renderer. NULL/0 is returned if failed.
    : create-renderer ( -- )
        window -1 0 SDL_CreateRenderer TO renderer
        renderer 0= IF
            ." Failed to create renderer: " SDL_GetError c-str> TYPE CR
            game-cleanup
        THEN
    ;

## The Game Loop
https://wiki.libsdl.org/SDL2/SDL_RenderClear \
https://wiki.libsdl.org/SDL2/SDL_RenderPresent \
https://wiki.libsdl.org/SDL2/SDL_Delay \
The `SDL_RenderClear` clears the renderer or backbuffer. It takes the renderer pointer and returns to the stack a 0 on success. We will just ignore it and DROP it from the stack. When all drawing to the renderer has finished `SDL_RenderPresent` flips the buffers to display what has been drawn. `5000 SDL_Delay` will pause the program for 5 seconds before closing. 

    : game-loop ( -- )
        \ Clears the back screen buffer.
        renderer SDL_RenderClear DROP
    
        \ Do all your drawing here.

        \ Flips the front and back buffers, displays what has been drawn. 
        renderer SDL_RenderPresent

        \ keeps window open for 5 seconds.
        5000 SDL_Delay
    ;

## Play game
To pull together all these words we have created we create `play-game`. This initializes SDL, creates the window, creates the renderer, runs the game loop for 5 seconds then finally cleanup the code. The will open a black window for 5 seconds and close it. It will not respond to closing the window as that is an event that will be added later.

    : play-game ( -- )
        initialize-sdl
        create-window
        create-renderer
        game-loop
        game-cleanup
    ;

    play-game

## Run with Gforth
`01-open-window.fs` contains all the code above. If this is your first time running this project it may take a minute or 2 to compile all the SDL2 bindings.

    gforth 01-open-window.fs

# Closing the Window
Now that we have successfully created a window in Gforth SDL, it would be nice to be able to close it. We will make the game loop actually loop and check for a close event.

## Create and Allocate an SDL_Event union
https://wiki.libsdl.org/SDL2/SDL_Event \
The `SDL_Event` is a union that holds a struct of a single event. We will loop through an array of events since the last game loop. The event struct will hold each even until the array is emptied. We will use the event to check for a close events or key press.

    \ Add after the other variables.
    CREATE event SDL_Event ALLOT

## Creating an infinite game loop
We will indent the previous code in the game loop and then put it inside of a `BEGIN` `FALSE UNTIL` loop. This will loop forever. We also need to change the `SDL_Delay` from 5000(5 seconds) to 16ms this will approximate a loop that runs 60 times a second.

    : game-loop ( -- )
        \ An infinate loop.
        BEGIN
            
            \ Clears the back screen buffer.
            renderer SDL_RenderClear DROP
            
            \ Do all your drawing here.
    
            \ Flips the front and back buffers, displays what has been drawn. 
            renderer SDL_RenderPresent
    
            \ 16ms delay in a loop is about 60 FPS.
            16 SDL_Delay
    
        FALSE UNTIL
    ;

## Polling the Events and checking them.
https://wiki.libsdl.org/SDL2/SDL_PollEvent \
https://wiki.libsdl.org/SDL2/SDL_KeyboardEvent \
`BEGIN event SDL_PollEvent WHILE` to `REPEAT` creates a loop where `SDL_PollEvent` pulls an `SDL_Event` from an internal array and stores it into the `event` VARIABLE. The loop continues until the array is empty. The `event SDL_Event-type L@` pulls the type member of the current event to the stack. This value is compared to the `SDL_QUIT_ENUM` value and `game-cleanup` is called if it’s true. This will allow the program to close if you click the close window or equivalent. Since we duplicated that value, we can compare it to `SDL_KEYDOWN` to see if the event was a key press. To get the actual key, `event SDL_KeyboardEvent-keysym L@` will put the value on the stack and it can be checked against the escape key code. This code is inside the game loop created above and placed right at the very top of the game loop.

    \ Loop through all the SDL events since last game loop.
    BEGIN event SDL_PollEvent WHILE
        \ Check event type, SDL_QUIT or SDL_KEYDOWN.
        event SDL_Event-type L@
        DUP SDL_QUIT_ENUM = IF
            DROP game-cleanup
        THEN
        \ Check which key has been pressed.
        SDL_KEYDOWN = IF event SDL_KeyboardEvent-keysym L@
            SDL_SCANCODE_ESCAPE = IF
                game-cleanup
            THEN
        THEN
    REPEAT

## Remove game-cleanup and run.
Since the game is running an infinate loop until `game-cleanup` is called we no longer need it in the `play-game` word. Remove `game-cleanup` from `play-game` and run it.

    gforth 02-close-window.fs



