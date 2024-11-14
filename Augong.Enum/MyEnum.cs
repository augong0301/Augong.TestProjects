namespace Augong.Enum
{
	[Flags]
	public enum SensorType
	{
		ScN = 1,// maped to AcqCard ch1; not change the sequence of SensorType
		ScO = 2,
		PL = 4,

		Sp1 = 8,
		ZrO2 = 16,
		ZcO2 = 32,
		Sp2 = 64,
		None = 128, // maped to AcqCard ch8

		Pho = 256, // Sp1 -Sp2

		Sp12 = 512, //sp1 + sp2
	}



    [Flags]
    public enum FHLaserStatusCode : long
    {
        None                    = 0,

        // laser
        LaserFault            = 0x00000001,
        LaserEmission         = 0x00000002,
        LaserReady            = 0x00000004,
        LaserStandby          = 0x00000008,

        CDRHDelay             = 0x00000010, // warm up delay
        LaserHardwareFault    = 0x00000020,
        LaserError            = 0x00000040,
        LaserPowerCalibration = 0x00000080, // only LX349 

        LaserWarmUp           = 0x00000100,
        LaserNoise            = 0x00000200,
        ExternalOperatingMode = 0x00000400,
        FieldCalibration      = 0x00000800,

        LaserPowerVoltage     = 0x00001000,

        // controller
        ControllerStandby     = 0x02000000,
        ControllerInterlock   = 0x04000000,
        ControllerEnumeration = 0x08000000,

        ControllerError       = 0x10000000,
        ControllerFault       = 0x20000000,
        RemoteActive          = 0x40000000,

        ControllerIndicator   = 0x80000000,
    }

}
