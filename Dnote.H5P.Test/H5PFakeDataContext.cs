#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Dnote.H5P.NetFW.Linq2Sql.Interfaces;

namespace Dnote.H5P.Test
{
    public class H5PFakeDataContext : IH5PDataContext
    {
        private static readonly IList<object> _inMemoryDataStore = new List<object>();

        /// <summary>
        /// Counters for autoincrement ids
        /// </summary>
        private readonly Dictionary<Type, int> _ids = new Dictionary<Type, int>();

        private List<object>? _itemsAddedDuringSubmit;
        private List<ChildAssigner>? _delayedChildAssigners;
        private readonly List<object> _deletedItems = new List<object>();

        public string ConnectionString => "Fake Connection";

        public static void Clear()
        {
            _inMemoryDataStore.Clear();
        }

        public IQueryable<T> Repository<T>() where T : class
        {
            var query = from objects in _inMemoryDataStore
                        where objects is T
                        select objects;

            return query.Select(o => (T)o).AsQueryable();
        }

        private IQueryable<object> GetObjectsOfType(Type type)
        {
            var query = from objects in _inMemoryDataStore
                        where objects.GetType() == type
                        select objects;

            return query.AsQueryable();
        }

        public void Insert<T>(T item) where T : class
        {
            if (!_inMemoryDataStore.Contains(item))
            {
                _inMemoryDataStore.Add(item);
            }
        }

        public void Delete<T>(T item) where T : class
        {
            _inMemoryDataStore.Remove(item);

            _deletedItems.Add(item);
        }

        public void DeleteAll<T>(IEnumerable<T> items) where T : class
        {
            foreach (var item in items.ToList())
            {
                Delete(item);
            }
        }

        public bool Changed(object item)
        {
            return false; // TODO: How to detect changes in this in-memory context?
        }

        /// <summary>
        /// Submits the changes.
        /// </summary>
        /// <remarks>
        /// Since submitting is not required for the InMemory storage,
        /// we can invoke an event when we need to validate it has been called.
        /// </remarks>
        public void SubmitChanges(bool suppressErrors = false)
        {
            try
            {
                InvokeCompleted(EventArgs.Empty);

                var processed = new List<object>();

                _itemsAddedDuringSubmit = new List<object>();
                _delayedChildAssigners = new List<ChildAssigner>();

                foreach (var item in _inMemoryDataStore)
                {
                    ProcessObject(item, _ids, processed);
                }

                // put the added items in the repository (items that were added using IList.Add in the model)
                foreach (var item in _itemsAddedDuringSubmit)
                {
                    Insert(item);
                }

                // reassign the processed child object to force linq to assign their associated id properties
                foreach (var item in _delayedChildAssigners)
                {
                    if (!_deletedItems.Contains(item.ParentObject))
                    {
                        item.Property.SetValue(item.ParentObject, null);
                        if (!_deletedItems.Contains(item.ChildObject))
                        {
                            item.Property.SetValue(item.ParentObject, item.ChildObject);
                        }
                    }
                }

                _deletedItems.Clear();
            }
            catch (Exception)
            {
                if (!suppressErrors)
                {
                    throw;
                }
            }
        }

