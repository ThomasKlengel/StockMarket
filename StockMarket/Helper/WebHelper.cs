using System.Net;
using System.Threading.Tasks;

namespace StockMarket
{
    /// <summary>
    /// A helper class for web access
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// get the content of a website
        /// </summary>
        /// <param name="webSite">the website to get the content from</param>
        /// <returns>the content of the website</returns>
        public static async Task<string> getWebContent(string webSite)
        {
            string webContent = string.Empty;
            try
            {
                using (WebClient client = new WebClient())
                {
                    webContent = await client.DownloadStringTaskAsync(webSite);
                    return webContent;
                }
            }
            catch (WebException WebEx)
            {
                System.Windows.MessageBox.Show(WebEx.Message);
                return string.Empty;
            }
        }

    }
}
