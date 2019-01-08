using System;
using Rewired.Dev;

namespace RewiredConsts
{
	public static class Actions
	{
		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "MenuToggle")]
		public const int MenuToggle = 2;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "Esc")]
		public const int Esc = 3;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "Back")]
		public const int Back = 43;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "Inventory")]
		public const int Inventory = 19;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "SurvivalBook")]
		public const int SurvivalBook = 28;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "PreviousChapter")]
		public const int PreviousChapter = 39;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "NextChapter")]
		public const int NextChapter = 41;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "PreviousPage")]
		public const int PreviousPage = 40;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "NextPage")]
		public const int NextPage = 42;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "Debug")]
		public const int Debug = 46;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "ScrollY")]
		public const int ScrollY = 47;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "ScrollX")]
		public const int ScrollX = 52;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "Rebind")]
		public const int Rebind = 53;

		[ActionIdFieldInfo(categoryName = "Navigate and interact with menu", friendlyName = "SetOption")]
		public const int SetOption = 54;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Horizontal")]
		public const int Horizontal = 7;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Vertical")]
		public const int Vertical = 8;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Mouse_X")]
		public const int Mouse_X = 12;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Joy X")]
		public const int Joy_X = 14;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Mouse_Y")]
		public const int Mouse_Y = 13;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Joy Y")]
		public const int Joy_Y = 15;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Jump")]
		public const int Jump = 9;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Run")]
		public const int Run = 16;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "Crouch")]
		public const int Crouch = 21;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "checkArms")]
		public const int checkArms = 27;

		[ActionIdFieldInfo(categoryName = "move character arround", friendlyName = "birdOnHand")]
		public const int birdOnHand = 35;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Fire1")]
		public const int Fire1 = 10;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "AltFire")]
		public const int AltFire = 11;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Take")]
		public const int Take = 17;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Drop")]
		public const int Drop = 18;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "RestKey")]
		public const int RestKey = 20;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Save")]
		public const int Save = 26;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Craft")]
		public const int Craft = 23;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Lighter")]
		public const int Lighter = 22;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Utility")]
		public const int Utility = 29;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "Map")]
		public const int Map = 45;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "ItemSlot1")]
		public const int ItemSlot1 = 48;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "ItemSlot2")]
		public const int ItemSlot2 = 49;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "ItemSlot3")]
		public const int ItemSlot3 = 50;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "ItemSlot4")]
		public const int ItemSlot4 = 51;

		[ActionIdFieldInfo(categoryName = "Interact with world objects", friendlyName = "WalkyTalky")]
		public const int WalkyTalky = 32;

		[ActionIdFieldInfo(categoryName = "build structures", friendlyName = "Rotate")]
		public const int Rotate = 24;

		[ActionIdFieldInfo(categoryName = "build structures", friendlyName = "Build")]
		public const int Build = 38;

		[ActionIdFieldInfo(categoryName = "using items from the inventory", friendlyName = "Batch")]
		public const int Batch = 30;

		[ActionIdFieldInfo(categoryName = "using items from the inventory", friendlyName = "Equip")]
		public const int Equip = 36;

		[ActionIdFieldInfo(categoryName = "using items from the inventory", friendlyName = "Combine")]
		public const int Combine = 37;

		[ActionIdFieldInfo(categoryName = "Mp Specific inputs", friendlyName = "OpenChat")]
		public const int OpenChat = 31;

		[ActionIdFieldInfo(categoryName = "Mp Specific inputs", friendlyName = "Submit")]
		public const int Submit = 33;

		[ActionIdFieldInfo(categoryName = "Mp Specific inputs", friendlyName = "CloseChat")]
		public const int CloseChat = 34;

		[ActionIdFieldInfo(categoryName = "Mp Specific inputs", friendlyName = "PlayerList")]
		public const int PlayerList = 44;
	}
}
