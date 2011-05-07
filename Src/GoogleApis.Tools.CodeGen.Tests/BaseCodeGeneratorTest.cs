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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Google.Apis.Discovery;
using Google.Apis.Json;

using log4net;
using NUnit.Framework;

namespace Google.Apis.Tools.CodeGen.Tests
{
	/// <summary>
	/// Is a base class for testing of code generators
	/// </summary>
	public abstract class BaseCodeGeneratorTest
	{
		public enum TestMethodNames
		{
			getTest,
			postTest,
			noParameterTest,
			oneOptionalParameterTest,
			oneRequiredParameterTest
		}

		public const string ServiceClassName = "Google.Apis.Tools.CodeGen.Tests.TestServiceClass";
		public const string ResourceClassName = "Google.Apis.Tools.CodeGen.Tests.TestResourceClass";
		public const string ResourceName = "TestResource";
		public const string ResourceAsJson = @"
		{
			""methods"":{
				""getTest"":{
					""pathUrl"":""activities/count"",
					""rpcName"":""chili.activities.count"",
					""httpMethod"":""GET"",
					""methodType"":""rest"",
					""parameters"":{
						""req_b"":{""parameterType"":""query"",""required"":true},
                        ""req_a"":{""parameterType"":""query"",""required"":true},
						""opt_b"":{""parameterType"":""query"",""required"":false},
						""opt_a"":{""parameterType"":""query"",""required"":false}
					}
				},
				""postTest"":{
					""pathUrl"":""activities/{userId}/{scope}/{postId}"",
					""rpcName"":""chili.activities.delete"",
					""httpMethod"":""POST"",
					""methodType"":""rest"",
					""parameters"":{
						""req_a"":{""parameterType"":""path"",""pattern"":"".*"",""required"":true},
						""req_c"":{""parameterType"":""path"",""pattern"":""[^/]+"",""required"":true},
						""opt_b"":{""parameterType"":""query"",""required"":false},
						""req_b"":{""parameterType"":""path"",""pattern"":""@.*"",""required"":true},
						""opt_a"":{""parameterType"":""query"",""required"":false}
					}
				},
				""noParameterTest"":{
					""pathUrl"":""activities/count"",
					""rpcName"":""chili.activities.count"",
					""httpMethod"":""GET"",
					""methodType"":""rest"",
					""parameters"":null
				},
				""oneOptionalParameterTest"":{
					""pathUrl"":""activities/count"",
					""rpcName"":""chili.activities.count"",
					""httpMethod"":""GET"",
					""methodType"":""rest"",
					""parameters"":{""opt_a"":{""parameterType"":""query"",""required"":false}}
				},
				""oneRequiredParameterTest"":{
					""pathUrl"":""activities/count"",
					""rpcName"":""chili.activities.count"",
					""httpMethod"":""GET"",
					""methodType"":""rest"",
					""parameters"":{""opt_a"":{""parameterType"":""query"",""required"":true}}
				}
			}
		}
		";
        
        private const string AdSenseV02AsJson = @"
{
 ""name"": ""adsense-mgmt"",
 ""version"": ""v1beta1"",
 ""description"": ""AdSense Management API"",
 ""restBasePath"": ""/adsense-mgmt/v1beta1/"",
 ""rpcPath"": ""/rpc"",
 ""resources"": {
  ""adunits"": {
   ""methods"": {
    ""list"": {
     ""restPath"": ""web_properties/{property_code}/ad_units"",
     ""rpcMethod"": ""adsensemgmt.adunits.list"",
     ""httpMethod"": ""GET"",
     ""parameters"": {
      ""property_code"": {
       ""restParameterType"": ""path"",
       ""pattern"": ""[^/]+"",
       ""required"": true
      }
     }
    }
   }
  },
  ""customchannels"": {
   ""methods"": {
    ""list"": {
     ""restPath"": ""web_properties/{property_code}/custom_channels"",
     ""rpcMethod"": ""adsensemgmt.customchannels.list"",
     ""httpMethod"": ""GET"",
     ""parameters"": {
      ""property_code"": {
       ""restParameterType"": ""path"",
       ""pattern"": ""[^/]+"",
       ""required"": true
      }
     }
    }
   }
  },
  ""reports"": {
   ""methods"": {
    ""generate"": {
     ""restPath"": ""reports/generate"",
     ""rpcMethod"": ""adsensemgmt.reports.generate"",
     ""httpMethod"": ""GET""
    }
   }
  },
  ""urlchannels"": {
   ""methods"": {
    ""list"": {
     ""restPath"": ""web_properties/{property_code}/url_channels"",
     ""rpcMethod"": ""adsensemgmt.urlchannels.list"",
     ""httpMethod"": ""GET"",
     ""parameters"": {
      ""property_code"": {
       ""restParameterType"": ""path"",
       ""pattern"": ""[^/]+"",
       ""required"": true
      }
     }
    }
   }
  },
  ""webproperties"": {
   ""methods"": {
    ""list"": {
     ""restPath"": ""web_properties"",
     ""rpcMethod"": ""adsensemgmt.webproperties.list"",
     ""httpMethod"": ""GET""
    }
   }
  }
 }
}";
            
        
        private const string AdSenseV02WithMultiLevelAsJson = @"
{
 ""name"": ""adsense-mgmt"",
 ""version"": ""v1beta1"",
 ""description"": ""AdSense Management API"",
 ""restBasePath"": ""/adsense-mgmt/v1beta1/"",
 ""rpcPath"": ""/rpc"",
 ""resources"": {
  ""mgmt"": {
   ""resources"": {
    ""adunits"": {
     ""methods"": {
      ""list"": {
       ""restPath"": ""web_properties/{property_code}/ad_units"",
       ""rpcMethod"": ""adsense.mgmt.adunits.list"",
       ""httpMethod"": ""GET"",
       ""parameters"": {
        ""property_code"": {
         ""restParameterType"": ""path"",
         ""pattern"": ""[^/]+"",
         ""required"": true
        }
       }
      }
     }
    },
    ""customchannels"": {
     ""methods"": {
      ""list"": {
       ""restPath"": ""web_properties/{property_code}/custom_channels"",
       ""rpcMethod"": ""adsense.mgmt.customchannels.list"",
       ""httpMethod"": ""GET"",
       ""parameters"": {
        ""property_code"": {
         ""restParameterType"": ""path"",
         ""pattern"": ""[^/]+"",
         ""required"": true
        }
       }
      }
     }
    },
    ""reports"": {
     ""methods"": {
      ""generate"": {
       ""restPath"": ""reports/generate"",
       ""rpcMethod"": ""adsense.mgmt.reports.generate"",
       ""httpMethod"": ""GET""
      }
     }
    },
    ""urlchannels"": {
     ""methods"": {
      ""list"": {
       ""restPath"": ""web_properties/{property_code}/url_channels"",
       ""rpcMethod"": ""adsense.mgmt.urlchannels.list"",
       ""httpMethod"": ""GET"",
       ""parameters"": {
        ""property_code"": {
         ""restParameterType"": ""path"",
         ""pattern"": ""[^/]+"",
         ""required"": true
        }
       }
      }
     }
    },
    ""webproperties"": {
     ""methods"": {
      ""list"": {
       ""restPath"": ""web_properties"",
       ""rpcMethod"": ""adsense.mgmt.webproperties.list"",
       ""httpMethod"": ""GET""
      }
     }
    }
   }
  }
 }
}
";

