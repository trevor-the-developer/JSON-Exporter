# JsonExtractor User Guide

## Table of Contents

1. [Getting Started](#getting-started)
2. [Installation](#installation)
3. [Basic Usage](#basic-usage)
4. [Advanced JSONPath Queries](#advanced-jsonpath-queries)
5. [Export Features](#export-features)
6. [Command Reference](#command-reference)
7. [Configuration](#configuration)
8. [Performance Tips](#performance-tips)
9. [Troubleshooting](#troubleshooting)
10. [FAQ](#faq)

## Getting Started

JsonExtractor is a powerful console application for parsing, querying, and manipulating JSON data. It provides an advanced JSONPath implementation with support for recursive descent, filters, array operations, and more.

### Prerequisites

- .NET 9.0 or later
- Console/Terminal access

## Installation

### Build from Source

```bash
git clone <repository>
cd JsonExtractor
dotnet build
```

### Running the Application

```bash
# From the project root
dotnet run --project src/JsonExtractor -- [command] [options]

# Or after building
./src/JsonExtractor/bin/Debug/net9.0/JsonExtractor [command] [options]
```

## Basic Usage

### 1. Parse JSON

Parse and validate JSON from a string or file:

```bash
# Parse from string
JsonExtractor parse '{"name":"John","age":30}'

# Parse from file
JsonExtractor parse --file data.json

# Parse with pretty formatting
JsonExtractor parse --pretty '{"name":"John","age":30}'
```

### 2. Query JSON

Execute JSONPath queries on JSON data:

```bash
# Simple property access
JsonExtractor query '{"name":"John","age":30}' '$.name'

# Array queries
JsonExtractor query '{"users":[{"name":"John"},{"name":"Jane"}]}' '$.users[*].name'
```

### 3. Format JSON

Format and prettify JSON:

```bash
# Format compact JSON
JsonExtractor format '{"name":"John","age":30}'

# Format from file
JsonExtractor format --file data.json
```

### 4. Export Data

Export JSON data to different formats:

```bash
# Export to CSV
JsonExtractor export '{"users":[{"name":"John","age":30}]}' csv '$.users[*]'

# Export to XML
JsonExtractor export '{"data":{"value":123}}' xml

# Export to JSON with query
JsonExtractor export '{"items":[1,2,3,4,5]}' json '$.items[2:]'
```

## Advanced JSONPath Queries

### Recursive Descent

Find all occurrences of a property at any level:

```bash
# Find all "price" properties
JsonExtractor query '{"store":{"book":[{"title":"Book1","price":10.99},{"title":"Book2","price":8.99}],"bicycle":{"price":19.95}}}' '$..price'

# Find all properties recursively
JsonExtractor query '{"a":{"b":{"c":1}}}' '$..*'
```

### Filter Expressions

Filter arrays based on conditions:

```bash
# Numeric comparisons
JsonExtractor query '{"books":[{"price":8.95},{"price":12.99},{"price":22.99}]}' '$.books[?(@.price < 10)]'
JsonExtractor query '{"books":[{"price":8.95},{"price":12.99},{"price":22.99}]}' '$.books[?(@.price > 15)]'

# String comparisons
JsonExtractor query '{"books":[{"category":"fiction"},{"category":"reference"}]}' '$.books[?(@.category == "fiction")]'
JsonExtractor query '{"books":[{"category":"fiction"},{"category":"reference"}]}' '$.books[?(@.category != "fiction")]'
```

### Array Operations

#### Array Slicing

Extract specific ranges from arrays:

```bash
# Get first 3 elements
JsonExtractor query '{"items":[1,2,3,4,5,6]}' '$.items[:3]'

# Get elements from index 2 to 4
JsonExtractor query '{"items":[1,2,3,4,5,6]}' '$.items[2:4]'

# Get elements from index 3 to end
JsonExtractor query '{"items":[1,2,3,4,5,6]}' '$.items[3:]'
```

#### Multiple Indices

Access multiple specific array elements:

```bash
# Get elements at indices 0, 2, and 4
JsonExtractor query '{"items":[1,2,3,4,5,6]}' '$.items[0,2,4]'
```

#### Array Wildcards

```bash
# Get all elements in array
JsonExtractor query '{"users":[{"name":"John"},{"name":"Jane"}]}' '$.users[*]'

# Get all properties of an object
JsonExtractor query '{"store":{"book":[],"bicycle":{}}}' '$.store.*'
```

### Functions

Execute functions on JSON elements:

```bash
# Get length of array
JsonExtractor query '{"items":[1,2,3,4,5]}' '$.items.length()'

# Get keys of object
JsonExtractor query '{"name":"John","age":30}' '$.keys()'

# Get values of object
JsonExtractor query '{"name":"John","age":30}' '$.values()'
```

## Export Features

### CSV Export

Convert JSON arrays to CSV format:

```bash
# Basic CSV export
JsonExtractor export '{"users":[{"name":"John","age":30},{"name":"Jane","age":25}]}' csv '$.users[*]'

# CSV with custom delimiter
JsonExtractor export '{"data":[{"a":1,"b":2}]}' csv '$.data[*]' --delimiter=';'
```

### XML Export

Convert JSON to XML format:

```bash
# Basic XML export
JsonExtractor export '{"person":{"name":"John","age":30}}' xml

# XML with custom root element
JsonExtractor export '{"data":{"value":123}}' xml --root-element='CustomRoot'
```

### JSON Export

Export with JSONPath filtering:

```bash
# Export filtered results
JsonExtractor export '{"items":[1,2,3,4,5]}' json '$.items[?(@.* > 3)]'

# Export with pretty formatting
JsonExtractor export '{"data":{"nested":{"value":123}}}' json --pretty
```

## Command Reference

### Global Options

- `--help, -h`: Show help information
- `--version`: Show version information
- `--verbose, -v`: Enable verbose logging

### Parse Command

```bash
JsonExtractor parse [options] <json-string>
```

**Options:**
- `--file, -f <file>`: Parse from file instead of string
- `--pretty, -p`: Format output with indentation
- `--validate-only`: Only validate, don't output

### Query Command

```bash
JsonExtractor query [options] <json-string> <jsonpath>
```

**Options:**
- `--file, -f <file>`: Parse from file instead of string
- `--pretty, -p`: Format output with indentation
- `--count, -c`: Show count of results only

### Format Command

```bash
JsonExtractor format [options] <json-string>
```

**Options:**
- `--file, -f <file>`: Parse from file instead of string
- `--compact, -c`: Compact output (no indentation)
- `--indent <size>`: Set indentation size (default: 2)

### Export Command

```bash
JsonExtractor export [options] <json-string> <format> [jsonpath]
```

**Options:**
- `--file, -f <file>`: Parse from file instead of string
- `--output, -o <file>`: Write to file instead of console
- `--delimiter <char>`: CSV delimiter (default: ',')
- `--root-element <name>`: XML root element name
- `--pretty, -p`: Pretty format output

### Help Command

```bash
JsonExtractor help [command]
```

## Configuration

### Configuration File

Create an `appsettings.json` file in the application directory:

```json
{
  "JsonExtractor": {
    "DefaultOutputFormat": "pretty",
    "MaxQueryDepth": 100,
    "EnableLogging": true,
    "LogLevel": "Information"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "JsonExtractor": "Debug"
    }
  }
}
```

### Environment Variables

- `JSONEXTRACTOR_LOGLEVEL`: Set logging level (Debug, Information, Warning, Error)
- `JSONEXTRACTOR_OUTPUTFORMAT`: Set default output format (pretty, compact)

## Performance Tips

### 1. Use Specific Queries

Instead of using recursive descent for everything, use specific paths when possible:

```bash
# Slower
JsonExtractor query '<data>' '$..price'

# Faster (if you know the structure)
JsonExtractor query '<data>' '$.store.book[*].price'
```

### 2. Limit Result Sets

Use filters and slicing to limit results:

```bash
# Get only first 10 results
JsonExtractor query '<data>' '$.items[:10]'

# Use filters to reduce data
JsonExtractor query '<data>' '$.items[?(@.price < 100)]'
```

### 3. File Operations

For large JSON files, use file input instead of passing data as arguments:

```bash
# Better for large files
JsonExtractor query --file large-data.json '$.items[*]'
```

### 4. Batch Operations

Process multiple queries efficiently:

```bash
# Multiple queries in one operation
JsonExtractor query '<data>' '$.store.book[*].title' '$.store.book[*].price'
```

## Troubleshooting

### Common Issues

#### 1. Invalid JSON

**Error:** `Failed to parse JSON: Invalid JSON format`

**Solution:** Validate your JSON using a JSON validator or the parse command:

```bash
JsonExtractor parse --validate-only '{"invalid": json}'
```

#### 2. Empty Results

**Error:** Query returns no results

**Common causes:**
- Incorrect JSONPath syntax
- Property doesn't exist
- Array index out of bounds

**Solutions:**
```bash
# Check if property exists
JsonExtractor query '<data>' '$'

# Use recursive descent to find properties
JsonExtractor query '<data>' '$..propertyName'

# Check array length
JsonExtractor query '<data>' '$.array.length()'
```

#### 3. Performance Issues

**Symptoms:** Slow query execution, high memory usage

**Solutions:**
- Use more specific queries
- Implement pagination with slicing
- Process data in smaller chunks

#### 4. Filter Syntax Errors

**Error:** `Filter expression failed`

**Common issues:**
- Missing quotes around string values
- Incorrect operator usage
- Invalid property references

**Correct syntax:**
```bash
# String comparison (note the quotes)
JsonExtractor query '<data>' '$.items[?(@.name == "John")]'

# Numeric comparison (no quotes)
JsonExtractor query '<data>' '$.items[?(@.price > 10)]'
```

## FAQ

### Q1: What JSONPath features are supported?

A: JsonExtractor supports:
- Basic property access (`$.property`)
- Array indexing (`$.array[0]`)
- Array slicing (`$.array[1:3]`)
- Wildcards (`$.array[*]`, `$.*`)
- Recursive descent (`$..property`)
- Filter expressions (`$.array[?(@.property > value)]`)
- Functions (`$.array.length()`, `$.keys()`, `$.values()`)
- Multiple indices (`$.array[0,2,4]`)

### Q2: How do I handle special characters in property names?

A: Use bracket notation for properties with special characters:

```bash
JsonExtractor query '{"my-property":"value"}' '$["my-property"]'
```

### Q3: Can I use JsonExtractor in scripts?

A: Yes, JsonExtractor is designed for scripting:

```bash
#!/bin/bash
result=$(JsonExtractor query "$json_data" "$.items[*].name")
echo "Names: $result"
```

### Q4: How do I export to a file?

A: Use the `--output` option:

```bash
JsonExtractor export --output results.csv '<data>' csv '$.items[*]'
```

### Q5: Can I process multiple files?

A: Currently, process one file at a time. For multiple files, use a script:

```bash
for file in *.json; do
    JsonExtractor query --file "$file" '$.items[*]' > "${file%.json}.results"
done
```

### Q6: How do I debug query issues?

A: Use verbose logging and step-by-step querying:

```bash
# Enable verbose logging
JsonExtractor --verbose query '<data>' '$.complex.query'

# Test parts of the query
JsonExtractor query '<data>' '$'
JsonExtractor query '<data>' '$.store'
JsonExtractor query '<data>' '$.store.book'
```

### Q7: What's the maximum JSON size supported?

A: Limited by available memory. For very large files:
- Use streaming approaches
- Process data in chunks
- Use specific queries to reduce memory usage

### Q8: How do I contribute or report issues?

A: Please refer to the project's contribution guidelines and issue tracker in the repository.

---

For more information, examples, and updates, please refer to the [README.md](../README.md) file and the project repository.
