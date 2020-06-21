using AppKit;

namespace Zemulator.Mac
{
    static class MainClass
    {
        static void Main( string[] args )
        {
            NSApplication.Init();
            NSApplication.Main( args );
        }
    }
}
