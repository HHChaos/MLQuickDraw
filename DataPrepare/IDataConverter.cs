using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrepare
{
    public interface IDataConverter
    {
        string GetHeader();
        string CovertData(DrawingInfo data);
    }
}
