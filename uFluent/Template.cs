﻿using System;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UmbracoTemplateDef = Umbraco.Core.Models.Template;
using uFluent.Persistence;

namespace uFluent
{
    /// <summary>
    /// Wrapper around the Umbraco <see cref="ITemplate"/> and <see cref="IFileService"/>
    /// </summary>
    public class Template
    {
        internal readonly ITemplate UmbracoTemplate;

        private readonly IFileService _fileService;

        /// <summary>
        /// Template alias
        /// </summary>
        public string Alias
        {
            get { return UmbracoTemplate.Alias; }
        }

        internal Template(ITemplate umbracoTemplate, IFileService fileService)
        {
            UmbracoTemplate = umbracoTemplate;
            _fileService = fileService;
        }

        /// <summary>
        /// Create a new template. Call <see cref="Save"/> to ensure the template
        /// is saved to disk. The template alias is generated by removing spaces
        /// from the name.
        /// </summary>
        /// <param name="name">Template alias and name</param>
        /// <returns></returns>
        public static Template Create(string name)
        {
            var alias = name.Replace(" ", "");

            return TemplateService.Create(alias, name);
        }

        /// <summary>
        /// Create a new template. Call <see cref="Save"/> to ensure the template
        /// is saved to disk
        /// </summary>
        /// <param name="alias">Template alias</param>
        /// <param name="name">Template name</param>
        /// <returns></returns>
        public static Template Create(string alias, string name)
        {
            return TemplateService.Create(alias, name);
        }

        /// <summary>
        /// Obtain a template using its alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static Template Get(string alias)
        {
            return TemplateService.Get(alias);
        }

        /// <summary>
        /// Set the master template property.
        /// </summary>
        /// <param name="masterTemplate"></param>
        /// <returns></returns>
        public Template SetMasterTemplate(Template masterTemplate)
        {

            if (masterTemplate == null)
            {
                throw new ArgumentNullException("masterTemplate");
            }

            var type = typeof(UmbracoTemplateDef);
            var alias = type.GetProperty("MasterTemplateAlias");
            alias.SetValue(UmbracoTemplate, masterTemplate.Alias, null);

            var id = type.GetProperty("MasterTemplateId");
            var templateId = masterTemplate.UmbracoTemplate.Id;
            id.SetValue(UmbracoTemplate, new Lazy<int>(() => templateId), null);

            return this;
        }

        /// <summary>
        /// Un-set the master template property.
        /// </summary>
        /// <returns></returns>
        public Template ClearMasterTemplate()
        {
            var type = typeof(UmbracoTemplateDef);
            var alias = type.GetProperty("MasterTemplateAlias");
            alias.SetValue(UmbracoTemplate, null, null);

            var id = type.GetProperty("MasterTemplateId");
            id.SetValue(UmbracoTemplate, null, null);

            return this;
        }

        /// <summary>
        /// Save this template.
        /// </summary>
        /// <returns></returns>
        public Template Save()
        {
            _fileService.SaveTemplate(UmbracoTemplate);
            return this;
        }

        /// <summary>
        /// Delete this template.
        /// </summary>
        public void Delete()
        {
            _fileService.DeleteTemplate(UmbracoTemplate.Alias, 0);
        }

        private static FluentTemplateService TemplateService
        {
            get
            {
                return NestedLazyLoader.Instance;
            }
        }

        private class NestedLazyLoader
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedLazyLoader()
            {
            }

            internal static readonly FluentTemplateService Instance = new FluentTemplateService(UmbracoUtils.Instance);
        }
    }
}