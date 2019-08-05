using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using dtSearch.Engine;

namespace WebDemo.Pages
{
    public class AboutModel : PageModel
    {

        public string VersionMessage { get; set; }
        public string EnvironmentMessage { get; set; }

        public string CodeBaseFolder { set; get; }

        public string ContentRootPath { set; get; }

        public string WebRootPath { set; get; }

        public string NetStdApiPath { set; get; }

        public string IndexCacheStatus { set; get; }

        public AboutModel(IHostingEnvironment env, WebDemoIndexCache indexCache) {
            ContentRootPath = env.ContentRootPath;
            WebRootPath = env.WebRootPath;

            IndexCacheStatus = "Size: " + indexCache.MaxCount +
                                " In use: " + indexCache.InUseCount +
                                " Open index count: " + indexCache.OpenIndexCount +
                                " Hit count: " + indexCache.HitCount;
                                }
        public void OnGet()
        {
           
            
            CodeBaseFolder = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            dtSearch.Sample.VersionInfo vi = new dtSearch.Sample.VersionInfo();
            VersionMessage = vi.ToString();
            EnvironmentMessage = vi.PlatformString;

            NetStdApiPath = typeof(dtSearch.Engine.Server).Assembly.Location;
        }
    }
}
