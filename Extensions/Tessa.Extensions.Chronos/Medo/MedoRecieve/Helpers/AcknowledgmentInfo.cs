using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve.Helpers
{
    public class AcknowledgmentInfo
    {
        public IDbScope DbScope { get; set; }
        public Guid CardID { get; set; }
        public Guid MainMesID { get; set; }
        public Guid MedoPartnerID { get; set; }
        //public XsdVersion XsdVersion { get; set; }
        public string MedoPartnerName { get; set; }
    }
}
