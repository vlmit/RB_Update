using System;
using Tessa.Files;

namespace Tessa.Extensions.Shared.Info
{
	/// <summary>
	/// Класс описывающий категории файлов используемых в решении
	/// </summary>
	public static class FileCategories
	{

		/// <summary>
		/// Документ
		/// </summary>
		public static FileCategory MainDoc = new FileCategory(new Guid("723bf3bd-31c2-4269-a76a-d323e082a2f1"), "Документ");

		/// <summary>
		/// Приложение
		/// </summary>
		public static FileCategory Application = new FileCategory(new Guid("cc650bc9-3e45-4480-9274-3bb3b13d20d5"), "Приложение");

		/// <summary>
		/// Предпросмотр
		/// </summary>
		public static FileCategory Preview = new FileCategory(new Guid("3133e616-81d8-4a82-937f-4a8636bfa1de"), "Предпросмотр");
		
	}
}
