using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Helper
{
    public class ScryptPasswordHasherOptions
    {
        public int IterationCount { get; set; } = 16384;
        public int BlockSize { get; set; } = 8;
        public int ThreadCount { get; set; } = 1;
    }
}
