﻿using System;
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
        Python,
        UberApkSigner
    }

    public static void useTool(ProcessTools tool, string args)
    {
        switch (tool)
        {
            case ProcessTools.ADB:
                Platform.useADB(args);
            break;
        }
    }
}