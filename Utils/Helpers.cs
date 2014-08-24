﻿using System.Text.RegularExpressions;
using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils
{
    public static class Helpers
    {
        public static string ToJsonString(this Build model)
        {
            // sb.AppendFormat("\"\":\"{0}\",", model);
            var json = new StringBuilder();
            //open json root
            json.Append("{");
            json.AppendFormat("\"version\":\"{0}\",",model.version);
            json.AppendFormat("\"name\":\"{0}\",", model.name);
            json.AppendFormat("\"number\":\"{0}\",", model.number);
            json.AppendFormat("\"buildAgent\":{{\"name\":\"{0}\",\"version\":\"{1}\"}},", model.buildAgent.name, model.buildAgent.version);
            json.AppendFormat("\"started\":\"{0}\",", model.started);
            json.AppendFormat("\"durationMillis\":{0},", model.durationMillis);
            var arrString = model.principal.Split('\\');
            json.AppendFormat("\"principal\":\"{0}\",", arrString.LastOrDefault());
            json.AppendFormat("\"artifactoryPrincipal\":\"{0}\",", model.artifactoryPrincipal);
            json.AppendFormat("\"url\":\"{0}\",", model.url);
            json.AppendFormat("\"vcsRevision\":\"{0}\",", model.vcsRevision);
            //TODO license control
            json.AppendFormat("\"licenseControl\":null,", model.licenseControl);
            json.Append("\"buildRetention\":null,");

            //system variables start
            json.Append("\"properties\":{");
            var lastKey = model.properties.LastOrDefault();

            String quoteMatch = @"""";
            String doubleBackSlashMatch = @"\\";

            foreach (var kvp in model.properties)
            {
                String cleanValue = Regex.Replace(kvp.Value, doubleBackSlashMatch, doubleBackSlashMatch).Replace(quoteMatch, @"\""");
                json.AppendFormat("\"{0}\":\"{1}\"", kvp.Key, cleanValue);
                if (kvp.Key != lastKey.Key)
                {
                    json.Append(",");
                }
            }
            json.Append("},");

            json.Append("\"modules\":[");
            var modulesCount = model.modules.Count();
            for (var i=0; i < modulesCount; i++)
            {
                createModule(model, json, i);
                if ((i + 1) < modulesCount)
                {
                    json.Append(",");
                }
            }
            json.Append("]"); 

            //close json root
            json.Append("}");

            return json.ToString();
        }

        private static void createModule(Build model, StringBuilder sb, int i)
        {
            //module start
            sb.Append("{");

            sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].id);
            sb.Append("\"dependencies\": [");
            for (var ii = 0; ii < model.modules[i].Dependencies.Count(); ii++)
            {
                sb.Append("{");
                sb.AppendFormat("\"type\":\"{0}\",", model.modules[i].Dependencies[ii].type);
                sb.AppendFormat("\"sha1\":\"{0}\",", model.modules[i].Dependencies[ii].sha1);
                sb.AppendFormat("\"md5\":\"{0}\",", model.modules[i].Dependencies[ii].md5);
                sb.AppendFormat("\"id\":\"{0}\",", model.modules[i].Dependencies[ii].id);
                sb.Append("\"scopes\":[");
                for (var n = 0; n < model.modules[i].Dependencies[ii].scopes.Count; n++)
                {
                    sb.AppendFormat(
                        n + 1 < model.modules[i].Dependencies[ii].scopes.Count ? "\"{0}\"," : "\"{0}\"",
                        model.modules[i].Dependencies[ii].scopes[n]);
                }
                sb.Append("]}");
                if ((ii + 1) < model.modules[i].Dependencies.Count())
                {
                    sb.Append(",");
                }
            }
            sb.Append("],");

            sb.Append("\"artifacts\": [");

            for (var ii = 0; ii < model.modules[i].Artifacts.Count(); ii++)
            {
                sb.Append("{");
                sb.AppendFormat("\"type\":\"{0}\",", model.modules[i].Artifacts[ii].type);
                sb.AppendFormat("\"sha1\":\"{0}\",", model.modules[i].Artifacts[ii].sha1);
                sb.AppendFormat("\"md5\":\"{0}\",", model.modules[i].Artifacts[ii].md5);
                sb.AppendFormat("\"name\":\"{0}\"", model.modules[i].Artifacts[ii].name);
                sb.Append("}");

                if ((ii + 1) < model.modules[i].Artifacts.Count())
                {
                    sb.Append(",");
                }
            }

            sb.Append("]");

            //module end
            sb.Append("}");
        }
    }
}
