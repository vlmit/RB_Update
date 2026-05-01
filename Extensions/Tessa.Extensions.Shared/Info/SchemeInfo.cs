using System;

namespace Tessa.Extensions.Shared.Info
{
	public class SchemeInfo
	{
		/// <summary>
		/// Тип. Отправки МЭДО
		/// </summary>
		public static readonly Guid DeliveryTypes_MEDO_ID = new Guid("c1329507-99d6-4139-bedc-d351b065c683");



		public static DocumentCommonInfoInfo DocumentCommonInfo = new();

		public class DocumentCommonInfoInfo
		{
			private readonly string name = "DocumentCommonInfo";

			public readonly Guid SchemeTableID = new("{a161e289-2f99-4699-9e95-6e3336be8527}");

			/// <summary>
			///	Порядковый первичный номер документа.
			/// 
			/// Тип: Int64 Null
			/// </summary>
			public readonly string Number = nameof(Number);

			/// <summary>
			///	Количество страниц.
			/// 
			/// Тип: Int32 Null
			/// </summary>
			public readonly string SheetsCount = nameof(SheetsCount);

			public readonly string SheetsAttachmentsCount = nameof(SheetsAttachmentsCount);

			/// <summary>
			///	Текстовое отображение первичного номера документа (с префиксами и др.).
			/// 
			/// Тип: String(200) Null
			/// </summary>
			public readonly string FullNumber = nameof(FullNumber);

			/// <summary>
			///	Текстовое отображение первичного номера документа (с префиксами и др.).
			/// 
			/// Тип: String(200) Null
			/// </summary>
			public readonly string DocTypeMEDOIndex = nameof(DocTypeMEDOIndex);

			/// <summary>
			///	Текстовое отображение первичного номера документа (с префиксами и др.).
			/// 
			/// Тип: String(200) Null
			/// </summary>
			public readonly string DocTypeMEDOName = nameof(DocTypeMEDOName);

			/// <summary>
			///	Последовательность для первичного номера (поля Number и FullNumber)
			/// 
			/// Тип: String(128) Null
			/// </summary>
			public readonly string Sequence = nameof(Sequence);

			/// <summary>
			///	Порядковый вторичный номер документа.
			/// 
			/// Тип: Int64 Null
			/// </summary>
			public readonly string SecondaryNumber = nameof(SecondaryNumber);

			/// <summary>
			///	Текстовое отображение вторичный номера документа (с префиксами и др.).
			/// 
			/// Тип: String(64) Null
			/// </summary>
			public readonly string SecondaryFullNumber = nameof(SecondaryFullNumber);

			/// <summary>
			///	Последовательность для вторичного номера (поля SecondaryNumber и SecondaryFullNumber)
			/// 
			/// Тип: String(128) Null
			/// </summary>
			public readonly string SecondarySequence = nameof(SecondarySequence);

			/// <summary>
			///	Тема документа.
			/// 
			/// Тип: String(440) Null
			/// </summary>
			public readonly string Subject = nameof(Subject);

			/// <summary>
			///	Дата документа.
			/// 
			/// Тип: Date Null
			/// </summary>
			public readonly string DocDate = nameof(DocDate);

			/// <summary>
			///	Дата создания.
			/// 
			/// Тип: DateTime Null
			/// </summary>
			public readonly string CreationDate = nameof(CreationDate);

			/// <summary>
			///	Исходящий (внешний) номер документа / Номер контрагента
			/// 
			/// Тип: String(60) Null
			/// </summary>
			public readonly string OutgoingNumber = nameof(OutgoingNumber);

			/// <summary>
			///	Денежная сумма в документе.
			/// 
			/// Тип: Decimal(18, 2) Null
			/// </summary>
			public readonly string Amount = nameof(Amount);
			public readonly string Barcode = nameof(Barcode);
			public readonly string DepartmentName = nameof(DepartmentName);
			public readonly string RefDocDescription = nameof(RefDocDescription);

			public readonly string ID = nameof(ID);

			/// <summary>
			///	Тип документа для карточки, использующей типы документов
			/// </summary>
			public readonly string DocTypeID = nameof(DocTypeID);

			/// <summary>
			///	Тип документа для карточки, использующей типы документов
			/// </summary>
			public readonly string DocTypeTitle = nameof(DocTypeTitle);

			/// <summary>
			///	Карточка валюты документа. Может использоваться для денежной суммы в поле Amount.
			/// </summary>
			public readonly string CurrencyID = nameof(CurrencyID);

			/// <summary>
			///	Карточка валюты документа. Может использоваться для денежной суммы в поле Amount.
			/// </summary>
			public readonly string CurrencyName = nameof(CurrencyName);

			/// <summary>
			///	Автор документа.
			/// </summary>
			public readonly string AuthorID = nameof(AuthorID);

			/// <summary>
			///	Автор документа.
			/// </summary>
			public readonly string AuthorName = nameof(AuthorName);

			/// <summary>
			///	Регистратор.
			/// </summary>
			public readonly string RegistratorID = nameof(RegistratorID);

			/// <summary>
			///	Регистратор.
			/// </summary>
			public readonly string RegistratorName = nameof(RegistratorName);

			/// <summary>
			///	Подписано
			/// </summary>
			public readonly string SignedByID = nameof(SignedByID);

			/// <summary>
			///	Подписано
			/// </summary>
			public readonly string SignedByName = nameof(SignedByName);

			/// <summary>
			///	Подразделение
			/// </summary>
			public readonly string DepartmentID = nameof(DepartmentID);

			/// <summary>
			///	Контрагент.
			/// </summary>
			public readonly string PartnerID = nameof(PartnerID);

			/// <summary>
			///	Контрагент.
			/// </summary>
			public readonly string PartnerName = nameof(PartnerName);

			/// <summary>
			///	Ссылка на связанный документ. Например, ссылка от исходящего документа на входящий.
			/// </summary>
			public readonly string RefDocID = nameof(RefDocID);

			public readonly string ReceiverRowID = nameof(ReceiverRowID);

			public readonly string ReceiverName = nameof(ReceiverName);

			public readonly string CategoryID = nameof(CategoryID);

			public readonly string CategoryName = nameof(CategoryName);
			public readonly string IsMigrated = nameof(IsMigrated);

			public readonly string CardTypeName = nameof(CardTypeName);

			public readonly string CardTypeID = nameof(CardTypeID);

			public readonly string CardTypeCaption = nameof(CardTypeCaption);

			public static implicit operator string(DocumentCommonInfoInfo obj) => obj.ToString();

			public override string ToString() => name;
		}
	}
}