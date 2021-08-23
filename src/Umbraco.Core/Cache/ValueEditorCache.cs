﻿using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Cache
{
    public class ValueEditorCache : IValueEditorCache,
        INotificationHandler<DataTypeSavedNotification>,
        INotificationHandler<DataTypeDeletedNotification>
    {
        private readonly Dictionary<string, Dictionary<int, IDataValueEditor>> _valueEditorCache = new();

        public IDataValueEditor GetValueEditor(IDataEditor editor, IDataType dataType)
        {
            // Instead of creating a value editor immediately check if we've already created one and use that.
            IDataValueEditor valueEditor;
            if (_valueEditorCache.TryGetValue(editor.Alias, out Dictionary<int, IDataValueEditor> dataEditorCache))
            {
                if (dataEditorCache.TryGetValue(dataType.Id, out valueEditor))
                {
                    return valueEditor;
                }

                valueEditor = editor.GetValueEditor(dataType.Configuration);
                dataEditorCache[dataType.Id] = valueEditor;
                return valueEditor;

            }

            valueEditor = editor.GetValueEditor(dataType.Configuration);
            _valueEditorCache[editor.Alias] = new Dictionary<int, IDataValueEditor> { [dataType.Id] = valueEditor };
            return valueEditor;
        }

        public void Handle(DataTypeSavedNotification notification) =>
            ClearCache(notification.SavedEntities.Select(x => x.Id));

        public void Handle(DataTypeDeletedNotification notification) =>
            ClearCache(notification.DeletedEntities.Select(x => x.Id));

        private void ClearCache(IEnumerable<int> dataTypeIds)
        {
            // If a datatype is saved or deleted we have to clear any value editors based on their ID from the cache,
            // since it could mean that their configuration has changed.
            foreach (var id in dataTypeIds)
            {
                foreach (Dictionary<int, IDataValueEditor> editors in _valueEditorCache.Values)
                {
                    editors.Remove(id);
                }
            }
        }
    }
}
