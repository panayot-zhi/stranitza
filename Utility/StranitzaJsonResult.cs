

// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles

namespace stranitza.Utility
{
    public class StranitzaJsonResult
    {
        public bool success { get; set; }

        public object data { get; set; }

        public string[] errors { get; set; }
    }
}

#pragma warning restore IDE1006 // Naming Styles