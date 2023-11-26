﻿using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.Labels;
using System;
//using System.Collections;
using System.Collections.Generic;
//using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SSD365VSAddIn.Labels
{
    /// <summary>
    /// Helper class to create labels
    /// Most of the code has been adapted from https://stoneridgesoftware.com/learn-to-use-a-label-creator-add-in-extension-in-dynamics-365-for-finance-operations/
    /// </summary>
    class LabelHelper
    {
        public static void GetLabelLanguages(string labelFileId)
        {

        }

        public IList<AxLabelFile> GetLabelFilesSettings()
        {
            List<AxLabelFile> labelFilesToUpdate = new List<AxLabelFile>();

            Settings.FetchSettings.FindOrCreateSettings().LabelsToUpdate.ForEach(
                labelFileName =>
                {
                    var axLabelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(labelFileName);
                    labelFilesToUpdate.Add(axLabelFile);
                });

            return labelFilesToUpdate;
        }

        /// <summary>
        /// Gets the all label files for the current model
        /// </summary>
        /// <returns></returns>
        public static IList<AxLabelFile> GetAllLabelFilesCurrentModel()
        {
            List<AxLabelFile> labelFilesToUpdate = new List<AxLabelFile>();
            // Choose the current model
            var modelInfo = Common.CommonUtil.GetCurrentModel();

            // Get the list of label files from that model
            var metaModelProvider = Common.CommonUtil.GetModelSaveService();
            var metaModelProviders = Common.CommonUtil.GetMetaModelProviders();
            //var metaModelProviders = Microsoft.Dynamics.Framework.Tools.MetaModel.Core.ServiceLocator.GetService(typeof(Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders)) as Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders;
            //var metaModelService = metaModelProviders.CurrentMetaModelService;

            IList<string> labelFiles = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(Common.CommonUtil.GetCurrentModel().Name);

            foreach (var labelFileNameToCheck in labelFiles)
            {
                var labelFileToCheck = metaModelProvider.GetLabelFile(labelFileNameToCheck);
                labelFilesToUpdate.Add(labelFileToCheck);
            }

            return labelFilesToUpdate;
        }

        public static IList<AxLabelFile> GetLabelFilesFromModelSettings()
        {
            List<AxLabelFile> labelFilesToUpdate = new List<AxLabelFile>();
            
            // Choose the current model
            var modelInfo = Common.CommonUtil.GetCurrentModel();

            // Get the list of label files from that model
            var metaModelProvider = Common.CommonUtil.GetModelSaveService();
            var metaModelProviders = Common.CommonUtil.GetMetaModelProviders();

            //var metaModelProviders = Microsoft.Dynamics.Framework.Tools.MetaModel.Core.ServiceLocator.GetService(typeof(Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders)) as Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders;
            //var metaModelService = metaModelProviders.CurrentMetaModelService;

            IList<string> labelFiles = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(Common.CommonUtil.GetCurrentModel().Name);


            var modelSettings = Settings.FetchSettings.FindOrCreateSettings();
            labelFiles = modelSettings.LabelsToUpdate;
            foreach (var labelFile in labelFiles)
            {
                var labelFileToCheck = metaModelProvider.GetLabelFile(labelFile);
                labelFilesToUpdate.Add(labelFileToCheck);
            }
            return labelFilesToUpdate;
        }

        /// <summary>
        /// Gets a list of labels in the current model settings (without the language)
        /// </summary>
        /// <returns>String list of label ids</returns>
        public static IList<AxLabelFile> GetLabelFiles()
        {
            List<AxLabelFile> labelFilesToUpdate = new List<AxLabelFile>();
            // Choose the current model
            var modelInfo = Common.CommonUtil.GetCurrentModel();

            // Get the list of label files from that model
            var metaModelProvider = Common.CommonUtil.GetModelSaveService();
            var metaModelProviders = Common.CommonUtil.GetMetaModelProviders();

            //var metaModelProviders = Microsoft.Dynamics.Framework.Tools.MetaModel.Core.ServiceLocator.GetService(typeof(Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders)) as Microsoft.Dynamics.Framework.Tools.Extensibility.IMetaModelProviders;
            //var metaModelService = metaModelProviders.CurrentMetaModelService;

            IList<string> labelFiles = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(Common.CommonUtil.GetCurrentModel().Name);
            /* Use this to get all label files in system vs model
            IList<string> labelFiles =
                allModels == false
                ? metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(Common.CommonUtil.GetCurrentModel().Name)
                : Common.CommonUtil.GetModelSaveService().GetLabelFileNames();
            */

            var labelFileName = labelFiles.FirstOrDefault();
            if (labelFileName != null)
            {
                var labelFile = metaModelProvider.GetLabelFile(labelFileName);
                //LabelControllerFactory factory = new LabelControllerFactory();
                //LabelEditorController labelController = factory.GetLabelController(labelFile.LabelContentFileName); //factory.GetOrCreateLabelController(labelFile);
                // we want to get a list of labels with the same labelid (all languages)
                foreach (var labelFileNameToCheck in labelFiles)
                {
                    if (labelFileNameToCheck.Equals(labelFileName))
                    {
                        labelFilesToUpdate.Add(labelFile);
                        //labelFile = metaModelProvider.GetLabelFile(labelFileNameToCheck);
                    }
                    else
                    {
                        var labelFileToCheck = metaModelProvider.GetLabelFile(labelFileNameToCheck);
                        if (labelFileToCheck.LabelFileId.Equals(labelFile.LabelFileId))
                        {
                            labelFilesToUpdate.Add(labelFileToCheck);
                        }

                    }
                }
            }
            else
            {
                throw new Exception("No Labels found in current model");
            }
            return labelFilesToUpdate;
            //IList<String> labelFileNames = metaModelProvider.GetLabelFileNames();
            // Choose the first
            // What happens if there is no label file?
            //var labelFile = metaModelProvider.GetLabelFile(labelFileNames[0]);

            //return labelFile;
        }

        public static string MakeCamelCase(string labelText)
        {
            var punctuation = labelText.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = labelText.Split().Select(x => x.Trim(punctuation));
            string res = "";

            foreach (var word in words)
            {
                if (word.Length == 0)
                {
                    continue;
                }
                else if (word.Length == 1)
                {
                    res += Char.ToUpperInvariant(word[0]);
                }
                else if (word.Length > 1)
                {
                    res += Char.ToUpperInvariant(word[0]) + word.Substring(1);
                }
            }

            return res.Replace(" ", "");
        }

        public static string FindOrCreateLabel(string labelText, string labelCode = "")
        {
            string newLabelId = String.Empty;

            //Is the labeltext Already a label?
            if (labelText.StartsWith("@"))
            {
                return labelText;
            }

            // Don't bother if the string is empty
            if (String.IsNullOrEmpty(labelText) == false)
            {
                // Construct a label id that will be unique
                //string labelId = $"{elementName}{propertyName}";

                string labelId = labelCode;

                if (string.IsNullOrEmpty(labelId))
                {
                    labelId = (Settings.FetchSettings.FindOrCreateSettings().UseCamelCaseForLabels
                                        ? MakeCamelCase(labelText)
                                        : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(labelText))
                                        .Replace(" ", "");
                }

                // #35 replace chars with short text
                var replaceableChars = LabelHelper.GetReplaceableLabelChars();
                foreach (var key in replaceableChars.Keys)
                {
                    labelId = labelId.Replace(key, replaceableChars[key]);
                } 
                
                if (Regex.IsMatch(labelId.Substring(0, 1), @"\d")) // label id cannot start with a number
                {
                    labelId = "Label" + labelId;
                }
                labelId = Regex.Replace(labelId, "[^a-zA-Z0-9]", ""); // no special chars in the label id
                // Get the label factory
                LabelControllerFactory factory = new LabelControllerFactory();

                // Get the label edit controller
                //var labelFiles = LabelHelper.GetLabelFiles();
                var labelFiles = LabelHelper.GetLabelFilesFromModelSettings();

                var labelFile = labelFiles.First();

                LabelEditorController labelController = factory.GetOrCreateLabelController(labelFile, Common.CommonUtil.GetVSApplicationContext());
                labelController.LabelSearchOption = SearchOption.MatchExactly;
                labelController.IsMatchExactly = true;

                // Make sure the label doesn't exist.
                // What will you do if it does?
                //var labelsx = labelController.Labels.ToList();
                if (labelController.Exists(labelId) == false
                    && labelController.Exists(labelText) == false) // the label text may be an Id by itself
                {
                    labelController.Insert(labelId, labelText, null);
                    labelController.Save();
                    Common.CommonUtil.AddElementToProject(labelFile);

                    // Construct a label reference to go into the label property
                    // newLabelId = $"@{labelFile.LabelFileId}:{labelId}";

                    //Create this label in the other languages as well.
                    foreach (var labelFileToAdd in labelFiles)
                    {
                        if (labelFileToAdd.LabelFileId == labelFile.LabelFileId
                            && !labelFileToAdd.Language.Equals(labelFile.Language))
                        {
                            LabelEditorController labelControllerToAdd = factory.GetOrCreateLabelController(labelFileToAdd, Common.CommonUtil.GetVSApplicationContext());
                            labelControllerToAdd.Labels.Add(new Label(
                                labelControllerToAdd.CurrentLabelFile,
                                string.IsNullOrEmpty(labelId) ? labelControllerToAdd.LabelIDCache.NewLabelID() : labelId,
                                labelText, 
                                String.Empty, // Comment
                                LabelState.Init
                                ));
                            //labelControllerToAdd.Insert(labelId, labelText, null);
                            labelControllerToAdd.Save();
                            Common.CommonUtil.AddElementToProject(labelFileToAdd);
                        }
                    }
                }
                newLabelId = $"@{labelFile.LabelFileId}:{labelId}";
            }

            return newLabelId;
        }

        public static string GetLabelTextFromLabelFileWithLanguage(string labelFileCode, string lang, string labelId)
        {
            string res = "";

            var filenames = Common.CommonUtil.GetModelSaveService().GetLabelFileNames();
            var fileName = filenames.Where(l => l.StartsWith(labelFileCode) && l.Contains(lang)).FirstOrDefault();

            var labelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(fileName);

            if (labelFile == null)
            {
                return res;
            }

            LabelControllerFactory factory = new LabelControllerFactory();
            LabelEditorController labelController = factory.GetOrCreateLabelController(labelFile, Common.CommonUtil.GetVSApplicationContext());
            labelController.LabelSearchOption = SearchOption.MatchExactly;
            labelController.IsMatchExactly = true;

            var label = labelController.Labels.Where(l => l.ID.Equals(labelId, StringComparison.InvariantCultureIgnoreCase))
                                        .FirstOrDefault();
            if (label != null)
            {
                res = label.Description;
            }

            return res;
        }

        public static LabelContent FindLabelGlobally(string labelIdText)
        {
            //string labelId = String.Empty;
            LabelContent labelContent = new LabelContent() { LabelIdForProperty = labelIdText };

            if (labelIdText.StartsWith("@") == false)
            {
                return labelContent;
            }

            //var labelFileId
            labelContent.LabelFileId = LabelHelper.GetLabelFileId(labelIdText);
            labelContent.LabelId = labelContent.LabelIdForProperty.Substring(labelContent.LabelIdForProperty.IndexOf(":") + 1);

            // Get the label factory
            LabelControllerFactory factory = new LabelControllerFactory();

            // Get the label edit controller
            List<AxLabelFile> labelFiles = new List<AxLabelFile>();
            // AxLabelFile labelFile = null;
            if (String.IsNullOrEmpty(labelContent.LabelFileId) == false)
            {
                // Issue with finding label file (as it seems its searching for the label file in the current model)
                var labelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(labelContent.LabelFileId);
                if (labelFile == null)
                {
                    var fileNames = Common.CommonUtil.GetModelSaveService().GetLabelFileNames()
                        .Where(l => l.StartsWith(labelContent.LabelFileId) && l.Contains("en-")).ToList();// && l.Contains("en-")).FirstOrDefault();
                    fileNames.ForEach(labelFileFound =>
                    {
                        labelFiles.Add(Common.CommonUtil.GetModelSaveService().GetLabelFile(labelFileFound));
                    });
                    //labelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(fileName);
                }
                else
                {
                    labelFiles.Add(labelFile);
                }
            }
            else
            {
                //TODO: we need to extract "ALL" Label files across the entire system to find the labels - Although, it should never come to this.
                /*
                labelFiles = LabelHelper.GetLabelFiles()
                                    .Where(l => l.LabelFileId.Equals(labelContent.LabelFileId, StringComparison.InvariantCultureIgnoreCase)
                                                && l.LabelFileId.Contains("en-"))
                                    .ToList();
                */
            }
            if (labelFiles != null && labelFiles.Count() > 0)
            {
                foreach (var labelFile in labelFiles)
                {
                    LabelEditorController labelController = factory.GetOrCreateLabelController(labelFile, Common.CommonUtil.GetVSApplicationContext());
                    labelController.LabelSearchOption = SearchOption.MatchExactly;
                    labelController.IsMatchExactly = true;

                    var label = labelController.Labels.Where(l => l.ID.Equals(labelContent.LabelId, StringComparison.InvariantCultureIgnoreCase))
                                        .FirstOrDefault();
                    if (label != null)
                    {
                        labelContent.LabelDescription = label.Description;
                        labelContent.LabelText = label.Text;
                        break;
                    }
                }
            }
            return labelContent;
        }

        /*
        public static LabelContent FindLabelGlobally_OLD(string labelIdText)
        {
            //string labelId = String.Empty;
            LabelContent labelContent = new LabelContent() { LabelIdForProperty = labelIdText };

            if (labelIdText.StartsWith("@") == false)
            {
                return labelContent;
            }

            //var labelFileId
            labelContent.LabelFileId = LabelHelper.GetLabelFileId(labelIdText);
            labelContent.LabelId = labelContent.LabelIdForProperty.Substring(labelContent.LabelIdForProperty.IndexOf(":") + 1);

            // Get the label factory
            LabelControllerFactory factory = new LabelControllerFactory();

            // Get the label edit controller
            AxLabelFile labelFile = null;
            if (String.IsNullOrEmpty(labelContent.LabelFileId) == false)
            {
                // Issue with finding label file (as it seems its searching for the label file in the current model)
                labelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(labelContent.LabelFileId);
                if (labelFile == null)
                {
                    var fileName = Common.CommonUtil.GetModelSaveService().GetLabelFileNames()
                        .Where(l => l.StartsWith(labelContent.LabelFileId) && l.Contains("en-")).FirstOrDefault();
                    labelFile = Common.CommonUtil.GetModelSaveService().GetLabelFile(fileName);
                }
            }
            else
            {
                var labelFiles = LabelHelper.GetLabelFiles();

                labelFile = labelFiles
                                        .Where(l => l.LabelFileId.Equals(labelContent.LabelFileId, StringComparison.InvariantCultureIgnoreCase))
                                        .First();
            }
            if (labelFile != null)
            {
                LabelEditorController labelController = factory.GetOrCreateLabelController(labelFile, Common.CommonUtil.GetVSApplicationContext());
                labelController.LabelSearchOption = SearchOption.MatchExactly;
                labelController.IsMatchExactly = true;

                var test = labelController.Labels.ToList();
                var label = labelController.Labels.Where(l => l.ID.Equals(labelContent.LabelId, StringComparison.InvariantCultureIgnoreCase))
                                    .FirstOrDefault();
                if (label != null)
                {
                    labelContent.LabelDescription = label.Description;
                    labelContent.LabelText = label.Text;
                }
            }
            return labelContent;
        }
        */

        private static string GetLabelFileId(string labelId)
        {
            if(labelId.StartsWith("@") && labelId.Contains(":") == false)
            {
                // this is a label like @SYS123456
                var labelid = labelId.Substring(1, 3) + "_en-US"; //TODO: how to change it from en-us? // this is a drawback as only the US files are available e.g. SYS_en-US under ApplicationPlatform
                return labelid;
            }
            if (labelId.StartsWith("@") == false
                || labelId.Contains(":") == false)
                return String.Empty;

            string labelFileId = labelId.Substring(1, labelId.IndexOf(":", 0) - 1);

            return labelFileId;

        }

        private static void CreateLabel(AxLabelFile labelFile, string labelId, string labelText)
        {
            // Get the label factory
            LabelControllerFactory factory = new LabelControllerFactory();

            LabelEditorController labelControllerToAdd = factory.GetOrCreateLabelController(labelFile, Common.CommonUtil.GetVSApplicationContext());
            labelControllerToAdd.Insert(labelId, labelText, null);
            labelControllerToAdd.Save();

        }

        protected static Dictionary<string, string> GetReplaceableLabelChars()
        {
            Dictionary<string, string> replaceableChars = new Dictionary<string, string>();
            replaceableChars.Add("#", "Hash");
            replaceableChars.Add("@", "At");
            replaceableChars.Add("!", "Excl");
            replaceableChars.Add("$", "Dollar");
            replaceableChars.Add("^", "Exp");
            replaceableChars.Add("&", "Amp");
            replaceableChars.Add("*", "Star");
            replaceableChars.Add("+", "Plus");
            replaceableChars.Add("-", "Dash");
            replaceableChars.Add("|", "Pipe");
            replaceableChars.Add(@"\", "BSlash");
            replaceableChars.Add("/", "FSlash");
            replaceableChars.Add("=", "Equal");
            replaceableChars.Add("_", "Undescore");
            replaceableChars.Add("~", "Tilde");
            replaceableChars.Add("%", "P");


            return replaceableChars;
        }
            
    }

    /// <summary>
    /// Holds data of a label
    /// </summary>
    public class LabelContent
    {
        /// <summary>
        /// String that is in the property fields e.g. @SSDemo:MyStringLabel
        /// </summary>
        public string LabelIdForProperty { get; set; }
        /// <summary>
        /// Part of the LabelIdProperty that represents the label's file Id
        /// e.d. SSDemo in @SSDemo:MyStringLabel
        /// </summary>
        public String LabelFileId { get; set; }
        /// <summary>
        /// Part of the LabelIdProperty that represents the label's Id in the file
        /// e.d. MyStringLabel in @SSDemo:MyStringLabel
        /// </summary>
        public string LabelId { get; set; }
        /// <summary>
        /// Value of the label
        /// e.g. My string label
        /// </summary>
        public string LabelText { get; set; }
        /// <summary>
        /// Labels description
        /// </summary>
        public string LabelDescription { get; set; }
    }
}
