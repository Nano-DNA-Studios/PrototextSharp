# PrototextSharp

PrototextSharp is a .NET library for parsing and writing a focused subset of Protocol Buffers text-format messages (Prototext). It maps messages to a lightweight `ProtoNode` tree, without requiring `.proto` schemas or generated message classes.

The library does not implement the full Protobuf Text Format. Check the supported syntax below before relying on features beyond it.

## Features

- Parse scalar fields and nested message blocks.
- Preserve repeated field values and their source order.
- Read quoted strings, lowercase Boolean literals, integer and decimal numbers, and bare identifiers.
- Skip `#` and `//` line comments.
- Serialize `ProtoNode` trees with configurable indentation.
- Format numeric output using invariant culture.

## Requirements

- .NET Standard 2.1 compatible runtime to use the library.
- .NET SDK to build the repository.
- The test project targets .NET 9.

## Add a project reference

No published NuGet package ID is documented in this repository. To reference the project from another local project, run this command from the consumer project directory:

```powershell
dotnet add reference "<path-to-repository>/PrototextSharp/PrototextSharp.csproj"
```

Replace `<path-to-repository>` with the path to your PrototextSharp checkout.

## Parse Prototext

```csharp
using PrototextSharp;

const string input = @"name: ""North Station""
enabled: true
platform_count: 4
location {
  city: ""Toronto""
  coordinates {
    latitude: 43.6532
    longitude: -79.3832
  }
}";

PrototextParser parser = new PrototextParser();
ProtoNode message = parser.Parse(input);

string name = (string)message["name"]!;
bool enabled = (bool)message["enabled"]!;
long platformCount = (long)message["platform_count"]!;

ProtoNode location = (ProtoNode)message["location"]!;
string city = (string)location["city"]!;
```

`ProtoNode` field names are case-sensitive. Its indexer returns the value directly when a field occurs once, or a `List<object?>` when it occurs more than once. It returns `null` for a missing field. A single stored `null` also returns `null`; use `Fields` to distinguish that case from a missing field.

```csharp
using System.Collections.Generic;
using PrototextSharp;

PrototextParser parser = new PrototextParser();
ProtoNode message = parser.Parse("tag: ""north"" tag: ""express""");

List<object?> tags = (List<object?>)message["tag"]!;
```

### Supported input

- Nested messages use braces: `child { value: 1 }`.
- Quoted strings support escapes for quotes, backslashes, newline, carriage return, and tab.
- Lowercase `true` and `false` are parsed as Booleans.
- Integer values are parsed as `long`; numbers containing a decimal point are parsed as `double`.
- Bare identifiers are parsed as strings. Identifiers start with a letter or underscore and may contain letters, digits, underscores, or hyphens.
- `#` and `//` start line comments outside quoted strings.

The parser does not support every Protobuf Text Format feature. For example, exponent notation such as `1e3`, plus-prefixed numbers such as `+1`, and leading-decimal numbers such as `.5` are not supported.

## Write Prototext

Create a node, add scalar or child-node values, and serialize it:

```csharp
using PrototextSharp;

ProtoNode station = new ProtoNode();
station.Add("name", "North Station");
station.Add("enabled", true);
station.Add("platform_count", 4L);

ProtoNode location = new ProtoNode();
location.Add("city", "Toronto");
station.Add("location", location);

PrototextWriter writer = new PrototextWriter(indentText: "  ");
string output = writer.Serialize(station);

System.Console.WriteLine(output);
```

The writer supports `null`, strings, Booleans, integral and floating-point numeric types, `decimal`, enums, and nested `ProtoNode` values. The default indentation is one space per nesting level; pass another string to `PrototextWriter` to customize it.

The writer emits `null` and enum values as bare text. Since the parser treats bare identifiers as strings (except lowercase `true` and `false`), serializing and parsing these values does not preserve their original CLR types. Round-trip behavior is limited to values representable by both sides of the supported subset.

## Build and run tests

From the repository root:

```powershell
dotnet test PrototextSharp.slnx -c Release
```

The test suite covers node field behavior, parsing, syntax errors, strings and comments, serialization, and parsing a railway-network fixture.

# License

This project is available under the NanoDNA Community License 1.0.

Individuals may use and modify the project for personal, hobby, educational, or research purposes outside the work or operations of a business or institution. Qualifying small businesses may use and modify it for their own project if they have no more than 20 people and no more than USD $10,000 in project revenue during the preceding rolling 12-month period.

Businesses that exceed either threshold and institutional users must obtain separate written authorization from NanoDNA Studios. Redistribution or providing the software, modified versions, or derivative works to another person or entity also requires separate written authorization for each recipient.

For licensing requests, contact: Mr.DNAlexandre@gmail.com

This is a source-available license. It is not an OSI-approved open-source license.