using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Tools.CommonConfig.Provider.SQL
{
    public class sys_plus_Config
    {
        public string id { set; get; }

        public string AuthType { set; get; }

        public string Key { set; get; }

        public string Value { set; get; }

        public DateTime LastUpdateDt { set; get; }
    }
}
