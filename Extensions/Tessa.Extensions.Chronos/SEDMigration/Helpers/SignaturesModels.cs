using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessa.Extensions.Chronos.SEDMigration.Helpers
{
    public class SEDMigrationFileSign
    {
        public DateTime? signed { get; set; }
        public string authorExtID { get; set; }
        public FileSignType signType { get; set; }
        public string content { get; set; }
    }

    public class SEDMigrationFileToSign
    {
        public string name { get; set; }
        public List<SEDMigrationFileSign> signs { get; set; }
    }

    public class SEDMigrationCardsToSign
    {
        public string SPUID { get; set; }
        public List<SEDMigrationFileToSign> files { get; set; }
    }

    public enum FileSignType
    {
        [Description("Утверждающая")]
        ApproveYes,

        [Description("Утверждающая (отклонено)")]
        ApproveNo,

        [Description("Согласующая (с замечаниями)")]
        EndorseYesComments,

        [Description("Согласующая")]
        EndorseYes,

        [Description("Согласующая (отклонено)")]
        EndorseNo,

        [Description("Заверяющая")]
        CertificateYes,

        [Description("Заверяющая (отклонено)")]
        CertificateNo
    }
}
