using System;


[Flags]
public enum PlayerCloting
{
	
	Default = 0,
	
	Jacket = 1,
	
	Blacksuit = 2,
	
	Vest = 4,
	
	Hoodie = 8,
	
	Beanie1 = 16,
	
	Beanie2 = 32,
	
	Beanie3 = 64,
	
	ShirtOpen = 128,
	
	ShirtClosed = 256,
	
	JacketLow = 512,
	
	HoodieUp = 1024
}
