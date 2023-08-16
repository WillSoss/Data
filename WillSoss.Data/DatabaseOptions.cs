using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WillSoss.Data
{
    public class DatabaseOptions
    {
        public Script? CreateScript { get; set; }
        public Script? ResetScript { get; set; }
        public Script? DropScript { get; set; }
        public int? CommandTimeout { get; set; }
        public int? PostCreateDelay { get; set; }
        public int? PostDropDelay { get; set; }

    }
}