        private void ProcessObject(object item, Dictionary<Type, int> ids, List<object> processed)
        {
            if (_delayedChildAssigners == null)
            {
                throw new ArgumentNullException(nameof(_delayedChildAssigners));
            }

            if (_itemsAddedDuringSubmit == null)
            {
                throw new ArgumentNullException(nameof(_itemsAddedDuringSubmit));
            }

            if (processed.Contains(item))
            {
                return;
            }

            processed.Add(item);

            foreach (var property in item.GetType().GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    // determine its a database column field
                    if (attribute is System.Data.Linq.Mapping.ColumnAttribute)
                    {
                        var columnAttribute = (System.Data.Linq.Mapping.ColumnAttribute)attribute;
                        if (columnAttribute.IsPrimaryKey && columnAttribute.IsDbGenerated && property.PropertyType == typeof(int))
                        {
                            // auto generate primary key

                            if (!ids.ContainsKey(item.GetType()))
                            {
                                ids.Add(item.GetType(), 1);
                            }

                            var currentValue = (int)property.GetValue(item);
                            if (currentValue == 0)
                            {
                                currentValue = ids[item.GetType()];
                                property.SetValue(item, currentValue);
                                ids[item.GetType()]++;
                            }
                            else
                            {
                                if (currentValue >= ids[item.GetType()])
                                {
                                    ids[item.GetType()] = currentValue + 1;
                                }
                            }
                        }
                        else if (!columnAttribute.IsPrimaryKey)
                        {
                            // resolve id to child object 

                            // search if there is a child property that is connected to this property
                            var childProperty = FindChildObjectProperty(item, property.Name, out var otherKey);
                            if (childProperty != null)
                            {
                                // yes, now get the id of the property
                                var id = property.GetValue(item);

                                if (!Equals(id, 0) && !Equals(id, null))
                                {
                                    // and search for child objects of this type and id
                                    var objectsOfType = GetObjectsOfType(childProperty.PropertyType);
                                    foreach (var obj in objectsOfType)
                                    {
                                        var idProperty = obj.GetType().GetProperty(otherKey);
                                        if (idProperty != null)
                                        {
                                            var childId = idProperty.GetValue(obj);
                                            if (Equals(id, childId))
                                            {
                                                // found, now delay assign it
                                                if (!_deletedItems.Contains(obj))
                                                {
                                                    // to be able to reassign the child while finishing up
                                                    var assigner = new ChildAssigner
                                                    {
                                                        ParentObject = item,
                                                        ChildObject = obj,
                                                        Property = childProperty
                                                    };
                                                    _delayedChildAssigners.Add(assigner);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (property.PropertyType.GetInterfaces().Contains(typeof(IList)))
                {
                    var list = (IList)property.GetValue(item);
                    var toRemove = new List<object>();
                    foreach (var listItem in list)
                    {
                        ProcessObject(listItem, ids, processed);

                        // add items added to the list to the repository root list as well
                        if (!_inMemoryDataStore.Contains(listItem) && !_deletedItems.Contains(listItem))
                        {
                            _itemsAddedDuringSubmit.Add(listItem);
                        }

                        if (_deletedItems.Contains(listItem))
                        {
                            toRemove.Add(listItem);
                        }
                    }

                    if (toRemove.Any())
                    {
                        foreach (var itemToRemove in toRemove)
                        {
                            list.Remove(itemToRemove);
                        }
                    }
                }
                else if (property.PropertyType.GetInterfaces().Contains(typeof(INotifyPropertyChanging))) // a linq class object always implements this interface
                {
                    var child = property.GetValue(item);
                    if (child != null)
                    {
                        ProcessObject(child, ids, processed);

                        if (!_deletedItems.Contains(child))
                        {
                            // to be able to reassign the child while finishing up
                            var assigner = new ChildAssigner
                            {
                                ParentObject = item,
                                ChildObject = child,
                                Property = property
                            };
                            _delayedChildAssigners.Add(assigner);
                        }
                    }
                }
            }
        }

        private PropertyInfo? FindChildObjectProperty(object item, string propertyName, out string? otherKey)
        {
            foreach (var property in item.GetType().GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    if (attribute is System.Data.Linq.Mapping.AssociationAttribute)
                    {
                        var associationAttribute = (System.Data.Linq.Mapping.AssociationAttribute)attribute;
                        if (associationAttribute.ThisKey == propertyName)
                        {
                            otherKey = associationAttribute.OtherKey;
                            return property;
                        }
                    }
                }
            }

            otherKey = null;
            return null;
        }

        public event EventHandler? Completed;

        private void InvokeCompleted(EventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        public void SetCommandTimeOut(int secs)
        {
            // not supported by fake data context
        }

        public void ResetCommandTimeOut()
        {
            // not supported by fake data context
        }

        public void DiscardPendingChanges()
        {
            Clear();
        }
    }

    public class ChildAssigner
    {
        public object ParentObject = null!;
        public object ChildObject = null!;
        public PropertyInfo Property = null!;
    }
}
