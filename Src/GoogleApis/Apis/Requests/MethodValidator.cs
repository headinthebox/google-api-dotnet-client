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
using System.Text.RegularExpressions;
using Google.Apis.Discovery;
using Google.Apis.Requests;

namespace Google.Apis
{
    /// <summary>
    /// Logic for validating that a method is correct.
    /// </summary>
    public class MethodValidator
    {
        public MethodValidator(IMethod method, ParameterCollection parameters)
        {
            CurrentMethod = method;
            Parameters = parameters;
        }

        /// <summary>
        /// The method which is currently being validated.
        /// </summary>
        public IMethod CurrentMethod { get; private set; }

        /// <summary>
        /// The parameters of the method.
        /// </summary>
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Validates all the parameters provided.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public bool ValidateAllParameters()
        {
            var parameters = CurrentMethod.Parameters;
            // Itterate across all the parameters in the discovery document, and check them against supplied arguments.
            foreach (var parameter in parameters)
            {
                var parameterInfo = parameter.Value;

                if (ValidateParameter(parameterInfo) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates a parameter.
        /// 
        /// Checks to see if it is required, or if it needs regex validation.
        /// </summary>
        /// <param name="param">
        /// A <see cref="IParameter"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public bool ValidateParameter(IParameter param)
        {
            if (Parameters == null)
            {
                return false;
            }

            string currentParam;
            bool parameterPresent = Parameters.TryGetValue(param.Name, out currentParam);

            // If a required parameter is not present, fail.
            if (param.IsRequired && String.IsNullOrEmpty(currentParam))
            {
                return false;
            }

            if (parameterPresent == false || String.IsNullOrEmpty(currentParam))
            {
                // The parameter is not present in the input and is not required, skip validation.
                return true;
            }

            // The parameter is present, validate the regex.
            bool isValidData = ValidateRegex(param, currentParam);
            if (isValidData == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a parameter value against the methods regex.
        /// </summary>
        /// <param name="param">
        /// A <see cref="IParameter"/>
        /// </param>
        /// <param name="paramValue">
        /// A <see cref="System.String"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public bool ValidateRegex(IParameter param, string paramValue)
        {
            if (param.Pattern == null)
            {
                return true; // No Validation so anything is valid.
            }
            string pattern = param.Pattern;
            string stringValue = paramValue;

            Regex r = new Regex(pattern);

            return r.IsMatch(stringValue);
        }
    }
}