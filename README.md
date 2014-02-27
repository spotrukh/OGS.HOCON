OGS.HOCON
=========
OGS.HOCON dotNet library based on the same HOCON notation described in [typesafe HCOCN](https://github.com/typesafehub/config/blob/master/HOCON.md).

Current implementation is not full support HOCON specification, follow link [LIMITATION.md](LIMITATION.md) to check current limitations.

**Notation example:**

    # Hocon notation example
    
    include "another.conf"
    
    top.level.property : 3
    
    rootNode {
        subNode {
            # strings
            string_property : "test string value"
            string_property : some-string
            
            #int
            int_property : 333
            
            # decimal
            decimal_property1 : 333.22
            decimal_property2 : 1E5
            
            # bool
            boolena_property1 : true
            boolena_property2 : false
            boolena_property3 : off
            boolena_property4 : off
            boolena_property5 : enabled
            boolena_property6 : disabled
        }
    }
    
    derived : $(rootNode) {
        # extend with new property
        extra_property : 1

        subNode {
            # override properties
            string_property : "new value"
            int_property : ${top.level.property}
        }
    }
    
### How to use
**Using HOCON notation:**    
```csharp
var reader = new DictionaryReader(null);
reader.ReadFromString(
@"ssh {
    connection {
        host : 127.0.0.1
        port : 22
    }
    status : on
}");

var host = reader.Source["ssh.connection.host"];
var port = reader.Source["ssh.connection.port"];
var status = reader.Source["ssh.status"];
```
    
**Using configuration library:**    
```csharp
var config = new Configuration(null);
config.ReadFromString(
@"ssh {
    connection {
        host : 127.0.0.1
        port : 22
    }
    status : on
    log : ["warn", error, info]
}");

var host = config.GetString("ssh.connection.host");
var port = config.GetInt("ssh.connection.port");
var status = config.GetBool("ssh.status");
var log = config.GetStringList("ssh.log");
```
    
    
