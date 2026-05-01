using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessa.Extensions.Chronos.SSTUIntegration.Models
{
    public class QuestionModel
    {
        public string code { get; set; }
        public string status { get; set; }
    }

    public class QNotRecieved : QuestionModel 
    {
        public QNotRecieved(string code)
        {
            this.code = code;
            this.status = "NotRecieved";
        }
    }

    public class QNotRegistered : QuestionModel
    {
        public DateTime incomingDate { get; set; }
    }

    public class QInWork : QuestionModel
    {
        public bool isExtend { get; set; }
        public DateTime incomingDate { get; set; }
        public DateTime registrationDate { get; set; }
    }

    public class QInWorkExtended : QuestionModel
    {
        public bool isExtend { get; set; }
        public DateTime incomingDate { get; set; }
        public DateTime registrationDate { get; set; }
    }

    public class QLeftWithoutAnswer : QuestionModel
    {
        public DateTime incomingDate { get; set; }
        public DateTime registrationDate { get; set; }
        public DateTime responseDate { get; set; }
    }

    public class ConsideredModel : QuestionModel
    {
        public DateTime incomingDate { get; set; }
        public DateTime registrationDate { get; set; }
        public DateTime responseDate { get; set; }
        public Attachment attachment { get; set; }
    }

    public class QExplained : ConsideredModel { }

    public class QAnswered : ConsideredModel { }

    public class QNotSupported : ConsideredModel { }

    public class QSupported : ConsideredModel
    {
        public bool actionsTaken { get; set; }
    }
}
