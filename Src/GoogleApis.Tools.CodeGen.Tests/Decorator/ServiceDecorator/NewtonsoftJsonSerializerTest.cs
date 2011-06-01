/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the ""License"");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an ""AS IS"" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.CodeDom;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Google.Apis.Tools.CodeGen.Decorator.ServiceDecorator;

namespace Google.Apis.Tools.CodeGen.Tests.Decorator.ServiceDecorator
{
    [TestFixture]
    public class NewtonsoftJsonSerializerTest
    {
        private void AssertContainsName(CodeTypeMemberCollection coll, string name)
        {
            foreach (CodeTypeMember member in coll)
            {
                if (name == member.Name)
                {
                    return;
                }
            }
            Assert.Fail("Failed to find [" + name + "] in CodeTypeMembers");
        }

        /// <summary>
        /// Tests the JsonSerializer @ Objects
        /// </summary>
        [Test]
        public void CreateJsonToObjectTest()
        {
            //public TOutput JsonToObject<TOutput>(Stream stream)
            NewtonsoftJsonSerializer decorator = new NewtonsoftJsonSerializer();
            CodeMemberMethod method = decorator.CreateJsonToObject();
            Assert.IsNotNull(method);
            Assert.AreEqual(MemberAttributes.Public, method.Attributes);
            Assert.AreEqual("JsonToObject", method.Name);
            Assert.IsNotEmpty(method.TypeParameters);
            Assert.AreEqual(1, method.TypeParameters.Count);
            Assert.IsNotEmpty(method.Parameters);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual(new CodeTypeReference(typeof(Stream)).BaseType, method.Parameters[0].Type.BaseType);
            Assert.IsNotEmpty(method.Statements);
        }

        /// <summary>
        /// Tests the JsonDeserializer
        /// </summary>
        [Test]
        public void CreateObjectToJsonTest()
        {
            NewtonsoftJsonSerializer decorator = new NewtonsoftJsonSerializer();
            CodeMemberMethod method = decorator.CreateObjectToJson();
            // public string ObjectToJson(object obj)
            Assert.IsNotNull(method);
            Assert.AreEqual(MemberAttributes.Public, method.Attributes);
            Assert.AreEqual(new CodeTypeReference(typeof(String)).BaseType, method.ReturnType.BaseType);
            Assert.AreEqual("ObjectToJson", method.Name);
            Assert.IsNotNull(method.Parameters);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual(new CodeTypeReference(typeof(object)).BaseType, method.Parameters[0].Type.BaseType);
            Assert.IsNotEmpty(method.Statements);
        }

        /// <summary>
        /// Tests the class decorator
        /// </summary>
        [Test]
        public void DecorateClassTest()
        {
            NewtonsoftJsonSerializer decorator = new NewtonsoftJsonSerializer();
            Assert.Throws(typeof(ArgumentNullException), () => decorator.DecorateClass(null, null));
            CodeTypeDeclaration declaration = new CodeTypeDeclaration("TestClass");
            decorator.DecorateClass(null, declaration);
            Assert.AreEqual(2, declaration.Members.Count);
            AssertContainsName(declaration.Members, "JsonToObject");
            AssertContainsName(declaration.Members, "ObjectToJson");
        }
    }
}