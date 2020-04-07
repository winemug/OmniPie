using System;
using System.Collections.Generic;
using System.Text;

namespace OmniPie.Api
{
    public class OmniPyHistoryEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public int Progress { get; set; }
        public int Minutes { get; set; }
        public string Command { get; set; }
        public decimal Delivered { get; set; }
        public decimal Canceled { get; set; }
        public decimal Reservoir { get; set; }
    }
}
