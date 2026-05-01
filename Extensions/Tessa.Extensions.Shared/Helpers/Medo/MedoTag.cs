namespace Tessa.Extensions.Shared.Helpers.Medo
{
    public static class MedoTag
    {
        #region Passport

        /// <summary>
        /// корневой элемент контейнера
        /// </summary>
        public const string TagContainer = "container";

        /// <summary>
        /// основные реквизиты документа
        /// </summary>
        public const string TagRequisites = "requisites";

        /// <summary>
        /// Вид документа
        /// </summary>
        public const string TagDocumentKind = "documentKind";

        /// <summary>
        /// Место составления (издания) документа
        /// </summary>
        public const string TagDocumentPlace = "documentPlace";

        /// <summary>
        /// Гриф доступа к документу
        /// </summary>
        public const string TagClassification = "classification";

        /// <summary>
        /// Краткое содержание (наименование, аннотация) документа
        /// </summary>
        public const string TagAnnotation = "annotation";

        /// <summary>
        /// Локальный идентификатор вида документа в СЭД отправителя или получателя.
        /// </summary>
        public const string TagId = "id";

        /// <summary>
        /// Ссылка на исходящий номер и дату документа адресата (исходящий номер и дата документа, в ответ на который направляются материалы)
        /// </summary>
        public const string TagLinks = "links";

        /// <summary>
        /// Ссылка на исходящий номер и дату документа адресата (исходящий номер и дата документа, в ответ на который направляются материалы)
        /// </summary>
        public const string TagLink = "link";

        /// <summary>
        /// Информация об организации - авторе связанного документа. Указывать описание комплексного типа содержимого "organization"
        /// </summary>
        public const string TagOrganization = "organization";

        /// <summary>
        /// Наименование организации**
        /// </summary>
        public const string TagTitle = "title";

        /// <summary>
        /// Почтовый адрес
        /// </summary>
        public const string TagAddress = "address";

        /// <summary>
        /// Телефонный номер
        /// </summary>
        public const string TagPhone = "phone";

        /// <summary>
        /// Официальный адрес электронной почты
        /// </summary>
        public const string TagEmail = "email";

        /// <summary>
        /// Данные регистрации документа в организации
        /// </summary>
        public const string TagRegistration = "registration";

        /// <summary>
        /// Регистрационный номер документа
        /// </summary>
        public const string TagNumber = "number";

        /// <summary>
        /// Дата документа
        /// </summary>
        public const string TagDate = "date";

        /// <summary>
        /// Уникальный идентификатор документа
        /// </summary>
        public const string TagUid = "uid";

        /// <summary>
        /// Уникальный идентификатор связанного документа
        /// </summary>
        public const string TagLinkUid = "uid";

        /// <summary>
        /// Информация о графических элементах визуализации регистрационных данных и данных ЭП
        /// </summary>
        public const string TagRegStamp = "registrationStamp";

        /// <summary>
        /// Информация о графических элементах визуализации регистрационных данных и данных ЭП
        /// </summary>
        public const string TagSignStamp = "signatureStamp";

        /// <summary>
        /// Данные о расположении графических элементов
        /// </summary>
        public const string TagPosition = "position";

        /// <summary>
        /// Номер страницы (начиная с 1), на которой должны быть помещены графические элементы
        /// </summary>
        public const string TagPage = "page";

        /// <summary>
        /// Отступ по горизонтали от верхнего левого угла страницы, в мм
        /// </summary>
        public const string TagTopLeft = "topLeft";

        /// <summary>
        /// Отступ по горизонтали от верхнего левого угла страницы, в мм
        /// </summary>
        public const string TagX = "x";

        /// <summary>
        /// Отступ по вертикали от верхнего левого угла страницы, в мм
        /// </summary>
        public const string TagY = "y";

        /// <summary>
        /// Размер изображения графических элементов
        /// </summary>
        public const string TagDimension = "dimension";

        /// <summary>
        /// Ширина изображения, в мм
        /// </summary>
        public const string TagW = "w";

        /// <summary>
        /// Высота изображения, в мм
        /// </summary>
        public const string TagH = "h";

        /// <summary>
        /// Имя файла графических элементов в транспортном контейнере
        /// </summary>
        public const string TagLocalName = "localName";

        /// <summary>
        /// Авторы документа (письма)
        /// </summary>
        public const string TagAuthors = "authors";

        /// <summary>
        /// Информация об авторе документа. Если документ подписан в нескольких организациях, первым в последовательности должен быть указан автор, являющийся отправителем документа.
        /// </summary>
        public const string TagAuthor = "author";

        /// <summary>
        /// Информация о подписи документа
        /// </summary>
        public const string TagSign = "sign";

        /// <summary>
        /// Лицо, подписавшее документ в организации
        /// </summary>
        public const string TagPerson = "person";

        /// <summary>
        /// Описание электронной подписи файла документа (//container/document/@local Name), сформированной данным лицом.
        /// </summary>
        public const string TagDocumentSignature = "documentSignature";

        /// <summary>
        /// Должность лица
        /// </summary>
        public const string TagPost = "post";

        /// <summary>
        /// Фамилия, Имя, Отчество (при наличии) лица, полностью, в именительном падеже
        /// </summary>
        public const string TagName = "name";

        /// <summary>
        /// Телефонный номер лица
        /// </summary>
        public const string TagPersonPhone = "phone";

        /// <summary>
        /// Адрес электронной почты лица
        /// </summary>
        public const string TagPersonEmail = "email";

        /// <summary>
        /// Тип подписи (визирующая или утверждающая (по умолчанию))
        /// </summary>
        public const string TagType = "type";

        /// <summary>
        /// Адресаты
        /// </summary>
        public const string TagAddressees = "addressees";

        /// <summary>
        /// Адресат документа
        /// </summary>
        public const string TagAddressee = "addressee";

        /// <summary>
        /// Документ
        /// </summary>
        public const string TagDocument = "document";

        /// <summary>
        /// Приложения
        /// </summary>
        public const string TagAttachments = "attachments";

        /// <summary>
        /// Приложение
        /// </summary>
        public const string TagAttachment = "attachment";

        /// <summary>
        /// подпись приложения
        /// </summary>
        public const string TagAttSign = "signature";

        /// <summary>
        /// Номер приложения, начиная с 0
        /// </summary>
        public const string TagOrder = "order";

        /// <summary>
        /// Минимальная версия XSD схемы, согласно которой сформирован файл описания транспортного контейнера
        /// </summary>
        public const string TagVersion = "version";

        /// <summary>
        /// Количество страниц документа
        /// </summary>
        public const string TagPagesQuantity = "pagesQuantity";

        #endregion

        #region Communication

        /// <summary>
        /// Элемент паспорта 
        /// </summary>
        public const string TagCommunication = "communication";

        /// <summary>
        /// Заголовок сообщения
        /// </summary>
        public const string TagHeader = "header";

        /// <summary>
        /// Источник сообщения
        /// </summary>
        public const string TagSource = "source";

        /// <summary>
        /// Наименование организации
        /// </summary>
        public const string TagComOrg = "organization";

        /// <summary>
        /// Обязательный уникальный идентификатор контрагента МЭДО
        /// </summary>
        public const string TagOrgUid = "uid";

        /// <summary>
        /// Тип сообщения
        /// </summary>
        public const string TagMesType = "type";

        /// <summary>
        /// Уникальный идентификатор сообщения
        /// </summary>
        public const string TagMesUid = "uid";

        /// <summary>
        /// 
        /// </summary>
        public const string TagCreated = "created";

        /// <summary>
        /// Дата.время создания сообщения
        /// </summary>
        public const string TagShippingContainer = "container";

        /// <summary>
        /// Файл "архива", содержащий файлы "контейнера" документа в электронном виде
        /// </summary>
        public const string TagBody = "body";

        /// <summary>
        /// Необязательный атрибут, определяющий тип контейнера, по умолчанию "Документ в электронном виде"
        /// </summary>
        public const string TagContainerType = "type";

        /// <summary>
        /// Квитанция о приеме сообщения
        /// </summary>
        public const string TagAcknowledgment = "acknowledgment";

        /// <summary>
        /// Дата/время принятия сообщения
        /// </summary>
        public const string TagTime = "time";

        /// <summary>
        /// Признак того, что сообщение принято
        /// </summary>
        public const string TagAccepted = "accepted";

        /// <summary>
        /// Необязательные комментарии
        /// </summary>
        public const string TagComment = "comment";

        /// <summary>
        /// Идентификатор квитируемого сообщения (//communication/header@uid)
        /// </summary>
        public const string TagResponseUid = "uid";

        /// <summary>
        /// Уведомление
        /// </summary>
        public const string TagNotification = "notification";

        /// <summary>
        /// Тип уведомления
        /// </summary>
        public const string TagNoticeType = "type";

        /// <summary>
        /// Номер и дата документа
        /// </summary>
        public const string TagNum = "num";

        /// <summary>
        /// Событие: документ зарегистрирован
        /// </summary>
        public const string TagDocumentAccepted = "documentAccepted";

        /// <summary>
        /// Событие: отказано в регистрации
        /// </summary>
        public const string TagDocumentRefused = "documentRefused";

        /// <summary>
        /// Событие: назначен исполнитель
        /// </summary>
        public const string TagexecutorAssigned = "executorAssigned";

        /// <summary>
        /// Событие: Доклад по документу подготовлен (соответствующий документ передан на подпись)
        /// </summary>
        public const string TagreportPrepared = "reportPrepared";

        /// <summary>
        /// Событие: доклад по документу направлен (выпущен исходящий документ - ответ)
        /// </summary>
        public const string TagreportSent = "reportSent";

        /// <summary>
        /// Событие: Изменение хода исполнения
        /// </summary>
        public const string TagcourseChanged = "courseChanged";

        /// <summary>
        /// Событие: Опубликование документа
        /// </summary>
        public const string TagdocumentPublished = "documentPublished";

        /// <summary>
        /// Причина отказа в регистрации
        /// </summary>
        public const string TagReason = "reason";

        /// <summary>
        /// Подписавшие
        /// </summary>
        public const string TagSignatories = "signatories";

        /// <summary>
        /// Подписавший
        /// </summary>
        public const string TagSignatory = "signatory";

        /// <summary>
        /// Листов документа
        /// </summary>
        public const string TagPages = "pages";

        /// <summary>
        /// Список присоединенных файлов
        /// </summary>
        public const string TagFiles = "files";

        /// <summary>
        /// Присоединенный файл (с образом текста документа/приложения или данными), в случае передачи информации о подписании УКЭП, может содержать ссылки на дополнительные файлы.
        /// </summary>
        public const string TagFile = "file";

        /// <summary>
        /// Информация о подписаннии УКЭП присоединенного файла
        /// </summary>
        public const string TagSignedData = "signedData";

        /// <summary>
        /// Информация о подписавшем УКЭП файл
        /// </summary>
        public const string TagSignerInfo = "signerInfo";

        /// <summary>
        /// Имя файла формата PKCS#7, содержащий подпись и сертификат подписанта (detached SignedData)
        /// </summary>
        public const string TagSignature = "signature";

        public const string TagDeliveryIndex = "deliveryIndex";

        public const string TagDestination = "destination";

        public const string TagFoundation = "foundation";

        public const string TagRegion = "region";
        
        public const string TagExecutor = "executor";

        #endregion
    }
}