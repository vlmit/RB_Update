using System;
using System.Xml.Linq;

namespace Tessa.Extensions.Shared.Helpers.Medo
{
    public static class MedoConst
    {
        #region Sql command

        public const string GetLinkOrg = "SELECT \"rp\".\"Name\", \"rp\".\"LegalAddress\", \"rp\".\"Phone\", \"rp\".\"Email\", \"rp\".\"MedoID\" " +
                    "FROM \"DocumentCommonInfo\" AS \"drc\" " +
                    "INNER JOIN \"Partners\" AS \"rp\" ON \"drc\".\"PartnerID\" = \"rp\".\"ID\" " +
                    //"INNER JOIN \"PartnerMedoInfo\" AS \"m\" ON \"rp\".\"ID\" = \"m\".\"ID\" " +
                    "WHERE \"drc\".\"ID\" = @CardID";

        public const string GetOrg = "SELECT \"rp\".\"Name\", \"rp\".\"LegalAddress\", \"rp\".\"Phone\", \"rp\".\"Email\", \"rp\".\"MedoID\" " +
                                        "FROM \"Partners\" AS \"rp\"   " +
                                       // "INNER JOIN \"RostehPartnerMedoInfo\" AS \"m\"   ON \"rp\".\"ID\" = \"m\".\"ID\" " +
                                        "WHERE \"rp\".\"ID\" = @CardID";

        public const string GetPersonData = "SELECT \"p\".\"Position\", \"p\".\"Phone\", \"p\".\"Email\", \"p\".\"FullName\" " +
                                                "FROM \"PersonalRoles\" AS \"p\" " +
                                                "WHERE \"p\".\"ID\" = @id";

        public const string GetAttachment = "SELECT \"f\".\"ID\", \"f\".\"RowID\", \"f\".\"Name\", \"f\".\"VersionRowID\" " +
                                                "FROM  \"Files\" AS \"f\" " +
                                                "WHERE \"f\".\"ID\" = @CardID AND \"f\".\"CategoryID\" = @Category";

        public const string InsertMedoInfo = "INSERT INTO \"MedoCommonInfo\" " +
            "(\"ID\", \"RowID\", \"MessageID\", \"DateMessage\", " +
            "\"MedoStatusID\", \"MedoStatusName\", " +
            //"\"MedoTypeID\", \"MedoTypeName\", " +
            "\"MedoComment\", \"MedoError\", \"MedoPartnerID\", \"MedoPartnerFullName\")" +
            "VALUES(@cID, @rowID, @mID, @date, " +
            "@state, @stName, " +
            //"@typeID, @typeName, " +
            "@comment, @error, @partnerID, @partnerName)";

        public const string GetSignStampInfo = "SELECT \"m\".\"Page\", \"m\".\"X\", \"m\".\"Y\", \"m\".\"Width\", \"m\".\"Height\" " +
            "FROM \"MedoStampInfo\" AS \"m\" " +
            "WHERE \"m\".\"ID\" = @CardID AND \"m\".\"StampTypeID\" = @type AND \"m\".\"FileSignaturesRowID\" = @sign LIMIT 1";

        public const string GetAllSignStampsInfo = "SELECT \"m\".\"Page\", \"m\".\"X\", \"m\".\"Y\", \"m\".\"Width\", \"m\".\"Height\" " +
            "FROM \"MedoStampInfo\" AS \"m\" " +
            "WHERE \"m\".\"ID\" = @CardID AND \"m\".\"StampTypeID\" = @type AND \"m\".\"FileSignaturesRowID\" = @sign";

        public const string GetRegStampInfo = "SELECT \"m\".\"Page\", \"m\".\"X\", \"m\".\"Y\", \"m\".\"Width\", \"m\".\"Height\" FROM \"MedoStampInfo\" AS \"m\" WHERE \"m\".\"ID\" = @CardID AND \"m\".\"StampTypeID\" = @type LIMIT 1";

        #endregion

        #region Configuration

        /// <summary>
        ///     глобальная часть пути к папке с временными файлами (из app.json)
        /// </summary>
        public const string GlobalInPathSettingName = "MedoSendPackagePlugin.GlobalInPath";

        /// <summary>
        ///     глобальная часть пути к папке с временными файлами (из app.json)
        /// </summary>
        public const string GlobalOutPathSettingName = "MedoSendPackagePlugin.GlobalOutPath";