		public const string SimpleResource = @"
		{
			""methods"":{
				""simpleMethod"":{
					""pathUrl"":""activities/count"",
					""rpcName"":""chili.activities.count"",
					""httpMethod"":""GET"",
					""methodType"":""rest"",
					""parameters"":null
				}
			}
		}
		";
        
        #region BuzzServiceAsJson
		public const string BuzzServiceAsJson = @"
{
 ""kind"": ""discovery#describeItem"",
 ""name"": ""buzz"",
 ""version"": ""v1"",
 ""description"": ""Buzz API"",
 ""restBasePath"": ""/buzz/v1/"",
 ""rpcPath"": ""/rpc"",
 ""features"": [
  ""dataWrapper""
 ],
 ""schemas"": {
  ""Activity"": {
   ""id"": ""Activity"",
   ""type"": ""object"",
   ""properties"": {
    ""actor"": {
     ""type"": ""object"",
     ""properties"": {
      ""id"": {
       ""type"": ""any""
      },
      ""name"": {
       ""type"": ""any""
      },
      ""profileUrl"": {
       ""type"": ""any""
      },
      ""thumbnailUrl"": {
       ""type"": ""any""
      }
     }
    },
    ""address"": {
     ""type"": ""any""
    },
    ""annotation"": {
     ""type"": ""any""
    },
    ""categories"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""label"": {
        ""type"": ""any""
       },
       ""schema"": {
        ""type"": ""any""
       },
       ""term"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""crosspostSource"": {
     ""type"": ""any""
    },
    ""detectedlLang"": {
     ""type"": ""any""
    },
    ""geocode"": {
     ""type"": ""any""
    },
    ""id"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#activity""
    },
    ""links"": {
     ""type"": ""object"",
     ""properties"": {
      ""liked"": {
       ""type"": ""array"",
       ""items"": {
        ""type"": ""object"",
        ""properties"": {
         ""count"": {
          ""type"": ""integer""
         },
         ""href"": {
          ""type"": ""any""
         },
         ""type"": {
          ""type"": ""any""
         }
        }
       }
      }
     },
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""object"": {
     ""type"": ""object"",
     ""properties"": {
      ""actor"": {
       ""type"": ""object"",
       ""properties"": {
        ""id"": {
         ""type"": ""any""
        },
        ""name"": {
         ""type"": ""any""
        },
        ""profileUrl"": {
         ""type"": ""any""
        },
        ""thumbnailUrl"": {
         ""type"": ""any""
        }
       }
      },
      ""attachments"": {
       ""type"": ""array"",
       ""items"": {
        ""type"": ""object"",
        ""properties"": {
         ""content"": {
          ""type"": ""any""
         },
         ""id"": {
          ""type"": ""any""
         },
         ""links"": {
          ""type"": ""object"",
          ""additionalProperties"": {
           ""type"": ""array"",
           ""items"": {
            ""type"": ""object"",
            ""properties"": {
             ""count"": {
              ""type"": ""any""
             },
             ""height"": {
              ""type"": ""any""
             },
             ""href"": {
              ""type"": ""any""
             },
             ""title"": {
              ""type"": ""any""
             },
             ""type"": {
              ""type"": ""any""
             },
             ""updated"": {
              ""type"": ""string""
             },
             ""width"": {
              ""type"": ""any""
             }
            }
           }
          }
         },
         ""title"": {
          ""type"": ""any""
         },
         ""type"": {
          ""type"": ""string""
         }
        }
       }
      },
      ""comments"": {
       ""type"": ""array"",
       ""items"": {
        ""$ref"": ""Comment""
       }
      },
      ""content"": {
       ""type"": ""any""
      },
      ""detectedlLang"": {
       ""type"": ""any""
      },
      ""id"": {
       ""type"": ""any""
      },
      ""liked"": {
       ""type"": ""array"",
       ""items"": {
        ""$ref"": ""Person""
       }
      },
      ""links"": {
       ""type"": ""object"",
       ""additionalProperties"": {
        ""type"": ""array"",
        ""items"": {
         ""type"": ""object"",
         ""properties"": {
          ""href"": {
           ""type"": ""any""
          },
          ""type"": {
           ""type"": ""any""
          }
         }
        }
       }
      },
      ""originalContent"": {
       ""type"": ""any""
      },
      ""shareOriginal"": {
       ""$ref"": ""Activity""
      },
      ""targetLang"": {
       ""type"": ""any""
      },
      ""type"": {
       ""type"": ""string""
      },
      ""untranslatedContent"": {
       ""type"": ""any""
      }
     }
    },
    ""placeId"": {
     ""type"": ""any""
    },
    ""placeName"": {
     ""type"": ""any""
    },
    ""placeholder"": {
     ""type"": ""any""
    },
    ""published"": {
     ""type"": ""string""
    },
    ""radius"": {
     ""type"": ""any""
    },
    ""source"": {
     ""type"": ""object"",
     ""properties"": {
      ""title"": {
       ""type"": ""any""
      }
     }
    },
    ""targetLang"": {
     ""type"": ""any""
    },
    ""title"": {
     ""type"": ""any""
    },
    ""untranslatedTitle"": {
     ""type"": ""any""
    },
    ""updated"": {
     ""type"": ""string""
    },
    ""verbs"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""string""
     }
    },
    ""visibility"": {
     ""type"": ""object"",
     ""properties"": {
      ""entries"": {
       ""type"": ""array"",
       ""items"": {
        ""type"": ""object"",
        ""properties"": {
         ""id"": {
          ""type"": ""any""
         },
         ""title"": {
          ""type"": ""any""
         }
        }
       }
      }
     }
    }
   }
  },
  ""ActivityFeed"": {
   ""id"": ""ActivityFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""id"": {
     ""type"": ""any""
    },
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Activity""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#activityFeed""
    },
    ""links"": {
     ""type"": ""object"",
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""title"": {
     ""type"": ""any""
    },
    ""updated"": {
     ""type"": ""string""
    }
   }
  },
  ""Album"": {
   ""id"": ""Album"",
   ""type"": ""object"",
   ""properties"": {
    ""created"": {
     ""type"": ""string""
    },
    ""description"": {
     ""type"": ""string""
    },
    ""firstPhotoId"": {
     ""type"": ""integer""
    },
    ""id"": {
     ""type"": ""integer""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#album""
    },
    ""lastModified"": {
     ""type"": ""string""
    },
    ""links"": {
     ""type"": ""object"",
     ""properties"": {
      ""alternate"": {
       ""$ref"": ""Link""
      },
      ""enclosure"": {
       ""$ref"": ""Link""
      }
     }
    },
    ""owner"": {
     ""type"": ""object"",
     ""properties"": {
      ""id"": {
       ""type"": ""string""
      },
      ""name"": {
       ""type"": ""string""
      },
      ""profileUrl"": {
       ""type"": ""string""
      },
      ""thumbnailUrl"": {
       ""type"": ""string""
      }
     }
    },
    ""tags"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""string""
     }
    },
    ""title"": {
     ""type"": ""string""
    },
    ""version"": {
     ""type"": ""integer""
    }
   }
  },
  ""AlbumLite"": {
   ""id"": ""AlbumLite"",
   ""type"": ""object"",
   ""properties"": {
    ""collection"": {
     ""type"": ""object"",
     ""properties"": {
      ""album"": {
       ""type"": ""any""
      },
      ""albumId"": {
       ""type"": ""any""
      },
      ""photo"": {
       ""type"": ""object"",
       ""properties"": {
        ""photoUrl"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#albumLite""
    }
   }
  },
  ""AlbumsFeed"": {
   ""id"": ""AlbumsFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Album""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#albumsFeed""
    }
   }
  },
  ""ChiliPhotosResourceJson"": {
   ""id"": ""ChiliPhotosResourceJson"",
   ""type"": ""object"",
   ""properties"": {
    ""album"": {
     ""type"": ""object"",
     ""properties"": {
      ""id"": {
       ""type"": ""integer""
      },
      ""page_link"": {
       ""$ref"": ""Link""
      }
     }
    },
    ""created"": {
     ""type"": ""string""
    },
    ""description"": {
     ""type"": ""string""
    },
    ""fileSize"": {
     ""type"": ""integer""
    },
    ""id"": {
     ""type"": ""integer""
    },
    ""kind"": {
     ""type"": ""string""
    },
    ""lastModified"": {
     ""type"": ""string""
    },
    ""links"": {
     ""type"": ""object"",
     ""properties"": {
      ""alternate"": {
       ""type"": ""array"",
       ""items"": {
        ""$ref"": ""Link""
       }
      }
     },
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""$ref"": ""Link""
      }
     }
    },
    ""owner"": {
     ""type"": ""object"",
     ""properties"": {
      ""id"": {
       ""type"": ""string""
      },
      ""name"": {
       ""type"": ""string""
      },
      ""profileUrl"": {
       ""type"": ""string""
      },
      ""thumbnailUrl"": {
       ""type"": ""string""
      }
     }
    },
    ""timestamp"": {
     ""type"": ""number""
    },
    ""title"": {
     ""type"": ""string""
    },
    ""version"": {
     ""type"": ""integer""
    },
    ""video"": {
     ""$ref"": ""Video""
    }
   }
  },
  ""Comment"": {
   ""id"": ""Comment"",
   ""type"": ""object"",
   ""properties"": {
    ""actor"": {
     ""type"": ""object"",
     ""properties"": {
      ""id"": {
       ""type"": ""any""
      },
      ""name"": {
       ""type"": ""any""
      },
      ""profileUrl"": {
       ""type"": ""any""
      },
      ""thumbnailUrl"": {
       ""type"": ""any""
      }
     }
    },
    ""content"": {
     ""type"": ""any""
    },
    ""detectedLang"": {
     ""type"": ""any""
    },
    ""id"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#comment""
    },
    ""links"": {
     ""type"": ""object"",
     ""properties"": {
      ""inReplyTo"": {
       ""type"": ""array"",
       ""items"": {
        ""type"": ""object"",
        ""properties"": {
         ""href"": {
          ""type"": ""any""
         },
         ""ref"": {
          ""type"": ""any""
         },
         ""source"": {
          ""type"": ""any""
         }
        }
       }
      }
     },
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""originalContent"": {
     ""type"": ""any""
    },
    ""placeholder"": {
     ""type"": ""any""
    },
    ""published"": {
     ""type"": ""string""
    },
    ""targetLang"": {
     ""type"": ""any""
    },
    ""untranslatedContent"": {
     ""type"": ""any""
    },
    ""updated"": {
     ""type"": ""string""
    }
   }
  },
  ""CommentFeed"": {
   ""id"": ""CommentFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""id"": {
     ""type"": ""any""
    },
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Comment""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#commentFeed""
    },
    ""links"": {
     ""type"": ""object"",
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""title"": {
     ""type"": ""any""
    },
    ""updated"": {
     ""type"": ""string""
    }
   }
  },
  ""CountFeed"": {
   ""id"": ""CountFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""counts"": {
     ""type"": ""object"",
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""timestamp"": {
         ""type"": ""string""
        }
       }
      }
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#countFeed""
    }
   }
  },
  ""Entity"": {
   ""id"": ""Entity"",
   ""type"": ""object"",
   ""properties"": {
    ""chipsUiAcl"": {
     ""type"": ""any""
    },
    ""comment"": {
     ""type"": ""any""
    },
    ""id"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#entity""
    },
    ""starredBy"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Person""
     }
    },
    ""starredByViewer"": {
     ""type"": ""any""
    },
    ""summary"": {
     ""type"": ""any""
    },
    ""title"": {
     ""type"": ""any""
    },
    ""totalNumStars"": {
     ""type"": ""any""
    },
    ""viewerStarAcl"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""displayName"": {
        ""type"": ""any""
       },
       ""id"": {
        ""type"": ""any""
       },
       ""kind"": {
        ""type"": ""any""
       },
       ""tags"": {
        ""type"": ""array"",
        ""items"": {
         ""type"": ""any""
        }
       }
      }
     }
    }
   }
  },
  ""Group"": {
   ""id"": ""Group"",
   ""type"": ""object"",
   ""properties"": {
    ""id"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#group""
    },
    ""links"": {
     ""type"": ""object"",
     ""properties"": {
      ""self"": {
       ""type"": ""array"",
       ""items"": {
        ""type"": ""object"",
        ""properties"": {
         ""href"": {
          ""type"": ""any""
         },
         ""type"": {
          ""type"": ""string"",
          ""default"": ""application/json""
         }
        }
       }
      }
     }
    },
    ""memberCount"": {
     ""type"": ""any""
    },
    ""title"": {
     ""type"": ""any""
    }
   }
  },
  ""GroupFeed"": {
   ""id"": ""GroupFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Group""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#groupFeed""
    },
    ""links"": {
     ""type"": ""object"",
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    }
   }
  },
  ""Link"": {
   ""id"": ""Link"",
   ""type"": ""object"",
   ""properties"": {
    ""count"": {
     ""type"": ""integer""
    },
    ""height"": {
     ""type"": ""integer""
    },
    ""href"": {
     ""type"": ""string""
    },
    ""title"": {
     ""type"": ""string""
    },
    ""type"": {
     ""type"": ""string""
    },
    ""updated"": {
     ""type"": ""string""
    },
    ""width"": {
     ""type"": ""integer""
    }
   }
  },
  ""PeopleFeed"": {
   ""id"": ""PeopleFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""entry"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Person""
     }
    },
    ""itemsPerPage"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#peopleFeed""
    },
    ""startIndex"": {
     ""type"": ""any""
    },
    ""totalResults"": {
     ""type"": ""any""
    }
   }
  },
  ""Person"": {
   ""id"": ""Person"",
   ""type"": ""object"",
   ""properties"": {
    ""aboutMe"": {
     ""type"": ""any""
    },
    ""accounts"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""domain"": {
        ""type"": ""any""
       },
       ""userid"": {
        ""type"": ""any""
       },
       ""username"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""activities"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""addresses"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""country"": {
        ""type"": ""any""
       },
       ""formatted"": {
        ""type"": ""any""
       },
       ""locality"": {
        ""type"": ""any""
       },
       ""postalCode"": {
        ""type"": ""any""
       },
       ""primary"": {
        ""type"": ""any""
       },
       ""region"": {
        ""type"": ""any""
       },
       ""streetAddress"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""anniversary"": {
     ""type"": ""any""
    },
    ""birthday"": {
     ""type"": ""any""
    },
    ""bodyType"": {
     ""type"": ""any""
    },
    ""books"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""cars"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""children"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""connected"": {
     ""type"": ""any""
    },
    ""currentLocation"": {
     ""type"": ""any""
    },
    ""displayName"": {
     ""type"": ""any""
    },
    ""drinker"": {
     ""type"": ""any""
    },
    ""emails"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""primary"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       },
       ""value"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""ethnicity"": {
     ""type"": ""any""
    },
    ""fashion"": {
     ""type"": ""any""
    },
    ""food"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""gender"": {
     ""type"": ""any""
    },
    ""happiestWhen"": {
     ""type"": ""any""
    },
    ""hasApp"": {
     ""type"": ""any""
    },
    ""heroes"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""humor"": {
     ""type"": ""any""
    },
    ""id"": {
     ""type"": ""any""
    },
    ""ims"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""primary"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       },
       ""value"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""interests"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""jobInterests"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#person""
    },
    ""languages"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""languagesSpoken"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""livingArrangement"": {
     ""type"": ""any""
    },
    ""lookingFor"": {
     ""type"": ""any""
    },
    ""movies"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""music"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""name"": {
     ""type"": ""object"",
     ""properties"": {
      ""familyName"": {
       ""type"": ""any""
      },
      ""formatted"": {
       ""type"": ""any""
      },
      ""givenName"": {
       ""type"": ""any""
      },
      ""honorificPrefix"": {
       ""type"": ""any""
      },
      ""honorificSuffix"": {
       ""type"": ""any""
      },
      ""middleName"": {
       ""type"": ""any""
      }
     }
    },
    ""nickname"": {
     ""type"": ""any""
    },
    ""note"": {
     ""type"": ""any""
    },
    ""organizations"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""department"": {
        ""type"": ""any""
       },
       ""description"": {
        ""type"": ""any""
       },
       ""endDate"": {
        ""type"": ""any""
       },
       ""location"": {
        ""type"": ""any""
       },
       ""name"": {
        ""type"": ""any""
       },
       ""primary"": {
        ""type"": ""any""
       },
       ""startDate"": {
        ""type"": ""any""
       },
       ""title"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""pets"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""phoneNumbers"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""primary"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       },
       ""value"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""photos"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""height"": {
        ""type"": ""any""
       },
       ""primary"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       },
       ""value"": {
        ""type"": ""any""
       },
       ""width"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""politicalViews"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""preferredUsername"": {
     ""type"": ""any""
    },
    ""profileSong"": {
     ""type"": ""any""
    },
    ""profileUrl"": {
     ""type"": ""any""
    },
    ""profileVideo"": {
     ""type"": ""any""
    },
    ""published"": {
     ""type"": ""string""
    },
    ""quotes"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""relationshipStatus"": {
     ""type"": ""any""
    },
    ""relationships"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""religion"": {
     ""type"": ""any""
    },
    ""romance"": {
     ""type"": ""any""
    },
    ""scaredOf"": {
     ""type"": ""any""
    },
    ""sexualOrientation"": {
     ""type"": ""any""
    },
    ""smoker"": {
     ""type"": ""any""
    },
    ""sports"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""status"": {
     ""type"": ""any""
    },
    ""tags"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""thumbnailUrl"": {
     ""type"": ""any""
    },
    ""turnOffs"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""turnOns"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""tvShows"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""any""
     }
    },
    ""updated"": {
     ""type"": ""string""
    },
    ""urls"": {
     ""type"": ""array"",
     ""items"": {
      ""type"": ""object"",
      ""properties"": {
       ""primary"": {
        ""type"": ""any""
       },
       ""type"": {
        ""type"": ""any""
       },
       ""value"": {
        ""type"": ""any""
       }
      }
     }
    },
    ""utcOffset"": {
     ""type"": ""any""
    }
   }
  },
  ""PhotosFeed"": {
   ""id"": ""PhotosFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""ChiliPhotosResourceJson""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#photosFeed""
    }
   }
  },
  ""Related"": {
   ""id"": ""Related"",
   ""type"": ""object"",
   ""properties"": {
    ""href"": {
     ""type"": ""any""
    },
    ""id"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#related""
    },
    ""summary"": {
     ""type"": ""any""
    },
    ""title"": {
     ""type"": ""any""
    }
   }
  },
  ""RelatedFeed"": {
   ""id"": ""RelatedFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""id"": {
     ""type"": ""any""
    },
    ""items"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Related""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#relatedFeed""
    },
    ""links"": {
     ""type"": ""object"",
     ""additionalProperties"": {
      ""type"": ""array"",
      ""items"": {
       ""type"": ""object"",
       ""properties"": {
        ""count"": {
         ""type"": ""any""
        },
        ""height"": {
         ""type"": ""any""
        },
        ""href"": {
         ""type"": ""any""
        },
        ""title"": {
         ""type"": ""any""
        },
        ""type"": {
         ""type"": ""any""
        },
        ""updated"": {
         ""type"": ""string""
        },
        ""width"": {
         ""type"": ""any""
        }
       }
      }
     }
    },
    ""title"": {
     ""type"": ""any""
    },
    ""updated"": {
     ""type"": ""string""
    }
   }
  },
  ""StarredEntityFeed"": {
   ""id"": ""StarredEntityFeed"",
   ""type"": ""object"",
   ""properties"": {
    ""entry"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Entity""
     }
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#starredEntityFeed""
    }
   }
  },
  ""StarredEntityFeedForUser"": {
   ""id"": ""StarredEntityFeedForUser"",
   ""type"": ""object"",
   ""properties"": {
    ""entry"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Entity""
     }
    },
    ""itemsPerPage"": {
     ""type"": ""any""
    },
    ""kind"": {
     ""type"": ""string"",
     ""default"": ""buzz#starredEntityFeedForUser""
    },
    ""startIndex"": {
     ""type"": ""any""
    },
    ""totalResults"": {
     ""type"": ""any""
    }
   }
  },
  ""Video"": {
   ""id"": ""Video"",
   ""type"": ""object"",
   ""properties"": {
    ""duration"": {
     ""type"": ""integer""
    },
    ""size"": {
     ""type"": ""integer""
    },
    ""streams"": {
     ""type"": ""array"",
     ""items"": {
      ""$ref"": ""Link""
     }
    }
   }
  }
 },
 ""resources"": {
  ""activities"": {
   ""methods"": {
    ""count"": {
     ""restPath"": ""activities/count"",
     ""rpcMethod"": ""chili.activities.count"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a count of link shares"",
     ""parameters"": {
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""url"": {
       ""restParameterType"": ""query"",
       ""repeated"": true,
       ""description"": ""URLs for which to get share counts."",
       ""type"": ""string""
      }
     },
     ""response"": {
      ""$ref"": ""CountFeed""
     }
    },
    ""delete"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}"",
     ""rpcMethod"": ""chili.activities.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Delete an activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity to delete."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection to which the activity belongs."",
       ""type"": ""string"",
       ""enum"": [
        ""@liked"",
        ""@muted"",
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Activities liked by the user."",
        ""Activities muted by the user."",
        ""Activities posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user whose post to delete."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId""
     ]
    },
    ""extractPeopleFromSearch"": {
     ""restPath"": ""activities/search/@people"",
     ""rpcMethod"": ""chili.activities.extractPeopleFromSearch"",
     ""httpMethod"": ""GET"",
     ""description"": ""Search for people by topic"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""bbox"": {
       ""restParameterType"": ""query"",
       ""description"": ""Bounding box to use in a geographic location query."",
       ""type"": ""string""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""lat"": {
       ""restParameterType"": ""query"",
       ""description"": ""Latitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""lon"": {
       ""restParameterType"": ""query"",
       ""description"": ""Longitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""pid"": {
       ""restParameterType"": ""query"",
       ""description"": ""ID of a place to use in a geographic location query."",
       ""type"": ""string""
      },
      ""q"": {
       ""restParameterType"": ""query"",
       ""description"": ""Full-text search query string."",
       ""type"": ""string""
      },
      ""radius"": {
       ""restParameterType"": ""query"",
       ""description"": ""Radius to use in a geographic location query."",
       ""type"": ""string""
      }
     },
     ""response"": {
      ""$ref"": ""PeopleFeed""
     }
    },
    ""get"": {
     ""restPath"": ""activities/{userId}/@self/{postId}"",
     ""rpcMethod"": ""chili.activities.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get an activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-comments"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of comments to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295""
      },
      ""max-liked"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of likes to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""0""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the post to get."",
       ""type"": ""string""
      },
      ""truncateAtom"": {
       ""restParameterType"": ""query"",
       ""description"": ""Truncate the value of the atom:content element."",
       ""type"": ""boolean""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user whose post to get."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""postId""
     ],
     ""response"": {
      ""$ref"": ""Activity""
     }
    },
    ""insert"": {
     ""restPath"": ""activities/{userId}/@self"",
     ""rpcMethod"": ""chili.activities.insert"",
     ""httpMethod"": ""POST"",
     ""description"": ""Create a new activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""preview"": {
       ""restParameterType"": ""query"",
       ""description"": ""If true, only preview the action."",
       ""type"": ""boolean"",
       ""default"": ""false""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId""
     ],
     ""request"": {
      ""$ref"": ""Activity""
     },
     ""response"": {
      ""$ref"": ""Activity""
     }
    },
    ""list"": {
     ""restPath"": ""activities/{userId}/{scope}"",
     ""rpcMethod"": ""chili.activities.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""List activities"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-comments"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of comments to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295""
      },
      ""max-liked"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of likes to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""0""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection of activities to list."",
       ""type"": ""string"",
       ""enum"": [
        ""@comments"",
        ""@consumption"",
        ""@liked"",
        ""@public"",
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Limit to activities commented on by the user."",
        ""Limit to activities to be consumed by the user."",
        ""Limit to activities liked by the user."",
        ""Limit to public activities posted by the user."",
        ""Limit to activities posted by the user.""
       ]
      },
      ""truncateAtom"": {
       ""restParameterType"": ""query"",
       ""description"": ""Truncate the value of the atom:content element."",
       ""type"": ""boolean""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope""
     ],
     ""response"": {
      ""$ref"": ""ActivityFeed""
     }
    },
    ""search"": {
     ""restPath"": ""activities/search"",
     ""rpcMethod"": ""chili.activities.search"",
     ""httpMethod"": ""GET"",
     ""description"": ""Search for activities"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""bbox"": {
       ""restParameterType"": ""query"",
       ""description"": ""Bounding box to use in a geographic location query."",
       ""type"": ""string""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""lat"": {
       ""restParameterType"": ""query"",
       ""description"": ""Latitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""lon"": {
       ""restParameterType"": ""query"",
       ""description"": ""Longitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""pid"": {
       ""restParameterType"": ""query"",
       ""description"": ""ID of a place to use in a geographic location query."",
       ""type"": ""string""
      },
      ""q"": {
       ""restParameterType"": ""query"",
       ""description"": ""Full-text search query string."",
       ""type"": ""string""
      },
      ""radius"": {
       ""restParameterType"": ""query"",
       ""description"": ""Radius to use in a geographic location query."",
       ""type"": ""string""
      },
      ""truncateAtom"": {
       ""restParameterType"": ""query"",
       ""description"": ""Truncate the value of the atom:content element."",
       ""type"": ""boolean""
      }
     },
     ""response"": {
      ""$ref"": ""ActivityFeed""
     }
    },
    ""track"": {
     ""restPath"": ""activities/track"",
     ""rpcMethod"": ""chili.activities.track"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get real-time activity tracking information"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""bbox"": {
       ""restParameterType"": ""query"",
       ""description"": ""Bounding box to use in a geographic location query."",
       ""type"": ""string""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""lat"": {
       ""restParameterType"": ""query"",
       ""description"": ""Latitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""lon"": {
       ""restParameterType"": ""query"",
       ""description"": ""Longitude to use in a geographic location query."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""pid"": {
       ""restParameterType"": ""query"",
       ""description"": ""ID of a place to use in a geographic location query."",
       ""type"": ""string""
      },
      ""q"": {
       ""restParameterType"": ""query"",
       ""description"": ""Full-text search query string."",
       ""type"": ""string""
      },
      ""radius"": {
       ""restParameterType"": ""query"",
       ""description"": ""Radius to use in a geographic location query."",
       ""type"": ""string""
      }
     },
     ""response"": {
      ""$ref"": ""ActivityFeed""
     }
    },
    ""update"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}"",
     ""rpcMethod"": ""chili.activities.update"",
     ""httpMethod"": ""PUT"",
     ""description"": ""Update an activity"",
     ""parameters"": {
      ""abuseType"": {
       ""restParameterType"": ""query"",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity to update."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection to which the activity belongs."",
       ""type"": ""string"",
       ""enum"": [
        ""@abuse"",
        ""@liked"",
        ""@muted"",
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Activities reported by the user."",
        ""Activities liked by the user."",
        ""Activities muted by the user."",
        ""Activities posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user whose post to update."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId""
     ],
     ""request"": {
      ""$ref"": ""Activity""
     },
     ""response"": {
      ""$ref"": ""Activity""
     }
    }
   }
  },
  ""comments"": {
   ""methods"": {
    ""delete"": {
     ""restPath"": ""activities/{userId}/@self/{postId}/@comments/{commentId}"",
     ""rpcMethod"": ""chili.comments.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Delete a comment"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""commentId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the comment being referenced."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity for which to delete the comment."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""postId"",
      ""commentId""
     ]
    },
    ""get"": {
     ""restPath"": ""activities/{userId}/@self/{postId}/@comments/{commentId}"",
     ""rpcMethod"": ""chili.comments.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a comment"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""commentId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the comment being referenced."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity for which to get comments."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""postId"",
      ""commentId""
     ],
     ""response"": {
      ""$ref"": ""Comment""
     }
    },
    ""insert"": {
     ""restPath"": ""activities/{userId}/@self/{postId}/@comments"",
     ""rpcMethod"": ""chili.comments.insert"",
     ""httpMethod"": ""POST"",
     ""description"": ""Create a comment"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity on which to comment."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user on whose behalf to comment."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""postId""
     ],
     ""request"": {
      ""$ref"": ""Comment""
     },
     ""response"": {
      ""$ref"": ""Comment""
     }
    },
    ""list"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}/@comments"",
     ""rpcMethod"": ""chili.comments.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""List comments"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity for which to get comments."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection to which the activity belongs."",
       ""type"": ""string"",
       ""enum"": [
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Activities posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user for whose post to get comments."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId""
     ],
     ""response"": {
      ""$ref"": ""CommentFeed""
     }
    },
    ""update"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}/@comments/{commentId}"",
     ""rpcMethod"": ""chili.comments.update"",
     ""httpMethod"": ""PUT"",
     ""description"": ""Update a comment"",
     ""parameters"": {
      ""abuseType"": {
       ""restParameterType"": ""query"",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""commentId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the comment being referenced."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity for which to update the comment."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection to which the activity belongs."",
       ""type"": ""string"",
       ""enum"": [
        ""@abuse"",
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Comments reported by the user."",
        ""Comments posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId"",
      ""commentId""
     ],
     ""request"": {
      ""$ref"": ""Comment""
     },
     ""response"": {
      ""$ref"": ""Comment""
     }
    }
   }
  },
  ""groups"": {
   ""methods"": {
    ""delete"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}"",
     ""rpcMethod"": ""chili.groups.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Delete a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group to delete."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId""
     ]
    },
    ""get"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}/@self"",
     ""rpcMethod"": ""chili.groups.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group to get."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId""
     ],
     ""response"": {
      ""$ref"": ""Group""
     }
    },
    ""insert"": {
     ""restPath"": ""people/{userId}/@groups"",
     ""rpcMethod"": ""chili.groups.insert"",
     ""httpMethod"": ""POST"",
     ""description"": ""Create a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId""
     ],
     ""request"": {
      ""$ref"": ""Group""
     },
     ""response"": {
      ""$ref"": ""Group""
     }
    },
    ""list"": {
     ""restPath"": ""people/{userId}/@groups"",
     ""rpcMethod"": ""chili.groups.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a user's groups"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId""
     ],
     ""response"": {
      ""$ref"": ""GroupFeed""
     }
    },
    ""update"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}/@self"",
     ""rpcMethod"": ""chili.groups.update"",
     ""httpMethod"": ""PUT"",
     ""description"": ""Update a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group to update."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId""
     ],
     ""request"": {
      ""$ref"": ""Group""
     },
     ""response"": {
      ""$ref"": ""Group""
     }
    }
   }
  },
  ""people"": {
   ""methods"": {
    ""delete"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}/{personId}"",
     ""rpcMethod"": ""chili.people.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Remove a person from a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group from which to remove the person."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""personId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the person to remove from the group."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the owner of the group."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId"",
      ""personId""
     ]
    },
    ""get"": {
     ""restPath"": ""people/{userId}/@self"",
     ""rpcMethod"": ""chili.people.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a user profile"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId""
     ],
     ""response"": {
      ""$ref"": ""Person""
     }
    },
    ""liked"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}/{groupId}"",
     ""rpcMethod"": ""chili.people.liked"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get people who liked an activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""type"": ""string"",
       ""enum"": [
        ""@liked""
       ],
       ""enumDescriptions"": [
        ""People who liked this activity.""
       ]
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity that was liked."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId"",
      ""groupId""
     ],
     ""response"": {
      ""$ref"": ""PeopleFeed""
     }
    },
    ""list"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}"",
     ""rpcMethod"": ""chili.people.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get people in a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group for which to list users."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId""
     ],
     ""response"": {
      ""$ref"": ""PeopleFeed""
     }
    },
    ""reshared"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}/{groupId}"",
     ""rpcMethod"": ""chili.people.reshared"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get people who reshared an activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""type"": ""string"",
       ""enum"": [
        ""@reshared""
       ],
       ""enumDescriptions"": [
        ""People who reshared this activity.""
       ]
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity that was reshared."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId"",
      ""groupId""
     ],
     ""response"": {
      ""$ref"": ""PeopleFeed""
     }
    },
    ""search"": {
     ""restPath"": ""people/search"",
     ""rpcMethod"": ""chili.people.search"",
     ""httpMethod"": ""GET"",
     ""description"": ""Search for people"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""q"": {
       ""restParameterType"": ""query"",
       ""description"": ""Full-text search query string."",
       ""type"": ""string""
      }
     },
     ""response"": {
      ""$ref"": ""PeopleFeed""
     }
    },
    ""update"": {
     ""restPath"": ""people/{userId}/@groups/{groupId}/{personId}"",
     ""rpcMethod"": ""chili.people.update"",
     ""httpMethod"": ""PUT"",
     ""description"": ""Add a person to a group"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""groupId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the group to which to add the person."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""personId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the person to add to the group."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the owner of the group."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""groupId"",
      ""personId""
     ],
     ""request"": {
      ""$ref"": ""Person""
     },
     ""response"": {
      ""$ref"": ""Person""
     }
    }
   }
  },
  ""photoAlbums"": {
   ""methods"": {
    ""delete"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}"",
     ""rpcMethod"": ""chili.photoAlbums.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Delete a photo album"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album to delete."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId""
     ]
    },
    ""get"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}"",
     ""rpcMethod"": ""chili.photoAlbums.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a photo album"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album to get."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId""
     ],
     ""response"": {
      ""$ref"": ""Album""
     }
    },
    ""insert"": {
     ""restPath"": ""photos/{userId}/@self"",
     ""rpcMethod"": ""chili.photoAlbums.insert"",
     ""httpMethod"": ""POST"",
     ""description"": ""Create a photo album"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId""
     ],
     ""request"": {
      ""$ref"": ""Album""
     },
     ""response"": {
      ""$ref"": ""Album""
     }
    },
    ""list"": {
     ""restPath"": ""photos/{userId}/{scope}"",
     ""rpcMethod"": ""chili.photoAlbums.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""List a user's photo albums"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection of albums to list."",
       ""type"": ""string"",
       ""enum"": [
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Albums posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope""
     ],
     ""response"": {
      ""$ref"": ""AlbumsFeed""
     }
    }
   }
  },
  ""photos"": {
   ""methods"": {
    ""delete"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}/@photos/{photoId}"",
     ""rpcMethod"": ""chili.photos.delete"",
     ""httpMethod"": ""DELETE"",
     ""description"": ""Delete a photo"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album to which to photo belongs."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""photoId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the photo to delete."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId"",
      ""photoId""
     ]
    },
    ""get"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}/@photos/{photoId}"",
     ""rpcMethod"": ""chili.photos.get"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get photo metadata"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the photo for which to get metadata."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""photoId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album containing the photo."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId"",
      ""photoId""
     ],
     ""response"": {
      ""$ref"": ""ChiliPhotosResourceJson""
     }
    },
    ""insert"": {
     ""restPath"": ""photos/{userId}/{albumId}"",
     ""rpcMethod"": ""chili.photos.insert"",
     ""httpMethod"": ""POST"",
     ""description"": ""Upload a photo to an album"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album to which to upload."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId""
     ],
     ""request"": {
      ""$ref"": ""AlbumLite""
     },
     ""response"": {
      ""$ref"": ""AlbumLite""
     }
    },
    ""insert2"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}/@photos"",
     ""rpcMethod"": ""chili.photos.insert2"",
     ""httpMethod"": ""POST"",
     ""description"": ""Upload a photo to an album"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album to which to upload."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId""
     ],
     ""request"": {
      ""$ref"": ""ChiliPhotosResourceJson""
     },
     ""response"": {
      ""$ref"": ""ChiliPhotosResourceJson""
     }
    },
    ""listByAlbum"": {
     ""restPath"": ""photos/{userId}/@self/{albumId}/@photos"",
     ""rpcMethod"": ""chili.photos.listByAlbum"",
     ""httpMethod"": ""GET"",
     ""description"": ""List photos in an album"",
     ""parameters"": {
      ""albumId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the album for which to list photos."",
       ""type"": ""string""
      },
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""albumId""
     ],
     ""response"": {
      ""$ref"": ""PhotosFeed""
     }
    },
    ""listByScope"": {
     ""restPath"": ""photos/{userId}/@self/{scope}/@photos"",
     ""rpcMethod"": ""chili.photos.listByScope"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get a user's photos"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""c"": {
       ""restParameterType"": ""query"",
       ""description"": ""A continuation token that allows pagination."",
       ""type"": ""string""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""max-results"": {
       ""restParameterType"": ""query"",
       ""description"": ""Maximum number of results to include."",
       ""type"": ""integer"",
       ""minimum"": ""0"",
       ""maximum"": ""4294967295"",
       ""default"": ""20""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection of photos to list."",
       ""type"": ""string"",
       ""enum"": [
        ""@recent""
       ],
       ""enumDescriptions"": [
        ""Recent photos uploaded by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope""
     ],
     ""response"": {
      ""$ref"": ""PhotosFeed""
     }
    }
   }
  },
  ""related"": {
   ""methods"": {
    ""list"": {
     ""restPath"": ""activities/{userId}/{scope}/{postId}/@related"",
     ""rpcMethod"": ""chili.related.list"",
     ""httpMethod"": ""GET"",
     ""description"": ""Get related links for an activity"",
     ""parameters"": {
      ""alt"": {
       ""restParameterType"": ""query"",
       ""description"": ""Specifies an alternative representation type."",
       ""type"": ""string"",
       ""enum"": [
        ""atom"",
        ""json""
       ],
       ""enumDescriptions"": [
        ""Use Atom XML format"",
        ""Use JSON format""
       ],
       ""default"": ""atom""
      },
      ""hl"": {
       ""restParameterType"": ""query"",
       ""description"": ""Language code to limit language results."",
       ""type"": ""string""
      },
      ""postId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the activity to which to get related links."",
       ""type"": ""string""
      },
      ""scope"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""The collection to which the activity belongs."",
       ""type"": ""string"",
       ""enum"": [
        ""@self""
       ],
       ""enumDescriptions"": [
        ""Activities posted by the user.""
       ]
      },
      ""userId"": {
       ""restParameterType"": ""path"",
       ""required"": true,
       ""description"": ""ID of the user being referenced."",
       ""type"": ""string""
      }
     },
     ""parameterOrder"": [
      ""userId"",
      ""scope"",
      ""postId""
     ],
     ""response"": {
      ""$ref"": ""RelatedFeed""
     }
    }
   }
  }
 }
}

