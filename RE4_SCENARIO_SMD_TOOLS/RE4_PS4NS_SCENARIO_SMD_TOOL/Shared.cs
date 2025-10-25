using System;

namespace SHARED_TOOLS
{
    public static class Shared
    {
        private const string VERSION = "V.1.3.0 (2025-10-25)";

        public static string HeaderText()
        {
            return "# github.com/JADERLINK/RE4-SCENARIO-SMD-TOOLS" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_PS4NS_SCENARIO_SMD_TOOL by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"mariokart64n\" and \"CodeMan02Fr\"" + Environment.NewLine +
                   "# Material information by \"Albert\"" + Environment.NewLine +
                  $"# Version {VERSION}";
        }
    }
}
