using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using static balatro_mobile_maker.Program;
using System.Diagnostics;
using System.IO.Compression;

namespace balatro_mobile_maker;
internal class Tools
{
    public enum ProcessTools
    {
        SevenZip,
        ADB,
        Java,
    }

    public static void useTool(ProcessTools tool, string args)
    {
        switch (tool)
        {
            case ProcessTools.ADB:
                Platform.useADB(args);
                break;
            case ProcessTools.Java:
                Platform.useOpenJDK(args);
                break;
            case ProcessTools.SevenZip:
                Platform.useSevenZip(args);
                break;
        }
    }

    public static bool directoryExists(string path)
    {
        return Directory.Exists(path);
    }
    
    public static void fileMove(string source, string dest)
    {
        fileCopy(source, dest);
        tryDelete(source);
    }

    public static void fileCopy(string source, string dest)
    {
        if (!fileExists(source))
            return;

        if (fileExists(dest))
            tryDelete(dest);

        File.Copy(source, dest);
    }

    public static bool fileExists(string file)
    {
        return File.Exists(file);
    }

    public static void tryDelete(string target)
    {
        if (Directory.Exists(target))
        {
            if (_verboseMode)
                Log("Deleting \"" + target + "/\"...");
            Directory.Delete(target, true);
        }

        if (File.Exists(target))
        {
            if (_verboseMode)
                Log("Deleting \"" + target + "\"...");
            File.Delete(target);
        }
    }

    /// <summary>
    /// Attempts to download a file if it does not exist
    /// </summary>
    /// <param name="name">Friendly name for file (for logging)</param>
    /// <param name="link">Download URL</param>
    /// <param name="fileName">File path to save to</param>
    public static void TryDownloadFile(string name, string link, string fileName)
    {
        //If the file does not already exist
        if (!File.Exists(fileName))
        {
            Log("Downloading " + name + "...");
            // TODO: WebClient is Obsolete, and needs to be replaced.
            using (var client = new WebClient())
            {
                client.DownloadFile(link, fileName!);
            }

            //Make sure it exists
            if (File.Exists(fileName))
                Log(name + " downloaded successfully.");
            else
            {
                //If it does not, that's a critical error
                Log("Failed to download " + name + "!");
                Exit();
            }
        }
        else
        {
            //File already exists
            Log(fileName + " already exists.");
        }
    }

    /// <summary>
    /// Wrapper for logging to the console.
    /// </summary>
    /// <param name="text">Text to be logged.</param>
    // This saves me from writing Console.WriteLine a million times
    // ReSharper disable once GrammarMistakeInComment
    // There's probably a better way to make an alias in C#. Oh well
    public static void Log(string text)
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Exits the application after the user presses any key
    /// </summary>
    public static void Exit()
    {
        View.Cleanup();
        Log("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }

    /// <summary>
    /// Prints output (or errors) if verbose mode is enabled
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
    {
        string data = e.Data;
        if (_verboseMode && data != null && data != "")
            try //I got System.InvalidOperationException here once. Seems to happen if ADB exits fatally
            {
                Log("[" + ((System.Diagnostics.Process)sender).ProcessName + "]: " + data);
            }
            catch
            {
                Log("Error Occurred!");
            }
        //I'd like to use another color for this text specifically, but I'm not sure if it's possible.
    }

    /// <summary>
    /// Starts process using the platform's shell
    /// Currently this is restricted to Windows.
    /// </summary>
    /// <param name="args">Command to pass to the shell</param>
    /// <returns>Process, post finishing.</returns>
    public static Process RunCommand(string command, string args)
    {
        //Create a new cmd process
        Process commandLineProcess = new Process();
        commandLineProcess.StartInfo.FileName = command;
        commandLineProcess.StartInfo.CreateNoWindow = true;
        commandLineProcess.StartInfo.UseShellExecute = false;

        //Output and error handling
        commandLineProcess.StartInfo.RedirectStandardOutput = true;
        commandLineProcess.StartInfo.RedirectStandardError = true;
        commandLineProcess.OutputDataReceived += ProcessOutputHandler;
        commandLineProcess.ErrorDataReceived += ProcessOutputHandler;

        //Apply args
        commandLineProcess.StartInfo.Arguments = args;

        //Start the process
        commandLineProcess.Start();
        commandLineProcess.BeginOutputReadLine();

        //This could be changed to allow for multi-threading, but that's handled with System.Threading anyway
        commandLineProcess.WaitForExit();

        //On exit
        commandLineProcess.Exited += (_, _) =>
        {
            //Check for errors
            if (commandLineProcess.ExitCode != 0)
            {
                //Error occurred
                Log("An unexpected error occurred!");
                if (!_verboseMode)
                    Log("Try running in verbose mode to determine the cause of the error.");
            }
            else
                Log("\n");
        };

        //Return the process
        return commandLineProcess;
    }

    /// <summary>
    /// Prompt user for a 'Y' or a 'N' (not case-sensitive)
    /// </summary>
    /// <param name="question">Prompt for the user</param>
    /// <returns>Status of prompt - true for 'Y', false for 'N'</returns>
    public static bool AskQuestion(string question)
    {
        string input = null;
        do
        {
            if (input != null)
                Log("Enter either 'Y' or 'N'!");
            Log(question + " (y/n):");
            input = Console.ReadLine()?.ToLower();
        } while (input != "y" && input != "n");

        return input == "y";
    }

    public static void ModifyZip()
    {
        string existingZipFile = "balatro-base.zip";
        string newFilePath = "game.love";
        string arcname = "Payload/Balatro.app/game.love";

        using (ZipArchive archive = ZipFile.Open(existingZipFile, ZipArchiveMode.Update))
        {
            archive.CreateEntryFromFile(newFilePath, arcname);
        }
    }
}
