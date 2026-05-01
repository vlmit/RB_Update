using System;

namespace Tessa.Extensions.Chronos.Medo.MedoSend.Helpers
{
    public class NewMessageInfo
    {
        public Guid ID { get; set; }
        public Guid RowID { get; set; }
        public int MedoTypeID { get; set; }
        public Guid? MedoPartnerID { get; set; }
        public int MedoXsdVersion { get; set; }
        public Guid MessageID { get; set; }
        public Guid? ResponseMesID { get; set; }
        public string MedoPartnerName { get; set; }
        public int MedoStatusID { get; set; }
    }
}