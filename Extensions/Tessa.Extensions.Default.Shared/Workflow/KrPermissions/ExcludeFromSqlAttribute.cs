using System;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    /// <summary>
    /// Аттрибут исключения флага из sql запроса
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFromSqlAttribute : Attribute
    {
        private readonly bool excludeFromSql;
        public bool ExcludeFromSql
        {
            get
            {
                return this.excludeFromSql;
            }
        }

        public ExcludeFromSqlAttribute()
        {
            this.excludeFromSql = true;
        }
    }
}
