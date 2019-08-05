using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using dtSearch.Engine;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


namespace WebDemo {
    public class WebDemoIndexCache : IndexCache {
        public WebDemoIndexCache(IOptions<AppSettings> settings) : base(settings.Value.IndexCache.MaxIndexCount) {
            AutoReopenTime = settings.Value.IndexCache.AutoReopenTime;
            AutoCloseTime = settings.Value.IndexCache.AutoCloseTime;
            }
        }
    public class Startup {
        private void EnableDebugLogging() {
            string DebugLogName = Path.Combine(Path.GetTempPath(), "dtSearchAspNetCoreDemo.log");
            Server.SetDebugLogging(DebugLogName, DebugLogFlags.dtsLogDefault);
            }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
            // Un-comment to generate a diagnostic log
            EnableDebugLogging();
            }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = true;
            });
            services.Configure<AppSettings>(options => Configuration.GetSection("SearchSettings").Bind(options));
            services.AddSingleton<WebDemoIndexCache>();
            }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            if (env.IsDevelopment()) {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                }
            else {
                app.UseExceptionHandler("/Error");
                }


            app.UseStaticFiles();

            app.UseMvc();

            }
        }
    }
