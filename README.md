# SDL2 Bindings for Gforth
A complete set of Bindings for SDL2, SDL_image, SDL_mixer and SDL_ttf.

## SDL Event Enum name collisions.
Because Forth is not case sensitive there are some naming collisons with the Event Enums. For example the funciton SDL_Quit and the Event Enum SDL_QUIT are viewed as the same. For the 6 Event Enums _ENUM has been added to their names. SDL_Quit fuction will still closes SDL, but for checking if an SDL_QUIT event has occured SDL_QUIT_ENUM is used.

    SDL_QUIT_ENUM
    SDL_DISPLAYEVENT_ENUM
    SDL_WINDOWEVENT_ENUM
    SDL_SYSWMEVENT_ENUM
    SDL_SENSORUPDATE_ENUM
    SDL_USEREVENT_ENUM

## SDL_Color struct in SDL_ttf fuctions.
Many functions in SDL_ttf take a SDL_Color struct directly instead of a pointer to a struct. Pointers to C structs are easily passed to functions in Forth but not structs directly. There has been wrapper functions so that a pointer to a struct can be passed instead. The bindings use the original SDL_ttf function names that point to the wrapper functions so the only change is SDL_Color structs are pass as pointers like all other

## Take care storing and retriving from SDL structs and untions.
Gforth stores numbers on a 64bit system as a 64bit two's compliment signed int. However when SDL uses different data types in their structs and unions. When storing into a struct you must make sure to store the write number of bytes or it will overwrite the next memory blocks. However simply using L@ to write to 4 bytes instead of 8 will only solve part of the problem. @L will not cast the number but simply truncate it.

    CREATE test SDL_Rect ALLOT  ok
    -1 test SDL_Rect-x L!  ok
    test SDL_Rect-x L@ . 4294967295  ok

You will need to think about the size of the data you are writing and also the type. specifically for 32bit signed int in Rects i have will use a int32<@ and int32>! that will be shown below. For 1 byte events simply C@ will work.

## C style strings.
Gforth using a string and length pair but C requires only the string but there must be a null terminator at the end. For sending you can simply use s\" c string\0" DROP but for converting back i have also shown below >c-str and c-str> words to help out.

# How to use SDL2 Bindings.
Using the SDL2 bindings for Gforth is just as it is in C. The function names are the exact same. All arguments are placed on the stack in the same order as the C version. The return value is placed on the stack afterwards. All the SDL2 Constants and Enums are included. NULL is simply 0 below i will create a CONSTANT for convinience. Below is an example.

C version from https://wiki.libsdl.org/SDL2/SDL_CreateWindow

    // SDL_Window * SDL_CreateWindow(const char *title, int x, int y, int w, int h, Uint32 flags);
    SDL_Window *window = SDL_CreateWindow("Window Title", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600, 0);

Gforth version

    S\"Window Title\0" DROP SDL_WINDOWPOS_CENTERED SDL_WINDOWPOS_CENTERED 800 600 0 SDL_CreateWindow window !







