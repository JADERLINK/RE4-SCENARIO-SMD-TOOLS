namespace RE4_PS4NS_SCENARIO_SMD_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            SHARED_UHD_SCENARIO_SMD.MainAction.MainContinue(args, true, SimpleEndianBinaryIO.Endianness.LittleEndian);
        }

    }
}

