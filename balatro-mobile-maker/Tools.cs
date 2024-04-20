using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace balatro_mobile_maker;
internal class Tools
{
    public enum ProcessTools
    {
        SevenZip,
        ADB,
        Java,
        Python
    }

    public static void useTool(ProcessTools tool, string args)
    {
        switch (tool)
        {
            case ProcessTools.ADB:
                Platform.useADB(args);
                break;
            case ProcessTools.Python:
                Platform.usePython(args);
                break;
            case ProcessTools.Java:
                Platform.useJava(args);
                break;
            case ProcessTools.SevenZip:
                Platform.useSevenZip(args);
                break;
        }
    }
}
