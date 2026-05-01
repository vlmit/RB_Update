using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Extensions.Chronos.SSTUIntegration.Helpers;

namespace Tessa.Extensions.Chronos.SSTUIntegration.Models
{
    public class SSTURequest<T>
    {
        public Guid departmentId { get; set; }
        public bool isDirect { get; set; }
        public string format { get; set; }
        public string number { get; set; }
        public string createDate { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public List<T> Questions { get; set; }
    }
}
