using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace BMR.Common.Models
{
    public partial class BmrencodedData
    {
        public int Id { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string BMRResult { get; set; }
    }
}
