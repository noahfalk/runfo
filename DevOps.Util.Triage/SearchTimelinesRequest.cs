﻿using DevOps.Util.DotNet;
using DevOps.Util.Triage;
using Microsoft.EntityFrameworkCore;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevOps.Util.Triage
{
    public class SearchTimelinesRequest : ISearchRequest
    {
        public string? Text { get; set; }

        public async Task<List<ModelTimelineIssue>> GetResultsAsync(
            TriageContextUtil triageContextUtil,
            IQueryable<ModelBuild> buildQuery,
            bool includeBuild)
        {
            IQueryable<ModelTimelineIssue> query = buildQuery
                .SelectMany(x => x.ModelTimelineIssues);

            if (includeBuild)
            {
                query = query.Include(x => x.ModelBuild).ThenInclude(x => x.ModelBuildDefinition);
            }

            var list = await query.ToListAsync().ConfigureAwait(false);

            // TODO: Unify with DotNetQueryUtil.CreateSearchRegex 
            if (Text is object)
            {
                var textRegex = new Regex(Text, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                list = list
                    .Where(x => textRegex.IsMatch(x.Message))
                    .ToList();
            }

            return list;
        }

        public string GetQueryString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Text))
            {
                Append($"text:\"{Text}\"");
            }

            return builder.ToString();

            void Append(string message)
            {
                if (builder.Length != 0)
                {
                    builder.Append(" ");
                }

                builder.Append(message);
            }
        }

        public void ParseQueryString(string userQuery)
        {
            if (!userQuery.Contains(":"))
            {
                Text = userQuery.Trim('"');
                return;
            }

            foreach (var tuple in DotNetQueryUtil.TokenizeQueryPairs(userQuery))
            {
                switch (tuple.Name.ToLower())
                {
                    case "text":
                        Text = tuple.Value.Trim('"');
                        break;
                    default:
                        throw new Exception($"Invalid option {tuple.Name}");
                }
            }
        }
    }
}