        /// <summary>
        ///     глобальная часть пути к папке с временными файлами (из app.json)
        /// </summary>
        public const string TempPathSettingName = "MedoSendPackagePlugin.TempPath";

        /// <summary>
        ///     расположение Container xsd (из app.json)
        /// </summary>
        public const string ContainerSettingName = "MedoSendPackagePlugin.Container";

        /// <summary>
        ///     расположение Communication xsd (из app.json)
        /// </summary>
        public const string CommunicationSettingName = "MedoSendPackagePlugin.Communication";
        
        /// <summary>
        ///     расположение Container 2.7.1 xsd (из app.json)
        /// </summary>
        public const string ContainerSettingName271 = "MedoSendPackagePlugin.Container.2.7.1";

        /// <summary>
        ///     расположение Communication 2.7.1 xsd (из app.json)
        /// </summary>
        public const string CommunicationSettingName271 = "MedoSendPackagePlugin.Communication.2.7.1";

        /// <summary>
        ///     расположение подписи организации (из app.json)
        /// </summary>
        public const string OrgSignNumber = "MedoSendPackagePlugin.OrgSignNumber";

        /// <summary>
        ///     расположение изображения для штампа регистрации (из app.json)
        /// </summary>
        public const string BitmapStampPath = "Cds.Stamp";

        #endregion

        #region Const names

        /// <summary>
        /// наименование xml с описанием контейнера МЭДО (название постояное по документации)
        /// </summary>
        public const string PassportXmlName = "passport.xml";

        /// <summary>
        ///     имя png с регистрационным штампом
        /// </summary>
        public const string RegStampName = "regStamp.png";

        /// <summary>
        ///     имя png с основой штампа подписи
        /// </summary>
        public const string StampName = "stamp.png";

        /// <summary>
        ///     наименование главного документа
        /// </summary>
        public const string MainDocName = "document.pdf";

        /// <summary>
        /// наименование подписи контейнера
        /// </summary>
        public const string ContainerSign = "containerSign.p7s";

        /// <summary>
        ///     версия 2.7 xsd схемы
        /// </summary>
        public const string Version27 = "2.7";

        /// <summary>
        ///     версия 2.7.1 xsd схемы
        /// </summary>
        public const string Version271 = "2.7.1";

        /// <summary>
        ///     тип контеера
        /// </summary>
        public const string ContainerType = "Документ в электронном виде";

        #endregion

        #region RB info

        /// <summary>
        ///     id ростех в мэдо
        /// </summary>
        //public static readonly Guid RostehMedoID = new(0x4dad02b0, 0x979f, 0x4bce, 0x9c, 0x23, 0x84, 0x4c, 0xc5, 0xe8, 0x36, 0xe5);
        public static readonly Guid RBMedoID = new(0x2051cd0a, 0xc28f, 0x4286, 0xad, 0x42, 0x48, 0x82, 0x02, 0xd5, 0x68, 0x9b);
        //2051cd0a- c28f- 4286- ad 42 -48 82 02 d5 68 9b

        /// <summary>
        ///     id ростех в tessa
        /// </summary>
        //public static readonly Guid RostehTessaID = new(0xb3a0caaa, 0xdd05, 0x44a5, 0x84, 0xba, 0xd5, 0x12, 0xac, 0x90, 0xb8, 0x96); // b3a0caaa-dd05-44a5-84ba-d512ac90b896;
        //public static readonly Guid RostehTessaID = new(0xb3a0caaa, 0xdd05, 0x44a5, 0x84, 0xba, 0xd5, 0x12, 0xac, 0x90, 0xb8, 0x96); // b3a0caaa-dd05-44a5-84ba-d512ac90b896;
        //2051cd0a-c28f-4286-ad42-488202d5689b

        /// <summary>
        ///     имя ростеха в мэдо
        /// </summary>
        public const string RBName = "Администрация Главы и Правительства Республики Бурятия";

        #endregion

        #region XML

        public static readonly XDeclaration Declare = new("1.0", "utf-8", null);

        public static readonly XNamespace Xdms27 = "http://www.infpres.com/IEDMS";

        public static readonly XNamespace Ns27 = "http://minsvyaz.ru/container";
        //public static readonly XNamespace Ns27 = "http://www.infpres.com/IEDMS";


        public static readonly XNamespace Xdms271 = "urn:IEDMS:MESSAGE";

        public static readonly XNamespace Ns271 = "urn:IEDMS:CONTAINER";

        #endregion
    }
}
