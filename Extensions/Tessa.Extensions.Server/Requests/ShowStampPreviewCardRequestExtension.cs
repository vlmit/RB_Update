using LinqToDB.Common;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
//using Tessa.Extensions.Shared.Extensions;
using Tessa.Extensions.Shared.Helpers.Medo;
using Tessa.Extensions.Shared.Info;
using Tessa.Files;
using Tessa.PdfSharp.Drawing;
using Tessa.PdfSharp.Drawing.BarCodes;
using Tessa.PdfSharp.Drawing.Layout;
using Tessa.PdfSharp.Pdf;
using Tessa.PdfSharp.Pdf.IO;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using StampType = Tessa.Extensions.Shared.Helpers.Medo.StampType;

namespace Tessa.Extensions.Server.Requests
{
    public sealed class ShowStampPreviewCardRequestExtension : CardRequestExtension
    {
        #region Fields

        private readonly ICardServerPermissionsProvider permissionsProvider;
        private readonly ICardStreamServerRepository cardStreamRepository;
        private readonly ICardFileManager fileManager;
        private readonly ICardRepository cardRepository;

        //private List<MedoStampInfo> stampInfoList;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructor

        public ShowStampPreviewCardRequestExtension(ICardServerPermissionsProvider permissionsProvider,
            ICardStreamServerRepository cardStreamRepository,
            ICardFileManager fileManager,
            ICardRepository cardRepository)
        {
            logger.Info("ShowStampPreviewCardRequestExtension constructor");
            

            this.permissionsProvider = permissionsProvider;
            this.cardStreamRepository = cardStreamRepository;
            this.fileManager = fileManager;
            this.cardRepository = cardRepository;
        }

        #endregion

        #region Base Overrides

        public override async Task AfterRequest(ICardRequestExtensionContext context)
        {
            logger.Info("ShowStampPreviewCardRequestExtension AfterRequest");
            
            var cardID = context.Request.Info.TryGet<Guid>("cardID");
            var fileID = context.Request.Info.TryGet<Guid>("fileID");
            var fileName = context.Request.Info.TryGet<string>("fileName");
            var fileVersion = context.Request.Info.TryGet<Guid>("fileVersion");
            var dbScope = context.DbScope;
            var file = await this.GetFileContentAsync(context.ValidationResult, fileID, fileName, fileVersion, cardID);
            var stampInfo = context.Request.Info.TryGet<Dictionary<string, object>>("stampInfo");
            var stampInfoList = await GetMedoStampInfo(stampInfo);
            var withGrid = context.Request.Info.Get<bool>("withGrid");

            await using var stream = await file.GetContentOrThrowAsync();
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            using var pdfFile = PdfReader.Open(memoryStream, PdfDocumentOpenMode.Modify);

            var request = new CardGetRequest
            {
                CardID = cardID,
                RestrictionFlags = CardGetRestrictionFlags.RestrictTaskHistory | CardGetRestrictionFlags.RestrictTasks,
                GetMode = CardGetMode.ReadOnly
            };
            this.permissionsProvider.SetFullPermissions(request);

            var response = await this.cardRepository.GetAsync(request, context.CancellationToken);
            var result = response.ValidationResult.Build();
            if (!result.IsSuccessful)
            {
                context.ValidationResult.Add(result);
                return;
            }
            var card = response.Card;

            await this.CreateRegStampsAsync(card, pdfFile, dbScope, withGrid, stampInfoList,  context.ValidationResult, context.CancellationToken);

            var signatures = await GetAllSignaturesAsync(card, context.CancellationToken);
            await this.CreateAllSignStampsAsync(dbScope, context.ValidationResult,
                pdfFile, cardID, signatures, context.CancellationToken);

            await using (var resultFile = new MemoryStream())
            {
                pdfFile.Save(resultFile);
                context.Response.Info["Content"] = resultFile.ToArray();
            }
        }

