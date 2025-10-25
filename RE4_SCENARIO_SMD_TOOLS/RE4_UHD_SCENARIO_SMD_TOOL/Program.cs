namespace RE4_UHD_SCENARIO_SMD_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            SHARED_UHD_SCENARIO_SMD.MainAction.MainContinue(args, false, SimpleEndianBinaryIO.Endianness.LittleEndian);
        }

    }
}
