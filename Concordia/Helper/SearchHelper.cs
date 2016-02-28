using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Concordia
{
    static class SearchHelper
    {
        public static async Task<string> GetResponseAsync(string v) =>
            await new StreamReader((await ((HttpWebRequest)WebRequest.Create(v)).GetResponseAsync()).GetResponseStream()).ReadToEndAsync();
    }
}
