using System;
using System.IO;
using Tessa.Platform.Data;

namespace Tessa.Extensions.Default.Console.SchemeRename
{
    public sealed class StreamQueryBuilder :
        IQueryBuilder
    {
        #region Fields

        private readonly Dbms dbms;

        private readonly TextWriter writer;

        #endregion

        #region Constructors

        public StreamQueryBuilder(TextWriter writer, Dbms dbms)
        {
            this.dbms = dbms;
            this.writer = writer;
        }

        #endregion

        #region IQueryBuilder Overrides

        public Dbms Dbms
        {
            get { return this.dbms; }
        }

        public IQueryBuilder Append(string value)
        {
            this.writer.Write(value);
            this.RequiresComma = false;

            var length = value.Length;
            if (length > 0)
            {
                var ch = value[length - 1];
                this.RequiresWhitespace = ch != '(' && !char.IsWhiteSpace(ch);
            }

            return this;
        }

        public IQueryBuilder IncreaseIndent()
        {
            this.Indent++;
            return this;
        }

        public IQueryBuilder DecreaseIndent()
        {
            if (this.Indent > 0)
            {
                this.Indent--;
            }

            return this;
        }

        public int Indent { get; private set; }

        public bool RequiresWhitespace { get; private set; }

        public IQueryBuilder RequireComma()
        {
            this.RequiresComma = true;
            return this;
        }

        public bool RequiresComma { get; private set; }

        string IQueryBuilder.Build() => throw new NotSupportedException();

        void IQueryBuilder.Clear() => throw new NotSupportedException();

        #endregion
    }
}