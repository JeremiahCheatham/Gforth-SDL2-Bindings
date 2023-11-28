\ ----===< prefix >===-----
c-library sdl_messagebox
s" SDL2" add-lib
\c #include <SDL2/SDL_messagebox.h>

\ --------===< enums >===---------
#16	constant SDL_MESSAGEBOX_ERROR
#32	constant SDL_MESSAGEBOX_WARNING
#64	constant SDL_MESSAGEBOX_INFORMATION
#128	constant SDL_MESSAGEBOX_BUTTONS_LEFT_TO_RIGHT
#256	constant SDL_MESSAGEBOX_BUTTONS_RIGHT_TO_LEFT
#1	constant SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT
#2	constant SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT
#0	constant SDL_MESSAGEBOX_COLOR_BACKGROUND
#1	constant SDL_MESSAGEBOX_COLOR_TEXT
#2	constant SDL_MESSAGEBOX_COLOR_BUTTON_BORDER
#3	constant SDL_MESSAGEBOX_COLOR_BUTTON_BACKGROUND
#4	constant SDL_MESSAGEBOX_COLOR_BUTTON_SELECTED
#5	constant SDL_MESSAGEBOX_COLOR_MAX

\ -------===< structs >===--------

\ SDL_MessageBoxButtonData
begin-structure SDL_MessageBoxButtonData
	c-uint32:   SDL_MessageBoxButtonData-flags
	c-int:      SDL_MessageBoxButtonData-buttonid
	c-char-ptr: SDL_MessageBoxButtonData-text
end-structure

\ SDL_MessageBoxColor
begin-structure SDL_MessageBoxColor
	c-uint8:    SDL_MessageBoxColor-r
	c-uint8:    SDL_MessageBoxColor-g
	c-uint8:    SDL_MessageBoxColor-b
end-structure

\ SDL_MessageBoxColorScheme
begin-structure SDL_MessageBoxColorScheme
	15 bytes: SDL_MessageBoxColorScheme-colors
end-structure

\ SDL_MessageBoxData
begin-structure SDL_MessageBoxData
	drop 0 4 +field SDL_MessageBoxData-flags
	drop 8 8 +field SDL_MessageBoxData-window
	drop 16 8 +field SDL_MessageBoxData-title
	drop 24 8 +field SDL_MessageBoxData-message
	drop 32 4 +field SDL_MessageBoxData-numbuttons
	drop 40 8 +field SDL_MessageBoxData-buttons
	drop 48 8 +field SDL_MessageBoxData-colorScheme
end-structure

\ ------===< functions >===-------
c-function SDL_ShowMessageBox SDL_ShowMessageBox a a -- n	( messageboxdata buttonid -- )
c-function SDL_ShowSimpleMessageBox SDL_ShowSimpleMessageBox n a a a -- n	( flags title message window -- )

\ ----===< postfix >===-----
end-c-library
