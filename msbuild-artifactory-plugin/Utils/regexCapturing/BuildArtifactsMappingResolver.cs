﻿using JFrog.Artifactory.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JFrog.Artifactory.Utils.regexCapturing
{
    public class BuildArtifactsMappingResolver
    {
        public static void matchMappingArtifacts(BuildArtifactsMapping mapping, string projectDirectory, Dictionary<string, string> resultMap)
        {           
            string rootDirectory = BuildArtifactsMapping.getRootDirectory(mapping);

            /*Incase we have none relative pattern, we need to add the project path for the 
             * regular expression not to recognize it as part of the capturing groups. 
             */
            //if (String.IsNullOrWhiteSpace(rootDirectory) || !System.IO.Path.IsPathRooted(rootDirectory))
            if (!System.IO.Path.IsPathRooted(rootDirectory))
            {
                mapping.input = projectDirectory + "\\" + mapping.input;
                rootDirectory = projectDirectory + "\\" + rootDirectory; 
            }

            if (Directory.Exists(rootDirectory) || File.Exists(rootDirectory))
            {
                IEnumerable<string> fileList;
                //Incase we have a path with a file
                if (File.Exists(rootDirectory))
                {
                    fileList = new string[] { rootDirectory };
                }
                else 
                {
                    fileList = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories);
                }


                string regexNormalize = NormalizeRegexInput(mapping);
                Regex regex = new Regex(regexNormalize);
                int inputGroupsNum = regex.GetGroupNumbers().Length - 1;

                ISet<Int32> placeHoldersSet = BuildArtifactsMapping.getPlaceHoldersList(mapping, inputGroupsNum);

                foreach (string file in fileList)
                {
                    Match fileMatcher = regex.Match(file);
                    if (fileMatcher.Success)
                    {
                        /* In case we didn't receive an output pattern, 
                        *   the target will be the root of the repository
                        */
                        if (String.IsNullOrWhiteSpace(mapping.output))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            resultMap.Add(file, fileInfo.Name);
                        }
                        else
                        {
                            List<string> replacementList = new List<string>();
                            string repositoryPath = mapping.output;

                            foreach (int groupIndex in placeHoldersSet) 
                            {
                                repositoryPath = repositoryPath.Replace("$" + groupIndex, fileMatcher.Groups[groupIndex].Value);
                            }

                            if (!resultMap.ContainsKey(file))
                                resultMap.Add(file, repositoryPath);
                        }
                    }
                }
            }
        }

        private static string NormalizeRegexInput(BuildArtifactsMapping mapping)
        {
            string regexNormalize = mapping.input.Replace("\\", "\\\\");
            Regex reg = new Regex(@"(\(.*?\))");
            Match matchParentheses = reg.Match(regexNormalize);
            while (matchParentheses.Success) 
            {
                regexNormalize = regexNormalize.Replace(matchParentheses.Value, Regex.Unescape(matchParentheses.Value));
                matchParentheses = matchParentheses.NextMatch();
            }

            return regexNormalize;
        }
    }
}
