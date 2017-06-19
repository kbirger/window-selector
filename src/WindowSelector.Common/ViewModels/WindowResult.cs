using System.Windows;

namespace WindowSelector.Common.ViewModels
{
    public abstract class WindowResult
    {
        public bool HasThumb { get; protected set; }

        public string Label { get; protected set; }
        public string DisplayText { get; protected set; }
        public string Details { get; protected set; }
        public object Icon { get; protected set; }
        public virtual object Value { get; protected set; }
        public virtual bool IsWhiteListed { get; protected set; }
        public virtual bool IsBlackListed { get; protected set; }
        public abstract void Select(bool centerWindow);
        public abstract void Close();
        public abstract void Minimize();
        public abstract void Whitelist();
        public abstract void Blacklist();
        public abstract void UnWhitelist();
        public abstract void UnBlacklist();
        public FrameworkElement ThumbnailElement { get; protected set; }
        public int Priority { get; protected set; }

    }
}