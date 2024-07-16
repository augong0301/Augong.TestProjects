using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
