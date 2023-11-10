\ Include all basic SDL2 functionality. SDL_image helps with loading images.
require SDL2/SDL.fs
require SDL2/SDL_image.fs
require SDL2/SDL_ttf.fs
require random.fs

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

\ Set constants for creating the SDL Window.
s" 05 Text" >c-str CONSTANT WINDOW_TITLE
800 CONSTANT SCREEN_WIDTH
600 CONSTANT SCREEN_HEIGHT
\ Flags for SDL_image
IMG_INIT_PNG CONSTANT img-flags
80 CONSTANT TEXT_SIZE

\ Pointers for SDL window, renderer and other variables. 
VARIABLE window
VARIABLE renderer
CREATE event SDL_Event ALLOT
VARIABLE background
VARIABLE text-font
CREATE text-color SDL_Color ALLOT
VARIABLE text-image
CREATE text-rect SDL_Rect ALLOT
VARIABLE text-xvel 2 text-xvel !
VARIABLE text-yvel 2 text-yvel !

\ Seed the random generator, throw away the first number.
utime DROP seed ! rnd DROP

\ Release allocated memory for pointers and shutdown SDL correctly.
: game-cleanup ( -- )
    text-font @ TTF_CloseFont
    text-image @ SDL_DestroyTexture
    background @ SDL_DestroyTexture
    renderer @ SDL_DestroyRenderer
    window @ SDL_DestroyWindow
    TTF_Quit
    IMG_Quit
    SDL_Quit
    BYE
;

: initialize-sdl ( -- )
    \ initialize SDL2. 0 is returned on success.
    SDL_INIT_EVERYTHING SDL_Init IF
        ." Error initializing SDL: " SDL_GetError c-str> TYPE CR
        game-cleanup
    THEN

    \ flags are bitwise encoded. IMG_Init returns bitwise answer. Using flags to mask answer then compare to check.
    img-flags IMG_Init img-flags AND img-flags <> IF
        ." Error initializing SDL_image: " IMG_GetError c-str> TYPE CR
        game-cleanup
    THEN

    \ TTF_Init returns 0 on success.
    TTF_Init IF
        ." Error initializing SDL_ttf: " TTF_GetError c-str> TYPE CR
        game-cleanup
    THEN
;

\ Create the SDL2 Window and store the pointer in window. NULL/0 is returned if failed.
: create-window ( -- )
    WINDOW_TITLE SDL_WINDOWPOS_CENTERED SDL_WINDOWPOS_CENTERED SCREEN_WIDTH SCREEN_HEIGHT 0
    SDL_CreateWindow window !
    window @ 0= IF 
        ." Error creating  window: " SDL_GetError c-str> TYPE CR
        game-cleanup
    THEN
;

\ Create the SDL Renderer and store the pointer in renderer. NULL/0 is returned if failed.
: create-renderer ( -- )
    window @ -1 0 SDL_CreateRenderer renderer !
    renderer @ 0= IF
        ." Failed to create renderer: " SDL_GetError c-str> TYPE CR
        game-cleanup
    THEN
;

: load-media ( -- )
    \ load an image directly to a hardware texture. NULL/0 is returned if failed.
    renderer @ S" images/background.png" >c-str IMG_LoadTexture background !
    background @ 0= IF
        ." Error creating texuture from file: " SDL_GeTerror c-str> TYPE CR
        game-cleanup
    THEN
;

: create-text ( -- )
    S" fonts/freesansbold.ttf" >c-str TEXT_SIZE TTF_OpenFont text-font !
    text-font @ 0= IF
        ." Error creating font: " TTF_GetError c-str> TYPE CR
        game-cleanup
    THEN

    text-color
    255 OVER SDL_Color-r C!
    255 OVER SDL_Color-g C!
    255 OVER SDL_Color-b C!
    255 SWAP SDL_Color-a C!

    text-font @ S" Forth" >c-str text-color TTF_RenderText_Blended DUP 0= IF
        ." Error creating font surface: " SDL_GetError c-str> TYPE CR
        DROP game-cleanup
    ELSE
        text-rect
        OVER SDL_Surface-w int32<@ OVER SDL_Rect-w int32>!
        OVER SDL_Surface-h int32<@ SWAP SDL_Rect-h int32>!
        renderer @ OVER SDL_CreateTextureFromSurface text-image !
        SDL_FreeSurface
        text-image @ 0= IF
            ." Error creating texuture from file: " SDL_GeTerror c-str> TYPE CR
            game-cleanup
        THEN
    THEN
;

: text-update ( -- )
    text-rect DUP SDL_Rect-x int32<@ text-xvel @ +
    DUP 0 < IF
        DROP 0 SWAP SDL_Rect-x int32>!
        text-xvel DUP @ NEGATE SWAP !
    ELSE
        DUP SCREEN_WIDTH 3 PICK SDL_Rect-w int32<@ - > IF
            DROP SCREEN_WIDTH OVER SDL_Rect-w int32<@ - SWAP SDL_Rect-x int32>!
            text-xvel DUP @ NEGATE SWAP !
        ELSE
            SWAP SDL_Rect-x int32>!
        THEN
    THEN

    text-rect DUP SDL_Rect-y int32<@ text-yvel @ +
    DUP 0 < IF
        DROP 0 SWAP SDL_Rect-y int32>!
        text-yvel DUP @ NEGATE SWAP !
    ELSE
        DUP SCREEN_HEIGHT 3 PICK SDL_Rect-h int32<@ - > IF
            DROP SCREEN_HEIGHT OVER SDL_Rect-h int32<@ - SWAP SDL_Rect-y int32>!
            text-yvel DUP @ NEGATE SWAP !
        ELSE
            SWAP SDL_Rect-y int32>!
        THEN
    THEN
;

\ set a random number from 0 to 255 for red, green, blue and set alpha to 255 for the renderer.
: random-color-renderer ( -- )
    renderer @ 256 random 256 random 256 random 255 SDL_SetRenderDrawColor DROP
;

: game-loop ( -- )
    \ An infinate loop.
    BEGIN
        \ Loop through all the SDL events since last game loop.
        BEGIN event SDL_PollEvent WHILE
            \ Check event type, SDL_QUIT or SDL_KEYDOWN.
            event SDL_Event-type L@
            DUP SDL_QUIT_ENUM = IF
                DROP game-cleanup
            THEN
            \ Check which key has been pressed.
            SDL_KEYDOWN = IF event SDL_KeyboardEvent-keysym L@
                DUP SDL_SCANCODE_ESCAPE = IF
                    DROP game-cleanup
                THEN
                SDL_SCANCODE_SPACE = IF
                    random-color-renderer
                THEN
            THEN
        REPEAT

        text-update
        
        \ Clears the back screen buffer.
        renderer @ SDL_RenderClear DROP
        
        \ SDL_RenderCopy takes a source and destination rect. NULL will use entire space.
        renderer @ background @ NULL NULL SDL_RenderCopy DROP

        renderer @ text-image @ NULL text-rect SDL_RenderCopy DROP

        \ Flips the front and back buffers, displays what has been drawn. 
        renderer @ SDL_RenderPresent

        \ 16ms delay in a loop is about 60 FPS.
        16 SDL_Delay

    FALSE UNTIL
;

: play-game ( -- )
    initialize-sdl
    create-window
    create-renderer
    load-media
    create-text
    game-loop
;

play-game
