namespace WindowSelector.Common
{
    public interface INativeWindowInfo
    {
         string ProcessName { get; set; }
         bool IsWhiteListed { get; set; }
         bool IsBlackListed { get; set; }
         int PID { get; set; }

        string Title { get; set; }

    }
}
