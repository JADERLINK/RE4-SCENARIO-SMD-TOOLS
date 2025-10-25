using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class ValidateMagic
    {
        public static void Validate(uint Magic) 
        {

            if (!(
                      Magic == 0x0040
                   || Magic == 0x0140
                   || Magic == 0x0031
                   || Magic == 0x0020
                   || Magic == 0x0010
                   ))
            {
                throw new ApplicationException("The content of the 'Magic' property is invalid.");
            }

        }

    }
}
