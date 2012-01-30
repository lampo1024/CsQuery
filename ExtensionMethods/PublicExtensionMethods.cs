﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.Script.Serialization;
using System.Dynamic;
using System.Text;
using System.Reflection;

using Jtc.CsQuery.ExtensionMethods.Internal;
using Jtc.CsQuery.Utility;

namespace Jtc.CsQuery.ExtensionMethods
{
    public static class PublicExtensionMethods
    {
        #region IEnumerable<T> extension methods
        public static void AddRange<T>(this ICollection<T> baseList, IEnumerable<T> list)
        {
            foreach (T obj in list)
            {
                baseList.Add(obj);
            }
        }
        #endregion
        #region string extension methods
        public static String RegexReplace(this String input, string pattern, string replacements)
        {
            return input.RegexReplace(Objects.Enumerate(pattern), Objects.Enumerate(replacements));
        }
        public static String RegexReplace(this String input, IEnumerable<string> patterns, IEnumerable<string> replacements)
        {
            List<string> patternList = new List<string>(patterns);
            List<string> replacementList = new List<string>(replacements);
            if (replacementList.Count != patternList.Count)
            {
                throw new ArgumentException("Mismatched pattern and replacement lists.");
            }

            for (var i = 0; i < patternList.Count; i++)
            {
                input = Regex.Replace(input, patternList[i], replacementList[i]);
            }

            return input;
        }
        public static string RegexReplace(this String input, string pattern, MatchEvaluator evaluator)
        {

            return Regex.Replace(input, pattern, evaluator);
        }
        public static bool RegexTest(this String input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
        /// <summary>
        /// Returns true when a value is "truthy" using similar logic as Javascript
        ///   null = false
        ///   empty string = false BUT zero string = true
        ///   zero numeric = false
        ///   false boolean values = false
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        #endregion

        #region object extension methods
        public static bool IsTruthy(this object obj)
        {
            return Objects.IsTruthy(obj);
        }
        #endregion


        /// <summary>
        /// Deep clone an enumerable. Deals with expando objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable CloneList(this IEnumerable obj)
        {
            return obj.CloneList(false);
        }
        public static IEnumerable CloneList(this IEnumerable obj, bool deep)
        {
            IEnumerable newList;
            // TODO - check for existence of a "clone" method
            //if (obj.GetType().IsArray)
            //{
            //    return (IEnumerable)((Array)obj).Clone();
            //} 
            if (obj.IsExpando())
            {
                newList = new JsObject();
                var newListDict = (IDictionary<string, object>)newList;
                foreach (var kvp in ((IDictionary<string, object>)obj))
                {
                    newListDict.Add(kvp.Key, deep ? Objects.CloneObject(kvp.Value,true) : kvp.Value);
                }
            }
            else
            {
                newList = new List<object>();
                foreach (var item in obj)
                {
                    ((List<object>)newList).Add(deep ? Objects.CloneObject(item, true) : item);
                }
            }
            return newList;
        }
        
        /// <summary>
        /// Serailize the object to a JSON string
        /// </summary>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static string ToJSON(this object objectToSerialize)
        {
            return JSON.ToJSON(objectToSerialize);
        }
        /// <summary>
        /// Deserialize the JSON string to a typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDeserialize"></param>
        /// <returns></returns>
        public static T ParseJSON<T>(this string objectToDeserialize)
        {
            return JSON.ParseJSON<T>(objectToDeserialize);
        }
        /// <summary>
        /// Deserialize the JSON string to an ExpandoObject or value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDeserialize"></param>
        /// <returns></returns>
        public static object ParseJSON(this string text)
        {
            return JSON.ParseJSON(text);
        }

        /// <summary>
        /// Convert an expandoobject to a list
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, object>> ToKvpList(this ExpandoObject obj)
        {
            var dict = ((IDictionary<string, object>)obj);
            return dict == null ? Objects.EmptyEnumerable<KeyValuePair<string, object>>() : dict.ToList();
        }
        public static bool HasProperty(this ExpandoObject obj, string propertyName)
        {
            return ((IDictionary<string, object>)obj).ContainsKey(propertyName);
        }
        /// <summary>
        /// Return typed value from an expandoobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(this ExpandoObject obj, string name)
        {
            if (obj == null)
            {
                return default(T);
            }
            var dict = (IDictionary<string, object>)obj;
            object val;
            if (dict.TryGetValue(name, out val))
            {
                return Objects.Convert<T>(val);
            }
            else
            {
                return default(T);
            }
        }
        public static JsObject Get(this ExpandoObject obj, string name)
        {
            IDictionary<string, object> dict = obj;
            object subProp;
            if (dict.TryGetValue(name, out subProp))
            {
                return CsQuery.ToExpando(subProp);
            }
            else
            {
                return null;
            }

        }



        /// <summary>
        /// Test if is an expando object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsExpando(this object obj)
        {
            return (obj is IDictionary<string, object>) ;
        }
        /// <summary>
        /// Returns true for expando objects with no properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsEmptyExpando(this object obj)
        {
            return obj.IsExpando() && ((IDictionary<string, object>)obj).Count == 0;
        }
        public static bool IsKeyValuePair(this object obj)
        {
            Type valueType = obj.GetType();
            if (valueType.IsGenericType)
            {
                Type baseType = valueType.GetGenericTypeDefinition();
                if (baseType == typeof(KeyValuePair<,>))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
}