        private async Task<List<MedoStampInfo>> GetMedoStampInfo(Dictionary<string, object> stampInfo)
        {
            logger.Info("GetMedoStampInfo");
            List<MedoStampInfo> stampInfoList = new();
            stampInfo.Select(x => x.Value)
                .Cast<Dictionary<string, object>>()
                .ForEach(d =>
                {
                    stampInfoList.Add(new MedoStampInfo
                    {
                        Page = (int)d["Page"],
                        X = (int)d["X"],
                        Y = (int)d["Y"],
                        Height = (int)d["Height"],
                        Width = (int)d["Width"],
                        StampTypeID = (int)d["StampTypeID"]
                        //FileSignaturesRowID = d["FileSignaturesRowID"] != null ? (Guid)d["FileSignaturesRowID"] : null
                    });
                });

            foreach(var x in stampInfoList)
            {
                logger.Info($"{x.Page} {x.X} {x.Y} {x.Height} {x.Width} {x.StampTypeID} {x.FileSignaturesRowID}");
            }

            return stampInfoList;
        }

        #endregion

        #region Private

        /// <summary>
        /// Отрисовка штампа регистрации
        /// </summary>
        /// <param name="card"></param>
        /// <param name="dbScope"></param>
        /// <param name="validationResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CreateRegStampsAsync(Card card
            , PdfDocument pdfFile
            , IDbScope dbScope
            , bool withGrid
            , List<MedoStampInfo> stampInfoList
            , IValidationResultBuilder validationResult
            , CancellationToken cancellationToken)
        {
            logger.Info("ShowStampPreviewCardRequestExtension CreateRegStampsAsync");
            
            //System.Diagnostics.Debugger.Launch();
            var cardFullNumber = card.Sections.TryGet(SchemeInfo.DocumentCommonInfo)?.Fields.TryGet<string>(SchemeInfo.DocumentCommonInfo.FullNumber);
            var cardRegDate = card.Sections.TryGet(SchemeInfo.DocumentCommonInfo)?.Fields.TryGet<DateTime>(SchemeInfo.DocumentCommonInfo.DocDate);

            //var regPosInfo = await MedoHelper.GetStampDataAsync(dbScope, validationResult, StampType.Reg,
            //            MedoConst.GetRegStampInfo, card.ID, cancellationToken: cancellationToken);

            var regPosInfo = stampInfoList.Where(x => x.StampTypeID.Equals((int)StampType.Reg)).FirstOrDefault();

            var regInfo = string.Concat(cardFullNumber, "       ", cardRegDate?.ToString("dd.MM.yyyy"));

            var pageNumber = regPosInfo.Page;
            if (pdfFile.Pages.Count < pageNumber)
            {
                validationResult.Add(
                    ValidationResult.FromText(this,
                    $"Некорректный номер страницы. Переданное значение: {pageNumber}, страниц в документе: {pdfFile.Pages.Count}.",
                    ValidationResultType.Error));
                return;
            }

            await using (var regStamp = this.CreateRegStamp(regInfo, validationResult))
            {
                DrawStamp(pdfFile, regPosInfo, regStamp);
                if (withGrid)
                {
                    //await DrawGrid(pdfFile);
                }
            }
        }

        /// <summary>
        ///     отрисовка всех штампов подписи
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="pdfFile">основной файл</param>
        /// <param name="cardID">id карточки</param>
        /// <param name="fileID">id файла</param>
        private async Task CreateAllSignStampsAsync(IDbScope dbScope,
            IValidationResultBuilder validationResult,
            PdfDocument pdfFile,
            Guid cardID,
            IFileSignatureCollection signatures,
            CancellationToken cancellationToken)
        {
            logger.Info("ShowStampPreviewCardRequestExtension CreateAllSignStampsAsync");
            
            if (signatures?.Count > 0)
            {
                var signStamps = await this.GetAllSignStampsInfoAsync(dbScope, cardID);

                foreach (var signStampItem in signStamps)
                {
                    if (signStampItem.StampTypeID != (int)StampType.Sign)
                    {
                        continue;
                    }

                    //var signPosInfo = stampInfoList.Where(x => x.StampTypeID.Equals(StampType.Sign)).FirstOrDefault();

                    var signature = signatures[(Guid)signStampItem.FileSignaturesRowID];

                    //if (signPosInfo is null)
                    //{
                    //    validationResult.AddError(this, $"Can't get position for sign {signature.ID}");

                    //    logger.Info($"ShowStampPreviewCardRequestExtension CreateAllSignStampAsync Can't get position for sign {signature.ID}");

                    //    return;
                    //}

                    var signName = signature.ID.ToString().Replace("-", "") + ".png";

                    await using var signStamp = await this.CreateSignStamp(signature, validationResult, cancellationToken);
                    if (signStamp is null)
                    {
                        validationResult.AddError(this, $"Sign stamp is null {signature.ID}");
                        return;
                    }

                    DrawStamp(pdfFile, signStampItem, signStamp);
                }


                //foreach (var signature in signatures)
                //{
                //    var signPosInfo = await MedoHelper.GetStampDataAsync(dbScope, validationResult, StampType.Sign,
                //        MedoConst.GetSignStampInfo, cardID, signature.ID, cancellationToken);

                //    if (signPosInfo.IsNullOrEmpty())
                //    {
                //        validationResult.AddError(this, $"Cant get position for sign {signature.ID}");
                //        return;
                //    }

                //    await using var signStamp = await this.CreateSignStamp(signature, validationResult, cancellationToken);
                //    if (signStamp is null)
                //    {
                //        validationResult.AddError(this, $"Sign stamp is null {signature.ID}");
                //        return;
                //    }
                //    DrawStamp(pdfFile, signPosInfo, signStamp);
                //}

                return;
            }

            validationResult.AddError(this, $"Cant get main file signatures, card id; {cardID}");
        }

