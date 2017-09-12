using System;
using System.Diagnostics;

namespace WindowSelector.Controls
{
    [DebuggerDisplay("{Text}")]
    public class CommandDescription : IEquatable<CommandDescription>
    {
        public string Alias { get; set; }
        public string Text { get; set; }
        public bool IsDefault { get; set; }

        public string DisplayText => $"({Alias}) {Text}";

        public bool Equals(CommandDescription other)
        {
            return other?.Alias == Alias;
        }

        //public override bool Equals(object obj)
        //{
        //    var other = obj as CommandDescription;

        //    if (other == null)
        //    {
        //        return false;
        //    }

        //    return other.Alias == Alias;
        //}

        //public override int GetHashCode()
        //{
        //    return Alias.GetHashCode();
        //}
    }
}
