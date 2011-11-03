﻿/*
Copyright 2011 Google Inc

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
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Discovery;
using Google.Apis.Tools.CodeGen.Generator;
using Google.Apis.Util;

namespace Google.Apis.Tools.CodeGen.Decorator.ResourceDecorator.RequestDecorator
{
    /// <summary>
    /// Decorator which adds a property for each parameter in the method to the request class.
    /// e.g.
    ///     <c>public string Name { get; set; }</c>
    /// </summary>
    public abstract class BaseParameterPropertyDecorator : IRequestDecorator
    {
        public abstract void DecorateClass(IResource resource,
                                  IMethod request,
                                  CodeTypeDeclaration requestClass,
                                  CodeTypeDeclaration resourceClass);

        internal CodeTypeMemberCollection GenerateParameterProperty(IParameter parameter,
                                                                    IMethod method,
                                                                    CodeTypeDeclaration resourceClass,
                                                                    IEnumerable<string> usedNames)
        {
            // Get the name and return type of this parameter.
            string name = parameter.Name;
            CodeTypeReference returnType = ResourceBaseGenerator.GetParameterTypeReference(
                resourceClass, parameter);

            // Generate the property and field.
            CodeTypeMemberCollection newMembers = DecoratorUtil.CreateAutoProperty(
                name, parameter.Description, returnType, usedNames, parameter.IsRequired);

            // Add the KeyAttribute to the property.
            foreach (CodeTypeMember member in newMembers)
            {
                CodeMemberProperty property = member as CodeMemberProperty;
                if (property == null)
                {
                    continue;
                }

                // Declare the RequestParameter attribute.
                CodeTypeReference attributeType = new CodeTypeReference(typeof(RequestParameterAttribute));
                CodeAttributeDeclaration attribute = new CodeAttributeDeclaration(attributeType);
                attribute.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(parameter.Name)));
                property.CustomAttributes.Add(attribute);
            }

            return newMembers;
        }
    }

    public class ParameterPropertyDecorator : BaseParameterPropertyDecorator
    {
        public override void DecorateClass(IResource resource,
                                  IMethod request,
                                  CodeTypeDeclaration requestClass,
                                  CodeTypeDeclaration resourceClass)
        {
            if (request.Parameters == null)
            {
                return; // Nothing to do here.
            }

            // Create a list of all used words based upon the existing resource class.
            IList<string> usedWords = new List<string>(GeneratorUtils.GetUsedWordsFromMembers(requestClass.Members));
            foreach (IParameter parameter in request.Parameters.Values)
            {
                // Generate and add the parameter properties.
                foreach (CodeTypeMember newMember in
                         GenerateParameterProperty(parameter, request, resourceClass, usedWords))
                {
                    requestClass.Members.Add(newMember);
                    usedWords.Add(newMember.Name);
                }
            }
        }
    }

    public class CommonParameterRequestDecorator : ParameterPropertyDecorator
    {
      public CommonParameterRequestDecorator(IDictionary<string, IParameter> parameters)
      {
          this.parameters = parameters;
      }

      private readonly IDictionary<string, IParameter> parameters;

      public override void DecorateClass(IResource resource,
                                  IMethod request,
                                  CodeTypeDeclaration requestClass,
                                  CodeTypeDeclaration resourceClass)
      {
        if (parameters == null || parameters.Count == 0)
        {
          return;
        }

        // Create a list of all used words based upon the existing resource class.
        IList<string> usedWords = new List<string>(GeneratorUtils.GetUsedWordsFromMembers(requestClass.Members));
        
        var filteredParams = parameters
          .Where(p => !request.Parameters.ContainsKey(p.Key))
          .Select(p => p.Value);

        foreach (IParameter parameter in filteredParams)
        {
          // Generate and add the parameter properties.
          foreach (CodeTypeMember newMember in
                   GenerateParameterProperty(parameter, request, resourceClass, usedWords))
          {
            requestClass.Members.Add(newMember);
            usedWords.Add(newMember.Name);
          }
        }
      }
    }
}