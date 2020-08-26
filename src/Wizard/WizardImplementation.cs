﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ArtemisPluginTemplates.Dialogs;
using ArtemisPluginTemplates.Models;
using ArtemisPluginTemplates.PluginTypes;
using ArtemisPluginTemplates.PluginTypes.DataModelExpansion;
using ArtemisPluginTemplates.PluginTypes.Device;
using ArtemisPluginTemplates.PluginTypes.LayerBrush;
using ArtemisPluginTemplates.PluginTypes.LayerEffect;
using ArtemisPluginTemplates.PluginTypes.Module;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace ArtemisPluginTemplates
{
    public class WizardImplementation : IWizard
    {
        private readonly PluginInfo _pluginInfo = new PluginInfo {Guid = Guid.NewGuid()};
        private IPluginType _pluginType;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            GatherPluginInfo(replacementsDictionary, customParams);
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return _pluginType.ShouldAddProjectItem(filePath);
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void GatherPluginInfo(Dictionary<string, string> replacementsDictionary, object[] customParams)
        {
            _pluginInfo.Name = replacementsDictionary["$specifiedsolutionname$"];
            _pluginInfo.Description = "This is my awesome plugin";
            _pluginInfo.Icon = "ToyBrickPlus";

            var pluginInfoDialog = new PluginInfoDialog(_pluginInfo);
            var pluginInfoDialogCompleted = pluginInfoDialog.ShowModal();
            if (pluginInfoDialogCompleted == false)
                throw new WizardCancelledException();

            // Paths
            replacementsDictionary["$ArtemisDirectory$"] = _pluginInfo.ArtemisDirectory;
            replacementsDictionary["$ArtemisDirectoryEscaped$"] = _pluginInfo.ArtemisDirectory.Replace("\\", "\\\\");

            // Nuget package versions
            replacementsDictionary["$MaterialDesignThemesVersion$"] = GetNugetFileVersion("MaterialDesignThemes.Wpf.dll");
            replacementsDictionary["$SkiaSharpVersion$"] = GetNugetFileVersion("SkiaSharp.dll");
            replacementsDictionary["$StyletVersion$"] = GetNugetFileVersion("Stylet.dll");

            // Plugin info
            replacementsDictionary["$PluginGuid$"] = _pluginInfo.Guid.ToString();
            replacementsDictionary["$PluginName$"] = _pluginInfo.Name;
            replacementsDictionary["$PluginDescription$"] = _pluginInfo.Description;
            if (_pluginInfo.Icon != null)
                replacementsDictionary["$PluginIcon$"] = _pluginInfo.Icon;


            // customParams contains the path to the template
            if (customParams.Any(p => p.ToString().Contains("Module")))
                _pluginType = new ModulePluginType(this, replacementsDictionary, customParams, _pluginInfo);
            else if (customParams.Any(p => p.ToString().Contains("Layer Brush")))
                _pluginType = new LayerBrushType(this, replacementsDictionary, customParams, _pluginInfo);
            else if (customParams.Any(p => p.ToString().Contains("Layer Effect")))
                _pluginType = new LayerEffectType(this, replacementsDictionary, customParams, _pluginInfo);
            else if (customParams.Any(p => p.ToString().Contains("Data Model Expansion")))
                _pluginType = new DataModelExpansionType();
            else if (customParams.Any(p => p.ToString().Contains("Device")))
                _pluginType = new DeviceType();
            else
                throw new Exception("Couldn't detect a plugin type based on the template that was selected.");

            _pluginType.GatherInfo();
        }

        private string GetNugetFileVersion(string file)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(_pluginInfo.ArtemisDirectory, file)).FileVersion;
            // Only take the first three parts of the version
            return string.Join(".", fileVersion.Split('.').Take(3));
        }
    }
}