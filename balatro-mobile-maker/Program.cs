namespace balatro_mobile_maker;


/// <summary>
/// Main entry point for the program.
/// </summary>
internal static class Program
{
    public static bool _verboseMode;

    //TODO: Better command line args handling
    public static bool ArgsEnableAccessibleSave = false;

    /// <summary>
    /// Main entry point of the program
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        //TODO: Better command line args handling 
        foreach (string s in args)
            if(s == "--enable-external-storage-patch")
                ArgsEnableAccessibleSave = true;

        View mainView = new View();
        mainView.Begin();
    }
}