        /// <summary>
        ///     получаем контент файла
        /// </summary>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="fileVersion"></param>
        /// <param name="cardID">id карточки</param>
        /// <param name="fileID">id файла</param>
        /// <param name="fileName">имя файла</param>
        private async Task<ICardFileContentResult> GetFileContentAsync(
            IValidationResultBuilder validationResult,
            Guid fileID,
            string fileName,
            Guid fileVersion,
            Guid cardID)
        {
            logger.Info("ShowStampPreviewCardRequestExtension GetFileContentAsync");
            
            var contentRequest = new CardGetFileContentRequest
            {
                CardID = cardID,
                FileID = fileID,
                FileName = fileName,
                VersionRowID = fileVersion
            };

            this.permissionsProvider.SetFullPermissions(contentRequest);
            contentRequest.SetForbidStoringHistory(true);

            var contentResult = await this.cardStreamRepository.GetFileContentAsync(contentRequest);
            var contentResponse = contentResult.Response;
            if (!contentResponse.HasContent)
            {
                validationResult.Add(contentResponse.ValidationResult);
                return null;
            }

            return contentResult;
        }

        /// <summary>
        ///     создаем штамп регистрации
        /// </summary>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="info">текст штампа</param>
        /// <returns></returns>
        private MemoryStream CreateRegStamp(string info, IValidationResultBuilder validationResult)
        {
            logger.Info("ShowStampPreviewCardRequestExtension CreateRegStamp");
            
            using var font = new Font("Times New Roman", 14, FontStyle.Regular);
            using var bm = new Bitmap(1, 1);
            using var graphics = Graphics.FromImage(bm);
            var size = graphics.MeasureString(info, font);

            using var bitmap = new Bitmap((int)size.Width, (int)size.Height);
            try
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    using (var sb = new SolidBrush(Color.White))
                    {
                        g.FillRectangle(sb, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }

                    g.DrawString(info, font, Brushes.Black, PointF.Empty);
                }

                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                return ms;
            }
            catch (Exception e)
            {
                validationResult.AddError(this, e.Message);
                return null;
            }
        }

        /// <summary>
        ///     создаем штамп подписи
        /// </summary>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="info">текст штампа</param>
        /// <returns></returns>
        private async Task<MemoryStream> CreateSignStamp(IFileSignature signature,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            logger.Info("ShowStampPreviewCardRequestExtension CreateSignStamp");
            
            //var bitmapPath = ConfigurationManager.Settings.TryGet<string>(MedoConst.BitmapStampPath);

            var bitmapPath = "C:\\home\\tessa\\tessa\\web\\stamp11.png";
            bitmapPath = "/home/tessa/tessa/web/stamp11.png";

            if (bitmapPath.IsNullOrEmpty())
            {
                validationResult.AddError(this, "Не удается получить расположение изображения штампа подписи");
                logger.Info("ShowStampPreviewCardRequestExtension CreateSignStamp \"Не удается получить расположение изображения штампа подписи\"");
                return null;
            }

            logger.Info(bitmapPath);

            using var bitmap = new Bitmap(bitmapPath);
            var (notBefore, notAfter) = await GetSignatureStampInfoAsync(signature, validationResult, cancellationToken);
            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                using (var font = new Font("Verdana", 24, FontStyle.Regular))
                {
                    graphics.DrawString("Сертификат:", font, Brushes.Black, 45, 350);
                    graphics.DrawString(signature.SerialNumber, font, Brushes.Black, 305, 350);
                    graphics.DrawString("Владелец:", font, Brushes.Black, 45, 400);
                    graphics.DrawString(signature.SubjectName, font, Brushes.Black, 305, 400);
                    graphics.DrawString("Действителен", font, Brushes.Black, 45, 450);
                    graphics.DrawString($"с {notBefore ?? DateTime.UtcNow:dd.MM.yyyy} по {notAfter ?? DateTime.UtcNow:dd.MM.yyyy}", font, Brushes.Black, 305, 450);
                }

                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                return ms;
            }
            catch (Exception e)
            {
                validationResult.AddError(this, e.Message);
                return null;
            }
        }

        /// <summary>
        ///     отрисовка штампа
        /// </summary>
        /// <param name="pdfFile">основной файл</param>
        /// <param name="stampInfo">Инфо о расположнеии штампа</param>
        /// <param name="ms">штамп</param>
        private static void DrawStamp(PdfDocument pdfFile,
            MedoStampInfo medoStampInfo,
            MemoryStream ms)
        {
            logger.Info("ShowStampPreviewCardRequestExtension DrawStamp");
            
            var pageNumber = medoStampInfo.Page - 1;
            pageNumber = pageNumber >= pdfFile.PageCount ? pdfFile.PageCount - 1 : pageNumber;
            var pdfPage = pdfFile.Pages[pageNumber];
            using var g = XGraphics.FromPdfPage(pdfPage, XGraphicsUnit.Millimeter);
            using var image = XImage.FromStream(() => ms);

            //string logs = DateTime.Now.ToString("G") + " CreateRegStampsAsync OK";

            StringBuilder sb = new StringBuilder();

           
            g.DrawImage(image, medoStampInfo.X, medoStampInfo.Y, medoStampInfo.Width, medoStampInfo.Height);
        }

        /// <summary>
        /// Все подписи основного файла
        /// </summary>
        /// <param name="card"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<IFileSignatureCollection> GetAllSignaturesAsync(Card card, CancellationToken cancellationToken = default)
        {
            logger.Info("ShowStampPreviewCardRequestExtension GetAllSignaturesAsync");
            
            await using var container = await this.fileManager.CreateContainerAsync(card, cancellationToken: cancellationToken);
            var file = card.Files.FirstOrDefault(x => x.CategoryID == FileCategories.MainDoc.ID);
            var lastVersion = container.FileContainer.Files.TryGet(file.RowID).Versions.Last;
            return (await lastVersion.Source.GetSignaturesAsync(lastVersion, FileSignatureLoadingMode.WithData, cancellationToken))?.Signatures;
        }

        /// <summary>
        /// Возвращает даты действия сертификата
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="validationResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<(DateTime?, DateTime?)> GetSignatureStampInfoAsync(
            IFileSignature signature,
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default)
        {
            logger.Info("ShowStampPreviewCardRequestExtension GetSignatureStampInfoAsync");
            
            var contentInfo = new ContentInfo(await signature.Data.GetBytesAsync(cancellationToken));
            var signedCms = new SignedCms(contentInfo);

            try
            {
                signedCms.Decode(signedCms.ContentInfo.Content);
            }
            catch (CryptographicException ex)
            {
                validationResult.AddException(this, ex);
                return (null, null);
            }

            X509Certificate2 certificateClient = null;

            for (var index = signedCms.Certificates.Count - 1; index >= 0; index--)
            {
                var cert = signedCms.Certificates[index];

                if (!string.Equals(cert.SerialNumber, signature.SerialNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                certificateClient = cert;
                break;
            }

            if (certificateClient is null)
            {
                throw new ArgumentNullException(nameof(X509Certificate2));
            }

            return (certificateClient.NotBefore, certificateClient.NotAfter);
        }

        private async Task<List<MedoStampInfo>> GetAllSignStampsInfoAsync(IDbScope dbScope, Guid CardID)
        {
            await using (dbScope.Create())
            {
                // работа в пределах одного SQL-соединения, транзакция при этом явно не создаётся

                var db = dbScope.Db;

                var builderFactory = dbScope.BuilderFactory;

                var result = await db.SetCommand(
                        builderFactory
                            .Select().C("MedoStampInfo", "Page", "X", "Y", "Width", "Height", "typeID", "FileSignaturesRowID")
                            .From("MedoStampInfo").NoLock()
                            .Where().C("ID").Equals().P("cardID")
                            //.And().C("StampTypeID").Equals().P("stampTypeId")
                            .Build(),
                        db.Parameter("cardID", CardID))
                    //    db.Parameter("stampTypeId", StampType.Sign))
                    .LogCommand()
                    .ExecuteListAsync<MedoStampInfo>();

                if (result == null || result.Count == 0)
                {
                    logger.Info($"GetAllStampsInfo no elements cardID: {CardID}");
                }

                foreach (var item in result)
                {
                    logger.Info($"Page: {item.Page}\nX: {item.X}\nY: {item.Y}\nStampTypeID: {item.StampTypeID}\nFileSignaturesRowID: {item.FileSignaturesRowID}");
                    if (item.FileSignaturesRowID != null)
                    {
                        item.StampTypeID = 1;
                    }
                    logger.Info($"Page: {item.Page}\nX: {item.X}\nY: {item.Y}\nStampTypeID: {item.StampTypeID}\nFileSignaturesRowID: {item.FileSignaturesRowID}");
                }

                return result;
            }
        }

        private Dictionary<string, int?> ConvertStampItemToDict(MedoStampInfo signStamp)
        {
            Dictionary<string, int?> signPosInfo = new Dictionary<string, int?>
            {
                { MedoTag.TagPage, signStamp.Page },
                { MedoTag.TagX, signStamp.X },
                { MedoTag.TagY, signStamp.Y },
                { MedoTag.TagW, signStamp.Width },
                { MedoTag.TagH, signStamp.Height }
            };

            return signPosInfo;
        }

        private async Task DrawGrid(PdfDocument doc)
        {
            var pages = doc.Pages;

            foreach (var page in pages)
            {
                XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Millimeter);
                var linePen = new XPen(XColors.LightGray, 0.5);
                linePen.DashStyle = XDashStyle.Dot;

                var height = Convert.ToInt32(page.Height.Value);
                var width = Convert.ToInt32(page.Width.Value);

                var heightPoint = Convert.ToInt32(page.Height.Millimeter);
                var widthPoint = Convert.ToInt32(page.Width.Millimeter);

                List<LineObject> lineObjects = await GetLineObjects(height, width);

                lineObjects.ForEach(async x =>
                {
                    gfx.DrawLine(linePen, x.StartPoint, x.EndPoint);
                    await GetLineDigits(gfx, x.StartPoint, x.step, x.lineType);
                });
            }
        }

        private async Task<List<LineObject>> GetLineObjects(int height, int width)
        {
            List<LineObject> lineObjects = new();
            int step = 10;

            for (int i = 10; i <= width; i += step)
            {
                Point startPoint = new(i, 0);
                Point endPoint = new Point(i, height);
                lineObjects.Add(new LineObject(startPoint, endPoint, i, LineType.Horizontal));
            }

            for (int i = 10; i <= height; i += step)
            {
                Point startPoint = new(0, i);
                Point endPoint = new(height, i);
                lineObjects.Add(new LineObject(startPoint, endPoint, i, LineType.Vertical));
            }

            return lineObjects;
        }

        private async Task GetLineDigits(XGraphics gfx, Point startPoint, int step, LineType lineType)
        {
            XFont font = new("Times New Roman", 3, XFontStyle.Regular);
            XTextFormatter tf = new(gfx);

            XRect rect;
            if (lineType == LineType.Horizontal)
            {
                rect = new XRect(startPoint.X, startPoint.Y + 1, 5, 5);
            }
            else
            {
                rect = new XRect(startPoint.X + 1, startPoint.Y, 5, 5);
            }
            //XRect rect = new(startPoint.X + 3, startPoint.Y - 5, 5, 5);
            gfx.DrawRectangle(XBrushes.Transparent, rect);
            tf.DrawString(step.ToString(), font, XBrushes.Red, rect, XStringFormats.TopLeft);
        }

        #endregion

        private record LineObject(Point StartPoint, Point EndPoint, int step, LineType lineType);
    }

    public class MedoStampInfo
    {
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int StampTypeID { get; set; }
        public Guid? FileSignaturesRowID { get; set; }
    }

    enum LineType
    {
        Horizontal,
        Vertical
    }
}