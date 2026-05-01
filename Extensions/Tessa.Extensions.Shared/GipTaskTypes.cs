using System;
using System.Collections.Generic;
using System.Text;

namespace Tessa.Extensions.Shared
{
    public static class GipTaskTypes
    {
        /// <summary>
        /// Task type identifier for "GipSignStamp": {C7490D80-555A-44A2-B518-A6ED589214D3}.
        /// </summary>
        public static readonly Guid GipSignStampTypeID = new Guid(0xc7490d80, 0x555a, 0x44a2, 0xb5, 0x18, 0xa6, 0xed, 0x58, 0x92, 0x14, 0xd3);

        /// <summary>
        /// Task type name for "GipSignStamp".
        /// </summary>
        public const string GipSignStampTypeName = "GipSignStamp";


        /// <summary>
        /// Task type identifier for "GipConvertToPdf": {22C9F7DB-D9BA-4C35-806A-8647DF29B878}.
        /// </summary>
        public static readonly Guid GipConvertToPdfTypeID = new Guid(0x22c9f7db, 0xd9ba, 0x4c35, 0x80, 0x6a, 0x86, 0x47, 0xdf, 0x29, 0xb8, 0x78);

        /// <summary>
        /// Task type name for "GipConvertToPdf".
        /// </summary>
        public const string GipConvertToPdfTypeName = "GipConvertToPdf";

    }
}
