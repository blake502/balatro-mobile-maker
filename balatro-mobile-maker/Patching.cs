using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static balatro_mobile_maker.Tools;

namespace balatro_mobile_maker;

internal class Patching
{
    /// <summary>
    /// Apply a "patch" given a file path, a string from a line to be replaced, and the text with which to replace it.
    /// </summary>
    /// <param name="file">File to patch</param>
    /// <param name="lineContains">Line to be replaced</param>
    /// <param name="replaceWith">New contents for the line</param>
    /// <returns></returns>
    // We wish to keep the return type, incase we want to make use of this later.
    // ReSharper disable once UnusedMethodReturnValue.Local
    static bool ApplyPatch(string file, string lineContains, string replaceWith)
    {
        //Read the file
        Log("Loading " + file + " file...");
        string[] loadedFile = File.ReadAllLines("Balatro/" + file);

        //Search for the line to replace
        bool found = false;
        for (int i = 0; i < loadedFile.Length; i++)
            // This has to be made culture-invariant, or, in some regions this could result in unexpected behaviour
            // Consider also using .Contains here - is there a reason we make use of IndexOf?
            if (loadedFile[i].IndexOf(lineContains, StringComparison.Ordinal) != -1)
            {
                //Replace the line
                loadedFile[i] = replaceWith;
                found = true;
                break;
            }

        if (found)
        {
            //If it is found, write the file.
            Log("Successfully applied patch...");
            File.WriteAllLines("Balatro/" + file, loadedFile);
        }
        else
            Log("Unable to find patch location...");

        return found;
    }

    /// <summary>
    /// Prompts the user to select which patches they want, then applies them.
    /// </summary>
    // This is hideous, but it works.
    public static void Begin()
    {
        Log("Applying mobile compatibilty patch...");
        //Android platform support
        ApplyPatch("globals.lua", "loadstring", @"    -- Removed 'loadstring' line which generated lua code that exited upon starting on mobile
    if love.system.getOS() == 'Android' or love.system.getOS() == 'iOS' then
        self.F_SAVE_TIMER = 5
        self.F_DISCORD = true
        self.F_NO_ACHIEVEMENTS = true
        self.F_CRASH_REPORTS = false
        self.F_SOUND_THREAD = true
        self.F_VIDEO_SETTINGS = false
        self.F_ENGLISH_ONLY = false
        self.F_QUIT_BUTTON = false
    end");
        //On-screen keyboard
        ApplyPatch("functions/button_callbacks.lua", "G.CONTROLLER.text_input_hook == e and G.CONTROLLER.HID.controller", "  if G.CONTROLLER.text_input_hook == e and (G.CONTROLLER.HID.controller or G.CONTROLLER.HID.touch) then");

        // Flame fix patch
        ApplyPatch("resources/shaders/flame.fs", "#endif", "#endif\n#ifdef GL_ES\n\tprecision MY_HIGHP_OR_MEDIUMP float;\n#endif");
        ApplyPatch("resources/shaders/flame.fs", "vec4 effect( vec4 colour, Image texture, vec2 texture_coords, vec2 screen_coords )", "mediump vec4 effect( mediump vec4 colour, Image texture, mediump vec2 texture_coords, mediump vec2 screen_coords )");

        //Ask whether they want the FPS cap patch
        if (AskQuestion("Would you like to apply the FPS cap patch?"))
        {
            int fps = -1;
            do
            {
                Log("Please enter your desired FPS cap (or leave blank to set to device refresh rate):");
                // Conditional access as ReadLine is nullable
                string input = Console.ReadLine()?.ToLower();

                if (String.IsNullOrWhiteSpace(input))
                {
                    //Set to refresh rate if blank
                    fps = -2;
                    break;
                }

                try
                {
                    //Set to specific value
                    fps = Convert.ToInt32(input);
                }
                catch
                {
                    // ignored
                }
            } while (fps <= 0 || fps > 999);

            if (fps > 0)
            {
                //Apply the patch using the given FPS
                ApplyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", "        G.FPS_CAP = " + fps.ToString());
            }
            else
            {
                //Apply the patch using the display refresh rate
                ApplyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", @"        G.FPS_CAP = G.FPS_CAP or select(3, love.window.getMode())['refreshrate']");
            }
        }

        //Extra patches

        if (AskQuestion("Would you like to apply the landscape orientation patch?"))
        {
            ApplyPatch("functions/button_callbacks.lua", "resizable = true,", "    resizable = not (love.system.getOS() == 'Android' or love.system.getOS() == 'iOS'),");
        }

        // Asking ReSharper to disable naming here, as, DPI (all-caps) is correct, not Dpi
        // ReSharper disable once InconsistentNaming
        if (AskQuestion("Would you like to apply the high DPI patch (recommended for devices with high resolution)?"))
        {
            ApplyPatch("conf.lua", "t.window.width = 0", "    t.window.width = 0\n    t.window.usedpiscale = false");
            ApplyPatch("functions/button_callbacks.lua", "highdpi = (love.system.getOS() == 'OS X')", "    highdpi = (love.system.getOS() == 'OS X' or love.system.getOS() == 'Android' or love.system.getOS() == 'iOS')");
        }

        if (AskQuestion("Would you like to apply the CRT shader disable patch? (Required for Pixel and some other devices!)"))
        {
            ApplyPatch("globals.lua", "crt = ", "            crt = 0,");
            ApplyPatch("game.lua", "G.SHADERS['CRT'])", "");
        }

        //TODO: Better command line args handling
        if (Program.ArgsEnableAccessibleSave && AskQuestion("Would you like to apply the external storage patch? (NOT recommended)"))
            ApplyPatch("conf.lua", "t.window.width = 0", "    t.window.width = 0\n    t.externalstorage = true");
    }
}