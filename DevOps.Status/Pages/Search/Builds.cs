
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevOps.Util;
using DevOps.Util.DotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;

namespace DevOps.Status.Pages.Search
{
    public class BuildsModel : PageModel
    {
        public class BuildData
        {
            public string Result { get; set; }

            public int BuildNumber { get; set; }

            public string BuildUri { get; set; }
        }

        public DevOpsServer Server { get; }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        public List<BuildData> Builds { get; set; } = new List<BuildData>();

        public BuildsModel(DevOpsServer server)
        {
            Server = server;
        }

        public async Task OnGet()
        {
            if (string.IsNullOrEmpty(Query))
            {
                return;
            }

            var queryUtil = new DotNetQueryUtil(Server);
            var builds = await queryUtil.ListBuildsAsync(Query);
            Builds = builds
                .Select(x =>
                {
                    var buildInfo = x.GetBuildInfo();
                    return new BuildData()
                    {
                        Result = x.Result.ToString(),
                        BuildNumber = buildInfo.Number,
                        BuildUri = buildInfo.BuildUri
                    };
                })
                .ToList();
        }
    }
}