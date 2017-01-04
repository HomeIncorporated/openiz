﻿using MARC.HI.EHRS.SVC.Core;
using OpenIZ.Core.Model;
using OpenIZ.Core.Model.Attributes;
using OpenIZ.Core.Model.EntityLoader;
using OpenIZ.Core.Model.Interfaces;
using OpenIZ.Core.Model.Query;
using OpenIZ.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenIZ.Messaging.IMSI.Util
{
    /// <summary>
    /// Object expansion tool
    /// </summary>
    public static class ObjectExpander
    {
        // Trace source
        private static TraceSource m_tracer = new TraceSource("OpenIZ.Messaging.IMSI");

        // Sync lock
        private static Object s_syncLock = new object();

        // Related load methods
        private static Dictionary<Type, MethodInfo> m_relatedLoadMethods = new Dictionary<Type, MethodInfo>();

        // Reloated load association
        private static Dictionary<Type, MethodInfo> m_relatedLoadAssociations = new Dictionary<Type, MethodInfo>();


        /// <summary>
        /// Load related object
        /// </summary>
        internal static object LoadRelated(Type propertyType, Guid key)
        {
            MethodInfo methodInfo = null;
            if (!m_relatedLoadMethods.TryGetValue(propertyType, out methodInfo))
            {
                methodInfo = typeof(ObjectExpander).GetRuntimeMethod(nameof(LoadRelated), new Type[] { typeof(Guid) }).MakeGenericMethod(propertyType);
                lock (s_syncLock)
                    if (!m_relatedLoadMethods.ContainsKey(propertyType))
                        m_relatedLoadMethods.Add(propertyType, methodInfo);

            }
            return methodInfo.Invoke(null, new object[] { key });
        }

        /// <summary>
        /// Load collection
        /// </summary>
        internal static  IList LoadCollection(Type propertyType, IIdentifiedEntity entity)
        {
            MethodInfo methodInfo = null;

            var key = entity.Key;
            var versionKey = (entity as IVersionedEntity)?.VersionSequence;

            // Load
            if (!m_relatedLoadAssociations.TryGetValue(propertyType, out methodInfo))
            {
                if(versionKey.HasValue && typeof(IVersionedAssociation).IsAssignableFrom(propertyType.StripGeneric()))
                    methodInfo = typeof(ObjectExpander).GetRuntimeMethod(nameof(LoadCollection), new Type[] { typeof(Guid), typeof(decimal?) }).MakeGenericMethod(propertyType.StripGeneric());
                else
                    methodInfo = typeof(ObjectExpander).GetRuntimeMethod(nameof(LoadCollection), new Type[] { typeof(Guid) }).MakeGenericMethod(propertyType.StripGeneric());

                lock (s_syncLock)
                    if (!m_relatedLoadAssociations.ContainsKey(propertyType))
                        m_relatedLoadAssociations.Add(propertyType, methodInfo);

            }

            IList listValue = null;
            if(methodInfo.GetParameters().Length == 2)
                listValue = methodInfo.Invoke(null, new object[] { key, versionKey }) as IList;
            else
                listValue = methodInfo.Invoke(null, new object[] { key }) as IList;
            if (propertyType.GetTypeInfo().IsAssignableFrom(listValue.GetType().GetTypeInfo()))
                return listValue;
            else
            {
                var retVal = Activator.CreateInstance(propertyType, listValue);
                return retVal as IList;
            }

        }

        /// <summary>
        /// Delay loads the specified collection association
        /// </summary>
        public static IEnumerable<TAssociation> LoadCollection<TAssociation>(Guid sourceKey, Decimal? sourceSequence) where TAssociation : IdentifiedData, IVersionedAssociation, new()
        {
            return EntitySource.Current.Provider.GetRelations<TAssociation>(sourceKey, sourceSequence);
        }

        /// <summary>
        /// Delay loads the specified collection association
        /// </summary>
        public static IEnumerable<TAssociation> LoadCollection<TAssociation>(Guid sourceKey) where TAssociation : IdentifiedData, ISimpleAssociation, new()
        {
            return EntitySource.Current.Provider.GetRelations<TAssociation>(sourceKey);
        }

        /// <summary>
        /// Load the related information
        /// </summary>
        public static TRelated LoadRelated<TRelated>(Guid? objectKey) where TRelated : IdentifiedData, new()
        {
            if (objectKey.HasValue)
                return EntitySource.Current.Provider.Get<TRelated>(objectKey);
            else
                return default(TRelated);
        }


        /// <summary>
        /// Expand properties
        /// </summary>
        public static void ExpandProperties(IdentifiedData returnValue, NameValueCollection qp, Stack<Guid> keyStack = null, Dictionary<Guid, HashSet<String>> emptyCollections = null)
        {
            if (emptyCollections == null)
                emptyCollections = new Dictionary<Guid, HashSet<string>>();

            // Set the stack
            if (keyStack == null)
                keyStack = new Stack<Guid>();
            else if (keyStack.Contains(returnValue.Key.Value))
                return;

            keyStack.Push(returnValue.Key.Value);

            try
            {
                // Expand property?
                if (qp.ContainsKey("_expand") && qp.ContainsKey("_all"))
                    return;


                if (qp.ContainsKey("_all"))
                {
                    if (keyStack.Count > 3) return;

                    foreach (var pi in returnValue.GetType().GetRuntimeProperties().Where(o=>(o.GetCustomAttribute<SerializationReferenceAttribute>() != null || o.GetCustomAttributes<XmlElementAttribute>().Count() > 0) &&
                    o.GetCustomAttribute<DataIgnoreAttribute>() == null))
                    {
      
                        // Get current value
                        var scope = pi.GetValue(returnValue);
                        
                        // Force a load if null!!!
                        if(scope == null || (scope as IList)?.Count == 0)
                        {
                            if(typeof(IdentifiedData).IsAssignableFrom(pi.PropertyType))
                            {
                                var keyPi = pi.GetCustomAttribute<SerializationReferenceAttribute>()?.GetProperty(returnValue.GetType());
                                var keyValue = keyPi?.GetValue(returnValue);

                                // Get the value
                                if (keyValue != null)
                                    scope = LoadRelated(pi.PropertyType, (Guid)keyValue);
                                if (scope != null)
                                    pi.SetValue(returnValue, scope);
                            }
                            else if(typeof(IList).IsAssignableFrom(pi.PropertyType) && !pi.PropertyType.IsArray &&
                                typeof(IdentifiedData).IsAssignableFrom(pi.PropertyType.StripGeneric()))
                            {
                                // Already loaded?
                                HashSet<String> properties = null;
                                if (emptyCollections.TryGetValue(returnValue.Key.Value, out properties))
                                    if (!properties.Contains(pi.Name))
                                        properties.Add(pi.Name);
                                    else
                                        continue;
                                else
                                    emptyCollections.Add(returnValue.Key.Value, new HashSet<string>() { pi.Name });
                                    
                                scope = LoadCollection(pi.PropertyType, returnValue);
                                if ((scope as IList).Count > 0)
                                    pi.SetValue(returnValue, scope);
                            }
                        }

                        // Cascade
                        if (scope is IdentifiedData)
                            ExpandProperties(scope as IdentifiedData, qp, keyStack);
                        else if (scope is IList)
                            foreach (var itm in scope as IList)
                                if (itm is IdentifiedData)
                                    ExpandProperties(itm as IdentifiedData, qp, keyStack);
                    }

                    ApplicationContext.Current.GetService<IDataCachingService>().Add(returnValue);
                }
                else if (qp.ContainsKey("_expand"))
				{
					foreach (var nvs in qp["_expand"])
					{
						// Get the property the user wants to expand
						object scope = returnValue;
						foreach (var property in nvs.Split('.'))
						{
							if (scope is IList)
							{
								foreach (var sc in scope as IList)
								{
									// ensure the object is delay loaded 
									// in case the property we are looking for is null but it's associated reference key is populated
									(sc as IdentifiedData)?.SetDelayLoad(true);
									var keyPi = sc.GetType().GetProperties().SingleOrDefault(o => o.GetCustomAttributes<XmlElementAttribute>().FirstOrDefault()?.ElementName == property);

									if (keyPi == null)
									{
										continue;
									}

									// Get the backing property
									var expandProp = sc.GetType().GetProperties().SingleOrDefault(o => o.GetCustomAttributes<SerializationReferenceAttribute>().FirstOrDefault()?.RedirectProperty == keyPi.Name);

									scope = expandProp != null ? expandProp.GetValue(sc) : keyPi.GetValue(sc);
								}
							}
							else
							{
								PropertyInfo keyPi = scope.GetType().GetProperties().SingleOrDefault(o => o.GetCustomAttributes<XmlElementAttribute>().FirstOrDefault()?.ElementName == property);
								if (keyPi == null)
									continue;
								// Get the backing property
								PropertyInfo expandProp = scope.GetType().GetProperties().SingleOrDefault(o => o.GetCustomAttributes<SerializationReferenceAttribute>().FirstOrDefault()?.RedirectProperty == keyPi.Name);

								Object existing = null;
								Object keyValue = keyPi.GetValue(scope);

								if (expandProp != null && expandProp.CanWrite)
								{
									expandProp.SetValue(scope, existing);
									scope = existing;
								}
								else
								{
									if (expandProp != null)
										scope = expandProp.GetValue(scope);
									else
										scope = keyValue;
								}
							}
						}
					}

					ApplicationContext.Current.GetService<IDataCachingService>()?.Add(returnValue);
				}
            }
            finally
            {
                keyStack.Pop();
            }
        }
    }
}