";
        #endregion

		public static KeyValuePair<string, object> CreateJsonResourceDefinition (string resourceName, string jsonString)
		{
			JsonDictionary json = (JsonDictionary)JsonReader.Parse (jsonString);
			
			return new KeyValuePair<string, object> (resourceName, json);
		}

		public static IResource CreateResourceDivcoveryV_1_0 (string resourceName, string json)
		{
			return new ResourceV1_0 (DiscoveryVersion.Version_1_0, CreateJsonResourceDefinition (resourceName, json));
		}

		protected void AddRefereenceToDelararingAssembly (Type target, CompilerParameters cp)
		{
			cp.ReferencedAssemblies.Add (target.Assembly.CodeBase);
		}

		protected void CheckCompile (CodeTypeDeclaration codeType, bool warnAsError, string errorMessage)
		{
			CodeCompileUnit compileUnit = new CodeCompileUnit ();
			var client = new CodeNamespace ("Google.Apis.Tools.CodeGen.Tests");
			compileUnit.Namespaces.Add (client);
			client.Types.Add (codeType);
			
			CheckCompile (compileUnit, warnAsError, errorMessage);
		}

		protected void CheckCompile (CodeCompileUnit codeCompileUnit, bool warnAsError, string errorMessage)
		{
			var language = "CSharp";
			var provider = CodeDomProvider.CreateProvider (language);
			CompilerParameters cp = new CompilerParameters ();
			// Add an assembly reference.
			cp.ReferencedAssemblies.Add ("System.dll");
			AddRefereenceToDelararingAssembly (typeof(DiscoveryService), cp);
			AddRefereenceToDelararingAssembly (typeof(ILog), cp);
            AddRefereenceToDelararingAssembly (typeof(Newtonsoft.Json.JsonSerializer), cp);
			
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.TreatWarningsAsErrors = warnAsError;
			// Warnings are errors.
			CompilerResults compilerResults = provider.CompileAssemblyFromDom (cp, codeCompileUnit);
			bool hasError = false;
			if (compilerResults.Errors.Count > 0) {
				var sb = new StringBuilder (errorMessage).AppendLine ();
				foreach (CompilerError error in compilerResults.Errors) {
					sb.AppendLine (error.ToString ());
					if (error.IsWarning == false || warnAsError) {
						hasError = true;
					}
				}
				sb.AppendLine ();
				sb.AppendLine ("Generated Code Follows");
				
				using (StringWriter sw = new StringWriter (sb)) {
					IndentedTextWriter tw = new IndentedTextWriter (sw);
					provider.GenerateCodeFromCompileUnit (codeCompileUnit, tw, new CodeGeneratorOptions ());
				}
				Console.Out.WriteLine (sb.ToString ());
				
				if (hasError) {
					Assert.Fail (sb.ToString ());
				}
			}
		}
		
		protected IService CreateBuzzService()
		{
			var version = "v1";
			var buzzTestFetcher = new StringDiscoveryDevice(){Document = BuzzServiceAsJson};
			var discovery = new DiscoveryService(buzzTestFetcher);
			// Build the service based on discovery information.
			return discovery.GetService(version, DiscoveryVersion.Version_0_3,
                new FactoryParameterV0_3("http://test.sever.example.com", "http://test.sever.example.com/testService"));
		}
        
        protected IService CreateAdSenseV1_0Service()
        {
            var version = "v1beta1";
            var buzzTestFetcher = new StringDiscoveryDevice(){Document = AdSenseV02AsJson};
            var discovery = new DiscoveryService(buzzTestFetcher);
            var param = new FactoryParameterV1_0();
            
            // Build the service based on discovery information.
            return discovery.GetService(version, DiscoveryVersion.Version_1_0, param);
        }
	}
}
