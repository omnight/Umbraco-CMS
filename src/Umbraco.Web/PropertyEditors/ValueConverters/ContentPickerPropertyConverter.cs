﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContentPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The content picker property editor converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Extensions;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The content picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(IPublishedContent))]
    [PropertyValueCache(PropertyCacheValue.Object, PropertyCacheLevel.ContentCache)]
    [PropertyValueCache(PropertyCacheValue.Source, PropertyCacheLevel.Content)]
    [PropertyValueCache(PropertyCacheValue.XPath, PropertyCacheLevel.Content)]
    public class ContentPickerPropertyConverter : PropertyValueConverterBase
    {
        /// <summary>
        /// The properties to exclude.
        /// </summary>
        private static readonly List<string> PropertiesToExclude = new List<string>()
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.ContentPickerAlias)
                    || propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.ContentPicker2Alias);
            }
            return false;
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        /// <summary>
        /// Convert the source nodeId into a IPublishedContent (or DynamicPublishedContent)
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            if (UmbracoContext.Current != null)
            {
                if ((propertyType.PropertyTypeAlias != null && PropertiesToExclude.Contains(propertyType.PropertyTypeAlias.ToLower(CultureInfo.InvariantCulture))) == false)
                {
                    IPublishedContent content;
                    switch (propertyType.PropertyEditorAlias)
                    {
                        case Constants.PropertyEditors.ContentPickerAlias:
                            int sourceInt;
                            if (int.TryParse(source.ToString(), out sourceInt))
                            {
                                content = UmbracoContext.Current.ContentCache.GetById(sourceInt);
                                return content;
                            }
                            break;
                        case Constants.PropertyEditors.ContentPicker2Alias:
                            Udi udi;
                            if (Udi.TryParse(source.ToString(), out udi))
                            {
                                content = udi.ToPublishedContent();
                                if (content != null)
                                    return content;
                            }
                            break;
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// The convert source to xPath.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source.ToString();
        }

    }
}