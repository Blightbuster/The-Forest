using System;

namespace ChromaSDK
{
	
	public class Keyboard
	{
		
		public const int MAX_ROW = 6;

		
		public const int MAX_COLUMN = 22;

		
		public enum RZKEY
		{
			
			RZKEY_ESC = 1,
			
			RZKEY_F1 = 3,
			
			RZKEY_F2,
			
			RZKEY_F3,
			
			RZKEY_F4,
			
			RZKEY_F5,
			
			RZKEY_F6,
			
			RZKEY_F7,
			
			RZKEY_F8,
			
			RZKEY_F9,
			
			RZKEY_F10,
			
			RZKEY_F11,
			
			RZKEY_F12,
			
			RZKEY_1 = 258,
			
			RZKEY_2,
			
			RZKEY_3,
			
			RZKEY_4,
			
			RZKEY_5,
			
			RZKEY_6,
			
			RZKEY_7,
			
			RZKEY_8,
			
			RZKEY_9,
			
			RZKEY_0,
			
			RZKEY_A = 770,
			
			RZKEY_B = 1031,
			
			RZKEY_C = 1029,
			
			RZKEY_D = 772,
			
			RZKEY_E = 516,
			
			RZKEY_F = 773,
			
			RZKEY_G,
			
			RZKEY_H,
			
			RZKEY_I = 521,
			
			RZKEY_J = 776,
			
			RZKEY_K,
			
			RZKEY_L,
			
			RZKEY_M = 1033,
			
			RZKEY_N = 1032,
			
			RZKEY_O = 522,
			
			RZKEY_P,
			
			RZKEY_Q = 514,
			
			RZKEY_R = 517,
			
			RZKEY_S = 771,
			
			RZKEY_T = 518,
			
			RZKEY_U = 520,
			
			RZKEY_V = 1030,
			
			RZKEY_W = 515,
			
			RZKEY_X = 1028,
			
			RZKEY_Y = 519,
			
			RZKEY_Z = 1027,
			
			RZKEY_NUMLOCK = 274,
			
			RZKEY_NUMPAD0 = 1299,
			
			RZKEY_NUMPAD1 = 1042,
			
			RZKEY_NUMPAD2,
			
			RZKEY_NUMPAD3,
			
			RZKEY_NUMPAD4 = 786,
			
			RZKEY_NUMPAD5,
			
			RZKEY_NUMPAD6,
			
			RZKEY_NUMPAD7 = 530,
			
			RZKEY_NUMPAD8,
			
			RZKEY_NUMPAD9,
			
			RZKEY_NUMPAD_DIVIDE = 275,
			
			RZKEY_NUMPAD_MULTIPLY,
			
			RZKEY_NUMPAD_SUBTRACT,
			
			RZKEY_NUMPAD_ADD = 533,
			
			RZKEY_NUMPAD_ENTER = 1045,
			
			RZKEY_NUMPAD_DECIMAL = 1300,
			
			RZKEY_PRINTSCREEN = 15,
			
			RZKEY_SCROLL,
			
			RZKEY_PAUSE,
			
			RZKEY_INSERT = 271,
			
			RZKEY_HOME,
			
			RZKEY_PAGEUP,
			
			RZKEY_DELETE = 527,
			
			RZKEY_END,
			
			RZKEY_PAGEDOWN,
			
			RZKEY_UP = 1040,
			
			RZKEY_LEFT = 1295,
			
			RZKEY_DOWN,
			
			RZKEY_RIGHT,
			
			RZKEY_TAB = 513,
			
			RZKEY_CAPSLOCK = 769,
			
			RZKEY_BACKSPACE = 270,
			
			RZKEY_ENTER = 782,
			
			RZKEY_LCTRL = 1281,
			
			RZKEY_LWIN,
			
			RZKEY_LALT,
			
			RZKEY_SPACE = 1287,
			
			RZKEY_RALT = 1291,
			
			RZKEY_FN,
			
			RZKEY_RMENU,
			
			RZKEY_RCTRL,
			
			RZKEY_LSHIFT = 1025,
			
			RZKEY_RSHIFT = 1038,
			
			RZKEY_MACRO1 = 256,
			
			RZKEY_MACRO2 = 512,
			
			RZKEY_MACRO3 = 768,
			
			RZKEY_MACRO4 = 1024,
			
			RZKEY_MACRO5 = 1280,
			
			RZKEY_OEM_1 = 257,
			
			RZKEY_OEM_2 = 268,
			
			RZKEY_OEM_3,
			
			RZKEY_OEM_4 = 524,
			
			RZKEY_OEM_5,
			
			RZKEY_OEM_6,
			
			RZKEY_OEM_7 = 779,
			
			RZKEY_OEM_8,
			
			RZKEY_OEM_9 = 1034,
			
			RZKEY_OEM_10,
			
			RZKEY_OEM_11,
			
			RZKEY_EUR_1 = 781,
			
			RZKEY_EUR_2 = 1026,
			
			RZKEY_JPN_1 = 21,
			
			RZKEY_JPN_2 = 1037,
			
			RZKEY_JPN_3 = 1284,
			
			RZKEY_JPN_4 = 1289,
			
			RZKEY_JPN_5,
			
			RZKEY_KOR_1 = 21,
			
			RZKEY_KOR_2 = 781,
			
			RZKEY_KOR_3 = 1026,
			
			RZKEY_KOR_4 = 1037,
			
			RZKEY_KOR_5 = 1284,
			
			RZKEY_KOR_6 = 1289,
			
			RZKEY_KOR_7,
			
			RZKEY_INVALID = 65535
		}

		
		public enum RZLED
		{
			
			RZLED_LOGO = 20
		}
	}
}
