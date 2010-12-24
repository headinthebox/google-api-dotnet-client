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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
namespace Google.Apis.Json
{
  public class TokenStream {
	public static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TokenStream));	
		
    private const int BuilderBufferSize = 24;
    private TextReader reader = null;
    
    public TokenStream(string inputText) {
      reader = new StringReader(inputText);
    }

    public TokenStream(Stream inputStream) {
      reader = new StreamReader(inputStream);
    }

    /// <summary>
    /// From the current position, extract the next token
    /// </summary>
    /// <returns></returns>
    public JsonToken GetNextToken() {
      JsonToken token = new JsonToken();
      StringBuilder sb;
      try {
        // first skip over all whitespace.
        while (Char.IsWhiteSpace((char)reader.Peek())) {
          reader.Read();
        }

        if (reader.Peek() == -1)
          return null;

        char cur = (char)reader.Read();
        switch (cur) {
          case '{':
            token.type = JsonToken.Type.ObjectStart;
            break;
          case '}':
            token.type = JsonToken.Type.ObjectEnd;
            break;
          case '[':
            token.type = JsonToken.Type.ArrayStart;
            break;
          case ']':
            token.type = JsonToken.Type.ArrayEnd;
            break;
          case ':':
            token.type = JsonToken.Type.NameSeperator;
            break;
          case ',':
            token.type = JsonToken.Type.MemberSeperator;
            break;
          case '"':
          case '\'':
            char seperator = cur;
            token.type = JsonToken.Type.String;
            // let's read in the string
            sb = new StringBuilder(BuilderBufferSize);
            char next;
            while ((next = (char)reader.Read()) != seperator) {
              sb.Append(next);
            }
            token.value = sb.ToString();
            break;
          case 'f':
            GetFalseToken(token);
            break;
          case 't':
            // this might be the true token
            GetTrueToken(token);
            break;
          case 'n':
            // this might be nul
            GetNullToken(token);
            break;
          default:
            token.type = JsonToken.Type.Undefined;
            if (Char.IsNumber(cur)) {
              sb = new StringBuilder(BuilderBufferSize);
              sb.Append(cur);
              while (IsTokenSeperator((char)reader.Peek()) == false) {
                sb.Append((char)reader.Read());
              }
              token.value = sb.ToString();
			  decimal decNumber;
			  if(Decimal.TryParse(token.value, out decNumber)){
				token.number = decNumber;
				token.type = JsonToken.Type.Number;
			  }
            }
            break;
          

        }
      }
      catch (IOException) {
        return null;
      }
      return token;
    }

    // starts at the a of the false
    private void GetFalseToken(JsonToken token) {
      token.type = JsonToken.Type.Undefined;
      // this might be the false token
      if (LookupToken("alse") == true)
         token.type = JsonToken.Type.False;
      return;
    }
    // starts at the a of the false
    private void GetTrueToken(JsonToken token) {
      token.type = JsonToken.Type.Undefined;
      // this might be the false token
      if (LookupToken("rue") == true)
        token.type = JsonToken.Type.True;
      return;
    }
    // starts at the a of the false
    private void GetNullToken(JsonToken token) {
      token.type = JsonToken.Type.Undefined;
      // this might be the false token
      if (LookupToken("ull") == true)
        token.type = JsonToken.Type.Null;
      return;
    }
  
    private bool LookupToken(string tokenString) {
      char cur;
      foreach (char c in tokenString) {
        cur = (char) reader.Read();
        if (c != cur)
          return false;
      }
      cur = (char) reader.Peek();
      if (IsTokenSeperator(cur))
        return true;

      return false;
    }

    private bool IsTokenSeperator(char c) {
      if (Char.IsLetterOrDigit(c) == true)
        return false;
      return true;
    }
  }
}
