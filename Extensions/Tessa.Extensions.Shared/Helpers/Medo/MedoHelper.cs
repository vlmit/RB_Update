using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using LinqToDB.Common;
using LinqToDB.Data;
using NLog;
using Tessa.Platform;
using Tessa.Platform.Data;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Shared.Helpers.Medo
{
    public static class MedoHelper
    {
        public static readonly IReadOnlyDictionary<NoticeType, string> NoticeTypeDict = new Dictionary<NoticeType, string>
        {
            {NoticeType.Registered, MedoTag.TagDocumentAccepted},
            {NoticeType.RegDenied, MedoTag.TagDocumentRefused},
            {NoticeType.ExecuterAssign, MedoTag.TagexecutorAssigned},
            {NoticeType.ReportPrepared, MedoTag.TagreportPrepared},
            {NoticeType.ReportSend, MedoTag.TagreportSent},
            {NoticeType.Execute, MedoTag.TagcourseChanged},
            {NoticeType.Publish, MedoTag.TagdocumentPublished},
            {NoticeType.StateReg, MedoTag.TagDocumentAccepted}
        };

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Имя zip папки
        /// </summary>
        /// <param name="id">id карточки, по которой формируется сообщение</param>
        /// <returns></returns>
        public static string ZipName(Guid id)
        {
            var str = id.ToString().Replace("-", "");
            return $"{str}.edc.zip";
        }

        /// <summary>
        ///     валидация xml по  xsd схеме
        /// </summary>
        /// <param name="doc">xml файл</param>
        /// <param name="xsdVersion"></param>
        /// <param name="xmlType"></param>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="path">путь к xsd схеме</param>
        public static bool CheckXml(
            this XDocument doc,
            XsdVersion xsdVersion,
            XmlType xmlType,
            IValidationResultBuilder validationResult,
            string path)
        {
            logger.Info("MedoHelper CheckXml");

            var result = true;
            var schemas = new XmlSchemaSet();

            switch (xsdVersion)
            {
                case XsdVersion.NoSupportVersion:
                {
                    validationResult.AddError(doc, "Форматы ниже версии 2.7 в настоящее время не поддерживаются.");
                    result = true;
                    break;
                }
                case XsdVersion.OldVersion:
                {
                    logger.Info("MedoHelper CheckXml OldVersion");
                    using var file = File.OpenRead(path);
                    var namespaces = xmlType == XmlType.Message
                        ? MedoConst.Xdms27.NamespaceName
                        : MedoConst.Ns27.NamespaceName;

                    schemas.Add(namespaces, XmlReader.Create(file));
                    doc.Validate(schemas, (_, e) =>
                    {
                        if (e?.Exception != null)
                        {
                            validationResult.AddException(doc, e.Exception);
                            result = false;
                        }
                    });
                    
                    break;
                }
                case XsdVersion.NewVersion:
                {
                    logger.Info("MedoHelper CheckXml NewVersion");
                    using var file = File.OpenRead(path);
                    var namespaces = xmlType == XmlType.Message
                        ? MedoConst.Xdms271.NamespaceName
                        : MedoConst.Ns271.NamespaceName;

                    schemas.Add(namespaces, XmlReader.Create(file));
                    doc.Validate(schemas, (_, e) =>
                    {
                        if (e?.Exception != null)
                        {
                            validationResult.AddException(doc, e.Exception);
                            result = false;
                        }
                    });
                    
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(xsdVersion), xsdVersion, null);
            }

            return result;
        }

        public static XsdVersion GetXsdVersion(
            this XDocument doc)
        {
            var version = doc?.Elements().FirstOrDefault()?.Attribute(MedoConst.Xdms27 + MedoTag.TagVersion)?.Value
                ?? doc?.Elements().FirstOrDefault()?.Attribute(MedoConst.Xdms271 + MedoTag.TagVersion)?.Value;

            return version switch
            {
                "2.7.0" or "2.7" => XsdVersion.OldVersion,
                "2.7.1" => XsdVersion.NewVersion,
                "2.2" or "2.2.0" => XsdVersion.Version22,
                "2.5" or "2.5.0" => XsdVersion.Version25,
                "2.6" or "2.6.0" => XsdVersion.Version26,
                "2.0" or "2.0.0" => XsdVersion.Version20,
                _ => XsdVersion.NoSupportVersion,
            };
        }

        /// <summary>
        ///     информация о расположении штампа
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="type">тип штампа</param>
        /// <param name="command">запрос</param>
        /// <param name="cardID"></param>
        /// <param name="signID">id подписи</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, int?>> GetStampDataAsync(
            IDbScope dbScope,
            IValidationResultBuilder validationResult,
            StampType type,
            string command,
            Guid cardID,
            Guid? signID = null,
            CancellationToken cancellationToken = default)
        {
            await using (dbScope.Create())
            {
                var db = dbScope.Db;
                var result = new Dictionary<string, int?>();

                var param = signID == null
                    ? new[]
                    {
                        new DataParameter("@CardID", cardID),
                        new DataParameter("@type", (int) type)
                    }
                    : new[]
                    {
                        new DataParameter("@CardID", cardID),
                        new DataParameter("@type", (int) type),
                        new DataParameter("@sign", signID)
                    };

                db.SetCommand(command, param).LogCommand();
                await using (var reader = await db.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.Add(MedoTag.TagPage, reader.GetValue<int?>(0));
                        result.Add(MedoTag.TagX, reader.GetValue<int?>(1));
                        result.Add(MedoTag.TagY, reader.GetValue<int?>(2));
                        result.Add(MedoTag.TagW, reader.GetValue<int?>(3));
                        result.Add(MedoTag.TagH, reader.GetValue<int?>(4));
                    }
                }

                foreach (var (key, value) in result)
                {
                    if (value == null)
                    {
                        validationResult.AddInfo(typeof(MedoHelper), $"Error in stamp info. {key} is null");
                    }
                }

                if (result.IsNullOrEmpty())
                {
                    validationResult.AddError(typeof(MedoHelper), $"Stamp {type.GetDescription()} data is null or empty");
                }

                return result;
            }
        }

        /// <summary>
        ///     записываем ValidationResult в лог
        /// </summary>
        /// <param name="validationResult"></param>
        /// <param name="logger"></param>
        /// <param name="messageID"></param>
        public static void MedoLoggerResult(this ILogger logger, IValidationResultBuilder validationResult, Guid messageID)
        {
            if (validationResult.HasData())
            {
                logger.Info($"ValidationResult for message {messageID}");
                logger.LogResult(validationResult);
            }
        }

        /// <summary>
        /// сортировака файлов из контейнера по требованиям мэдо
        /// </summary>
        /// <param name="list"></param>
        /// <param name="contSignName">имя файла подписи</param>
        public static void MedoSort(this List<string> list, string contSignName = null)
        {
            var first = list.First(x => Path.GetFileName(x) == MedoConst.PassportXmlName);
            list.Remove(first);

            //удаляем файл подписи контейнера
            if (contSignName != null)
            {
                var contSign = list.First(x => Path.GetFileName(x) == contSignName);
                list.Remove(contSign);
            }

            list.Sort(StringComparer.Ordinal);
            list.Insert(0, first);
        }

        /// <summary>
        /// создаем массив байт из всех файлов контейнера
        /// </summary>
        /// <param name="contPath">путь к папке контейнера</param>
        /// <param name="contSignName">имя файла подписи</param>
        /// <returns></returns>
        public static byte[] GetAllBytesFromContainer(string contPath, string contSignName = null)
        {
            //получаем отсортированный список файлов из папки контейнера
            var files = Directory.GetFiles(contPath).ToList();
            files.MedoSort(contSignName);

            var fileToSign = new List<byte>();
            files.ForEach(file => fileToSign.AddRange(File.ReadAllBytes(file)));

            return fileToSign.ToArray();
        }

        public static Tuple<XDocument, Guid> CreateAcknowledgMes(
            XsdVersion xsdVersion,
            bool accept,
            Guid messageId,
            string destId,
            string destName,
            string comment = null)
        {
            var respId = Guid.NewGuid();
            var date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local).ToString("yyyy-MM-ddTHH:mm:ssK");

            if (xsdVersion != XsdVersion.OldVersion || xsdVersion != XsdVersion.NewVersion)
            {
                xsdVersion = XsdVersion.OldVersion;
            }

            XDeclaration xd = new XDeclaration("1.0", "utf-8", null);
            xd.Standalone = null;

            var result = xsdVersion switch
            {
                XsdVersion.NoSupportVersion or XsdVersion.OldVersion => Tuple.Create(
                    new XDocument(xd,
                                  new XElement(MedoConst.Xdms27 + MedoTag.TagCommunication, new XAttribute(XNamespace.Xmlns + "xdms", MedoConst.Xdms27),
                                               new XAttribute(MedoConst.Xdms27 + MedoTag.TagVersion, MedoConst.Version27),
                                               new XElement(MedoConst.Xdms27 + MedoTag.TagHeader, new XAttribute(MedoConst.Xdms27 + MedoTag.TagMesType, MedoMessageType.Acknowledgment.GetDescription()),
                                                            new XAttribute(MedoConst.Xdms27 + MedoTag.TagMesUid, respId), new XAttribute(MedoConst.Xdms27 + MedoTag.TagCreated, DateTime.UtcNow),
                                                            new XElement(MedoConst.Xdms27 + MedoTag.TagSource, new XElement(MedoConst.Xdms27 + MedoTag.TagComOrg, MedoConst.RBName),
                                                                         new XAttribute(MedoConst.Xdms27 + MedoTag.TagOrgUid, MedoConst.RBMedoID))),
                                               new XElement(MedoConst.Xdms27 + MedoTag.TagAcknowledgment, new XAttribute(MedoConst.Xdms27 + MedoTag.TagResponseUid, messageId),
                                                            new XElement(MedoConst.Xdms27 + MedoTag.TagTime, DateTime.UtcNow), new XElement(MedoConst.Xdms27 + MedoTag.TagAccepted, accept),
                                                            comment.IsNullOrEmpty() ? null : new XElement(MedoConst.Xdms27 + MedoTag.TagComment, comment)))), respId),
                XsdVersion.NewVersion => Tuple.Create(
                    new XDocument(xd,
                                  new XElement(MedoConst.Xdms271 + MedoTag.TagCommunication, new XAttribute(XNamespace.Xmlns + "xdms", MedoConst.Xdms271),
                                               new XAttribute(MedoConst.Xdms271 + MedoTag.TagVersion, MedoConst.Version271),
                                               new XElement(MedoConst.Xdms271 + MedoTag.TagHeader, new XAttribute(MedoConst.Xdms271 + MedoTag.TagMesType, MedoMessageType.Acknowledgment.GetDescription()),
                                                            new XAttribute(MedoConst.Xdms271 + MedoTag.TagMesUid, respId), new XAttribute(MedoConst.Xdms271 + MedoTag.TagCreated, date),
                                                            new XElement(MedoConst.Xdms271 + MedoTag.TagSource, new XElement(MedoConst.Xdms271 + MedoTag.TagComOrg, MedoConst.RBName),
                                                                         new XAttribute(MedoConst.Xdms271 + MedoTag.TagOrgUid, MedoConst.RBMedoID))),
                                               new XElement(MedoConst.Xdms271 + MedoTag.TagAcknowledgment, new XAttribute(MedoConst.Xdms271 + MedoTag.TagResponseUid, messageId),
                                                            new XElement(MedoConst.Xdms271 + MedoTag.TagTime, date), new XElement(MedoConst.Xdms271 + MedoTag.TagAccepted, accept),
                                                            comment.IsNullOrEmpty() ? null : new XElement(MedoConst.Xdms271 + MedoTag.TagComment, comment)),
                                      new XElement(MedoConst.Xdms271 + MedoTag.TagDeliveryIndex, new XElement(MedoConst.Xdms271 + MedoTag.TagDestination,
                                                       new XElement(MedoConst.Xdms271 + MedoTag.TagDestination, new XAttribute(MedoConst.Xdms271 + MedoTag.TagUid, destId),
                                                       new XElement(MedoConst.Xdms271 + MedoTag.TagOrganization, destName)))))), respId),
                _ => throw new ArgumentOutOfRangeException(nameof(xsdVersion), xsdVersion, null)
            };

            return result;
        }
    }
}
