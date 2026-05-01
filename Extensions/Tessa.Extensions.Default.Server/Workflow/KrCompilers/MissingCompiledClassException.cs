using System;
using System.Runtime.Serialization;
using Tessa.Localization;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Исключение, возникающее при попытке доступа к скомпилированному классу, который не существует.
    /// </summary>
    [Serializable]
    public class MissingCompiledClassException : Exception
    {
        #region Fields

        private readonly string className;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MissingCompiledClassException"/>.
        /// </summary>
        /// <param name="className">Имя класса.</param>
        public MissingCompiledClassException(string className)
            : this(className, null)
        {

        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MissingCompiledClassException"/>.
        /// </summary>
        /// <param name="className">Имя класса.</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение, или пустая ссылка, если внутреннее исключение не задано.</param>
        public MissingCompiledClassException(string className, Exception innerException)
            : base(null, innerException)
        {
            Check.ArgumentNotNullOrEmpty(className, nameof(className));

            this.className = className;
        }

        /// <doc path='info[@type="ISerializable" and @item=".ctor"]'/>
        protected MissingCompiledClassException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.className = info.GetString("ClassName");
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ClassName", this.className, typeof(string));
        }

        /// <inheritdoc/>
        public override string Message => LocalizationManager.Format("$KrProcess_ClassMissed", this.className);

        #endregion
    }
}
