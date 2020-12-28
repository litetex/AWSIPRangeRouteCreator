using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSIPRangeRouteCreator
{
   public enum OutputFileFormat
   {
      /// <summary>
      /// OpenVPN route format
      /// </summary>
      /// <example>
      /// route 1.1.0.0 255.255.0.0
      /// </example>
      OpenVPN,
      /// <summary>
      /// Standard format
      /// </summary>
      /// <example>
      /// 1.1.0.0/16
      /// </example>
      Simple
   }
}
