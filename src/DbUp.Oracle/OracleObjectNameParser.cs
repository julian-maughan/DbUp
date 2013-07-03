using System;
using DbUp.Support;

namespace DbUp.Oracle
{
    public class OracleObjectNameParser : IObjectNameParser
    {
        public string Quote(string objectName)
        {
            return objectName;
        }

        public string Quote(string objectName, ObjectNameOptions objectNameOptions)
        {
            return objectName;
        }
    }
}