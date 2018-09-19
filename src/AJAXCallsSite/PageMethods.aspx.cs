using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;

namespace AJAXCallsSite
{
    public partial class PageMethods : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(Execute));
        }

        public async Task Execute()
        {
            switch (Request.PathInfo)
            {
                case "/fast/":
                    await FastGetCall();
                    break;

                case "/slow/":
                    await SlowGetCall();
                    break;

                case "/post/":
                    await SlowPostCall();
                    break;
            }
        }

        public async Task FastGetCall()
        {
            await Task.Yield();
            Response.Write(DateTime.Now.ToString("o"));
        }
        public async Task SlowGetCall()
        {
            await Task.Delay(int.TryParse(Request.QueryString["wait"], out var wait) ? wait : 4000);
            Response.Write(DateTime.Now.ToString("o"));
        }

        public async Task SlowPostCall()
        {
            await Task.Yield();
            var req = await Task.Run(() =>
            {
                using (var w = Request.InputStream)
                {
                    return JObject.Parse(new StreamReader(w).ReadToEnd());
                }
            });

            await Task.Delay(req["wait"]?.Value<int>() ?? 6000);
            Response.Write(new Random().Next(100000000));
        }
        
    }
}