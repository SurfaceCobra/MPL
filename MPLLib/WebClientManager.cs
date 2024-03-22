using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib
{
    public static class WebClientManager
    {
        public static byte[] HTTPGet(string url)
        {
            TryHTTPGet(url, out byte[] response);
            return response;
        }

        public static bool TryHTTPGet(string url, out byte[] response)
        {
            try
            {
                using var web = new WebClient();
                response = web.DownloadData(url);
                return true;
            }
            catch
            {
                response = null;
                return false;
            }
        }
    }
}
