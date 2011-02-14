/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Apis.Json;
using Google.Apis.Requests;
namespace Google.Apis.Util
{
	public static class Utilities
	{
        /// <summary>
        /// Fetches an element from a dictionary in a safe way, returning null if there is no value present.
        /// </summary>
        public static TValue GetValueAsNull<TKey, TValue> (this IDictionary<TKey, TValue> data, TKey key)
		{
			TValue result;
			if (!data.TryGetValue (key, out result)) 
			{
				return default(TValue);
			}
			return result;
		}
        
        /// <summary>Extension method on object, which throws a ArgumentNullException if obj is null</summary>
        public static void ThrowIfNull(this object obj, string paramName)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
        
        public static void ThrowIfNullOrEmpty(this string str, string paramName)
        {
            str.ThrowIfNull(paramName);
            if ( str.Length == 0 )
            {
                throw new ArgumentException("Parameter was empty", paramName);
            }
        }
        
        public static void ThrowIfNullOrEmpty<T>(this ICollection<T> coll, string paramName)
        {
            coll.ThrowIfNull(paramName);
            if ( coll.Count == 0 )
            {
                throw new ArgumentException("Parameter was empty", paramName);
            }
        }
        
        public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            dict.ThrowIfNull("this");
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }
        
        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Length == 0;
        }
        
        public static bool IsNotNullOrEmpty(this string str)
        {
            return str != null && str.Length > 0;
        }
        
        public static bool IsNullOrEmpty<T>(this ICollection<T> coll)
        {
            return coll == null || coll.Count == 0;
        }
	}
}
