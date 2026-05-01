using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessa.Extensions.Chronos.MobileSign.Helpers
{
    public class NewMobileSignInfo
    {
        public Guid ID { get; set; }
        public Guid RowID { get; set; }
        //public string SignedBy { get; set; }
        public string SignedFileName { get; set; }
        public string SignContent { get; set; }
        public string SignType { get; set; }
        public string SignDate { get; set; }
        public bool IsProcessed { get; set; }
    }
}
