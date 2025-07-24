# JSON Extractor API Reference

Technical reference documentation for all commands, options, interfaces, and models in the JSON Extractor console application.

## Table of Contents

1. [Overview](#overview)
2. [Commands](#commands)
3. [Command-Line Options](#command-line-options)
4. [Core Interfaces](#core-interfaces)
5. [Models and Data Types](#models-and-data-types)
6. [Configuration](#configuration)
7. [Error Handling](#error-handling)
8. [Examples](#examples)

## Overview

JSON Extractor provides a comprehensive API through its command-line interface, built on a modular architecture with dependency injection, structured logging, and extensible design patterns.

**Key Capabilities:**
- JSON parsing and validation with customisable options
- Advanced JSONPath querying with full expression support
- Multiple export formats (CSV, XML, JSON)
- Extensible command system
- Configuration management
- Comprehensive error handling and logging

## Commands

### parse
Parse and format JSON from string or file input.

#### Syntax
```bash
dotnet run --project src/JsonExtractor parse <json-string>
dotnet run --project src/JsonExtractor parse --file <file-path>
```

#### Parameters
- `<json-string>` - JSON string to parse (required if not using --file)
- `--file, -f <file-path>` - Path to JSON file to parse

#### Options
- `--pretty, -p` - Format output with indentation (default: true)
- `--validate-only` - Only validate JSON without outputting formatted result

#### Returns
- **Success**: Formatted JSON string
- **Error**: Validation error message with line/position information

#### Examples
```bash
# Parse JSON string
parse '{"name":"John","age":30}'

# Parse from file
parse --file data/sample.json

# Validate only
parse --validate-only '{"test": "value"}'
```

### query
Execute JSONPath queries on JSON data.

#### Syntax
```bash
dotnet run --project src/JsonExtractor query <json-string> <jsonpath-query>
dotnet run --project src/JsonExtractor query --file <file-path> <jsonpath-query>
```

#### Parameters
- `<json-string>` - JSON string to query (required if not using --file)
- `<jsonpath-query>` - JSONPath expression to execute
- `--file, -f <file-path>` - Path to JSON file to query

#### Options
- `--pretty, -p` - Format output with indentation
- `--count, -c` - Show count of results only

#### JSONPath Support
| Expression | Description | Example Data |
|------------|-------------|-------------|
| `$` | Root element | `{"name":"John"}` → entire object |
| `$.property` | Property access | `{"name":"John"}` → `"John"` |
| `$.store.book` | Nested property | `{"store":{"book":[...]}}` → book array |
| `$.array[0]` | Array index | `{"users":[{"name":"John"}]}` → first user |
| `$.array[*]` | Array wildcard | `{"users":[{"name":"John"},{"name":"Jane"}]}` → all users |
| `$..property` | Recursive descent | `{"store":{"book":{"price":10}}}` → all prices |
| `$.array[?(@.prop > value)]` | Filter expression | `{"books":[{"price":8},{"price":15}]}` → books under $10 |
| `$.array[1:3]` | Array slicing | `{"items":[1,2,3,4,5]}` → `[2,3]` (items 1-2) |
| `$.array[0,2,4]` | Multiple indices | `{"items":["a","b","c","d","e"]}` → `["a","c","e"]` |
| `$.array.length()` | Functions | `{"items":[1,2,3]}` → `3` |

#### Returns
- **Success**: Array of matching JSON elements with execution time
- **Error**: Query parsing error or execution failure

#### Examples
```bash
# Simple property query
query '{"name":"John","age":30}' '$.name'

# Array operations
query '{"users":[{"name":"John"},{"name":"Jane"}]}' '$.users[*].name'

# Filter expression
query '{"books":[{"price":8.99},{"price":12.99}]}' '$.books[?(@.price < 10)]'

# Recursive search
query --file data.json '$..price'
```

### format
Format and prettify JSON strings.

#### Syntax
```bash
dotnet run --project src/JsonExtractor format <json-string>
dotnet run --project src/JsonExtractor format --file <file-path>
```

#### Parameters
- `<json-string>` - JSON string to format (required if not using --file)
- `--file, -f <file-path>` - Path to JSON file to format

#### Options
- `--compact, -c` - Compact output (no indentation)
- `--indent <size>` - Set indentation size (default: 2)

#### Returns
- **Success**: Formatted JSON string
- **Error**: Parsing error message

#### Examples
```bash
# Format with default indentation
format '{"name":"John","age":30}'

# Compact formatting
format --compact '{"name":"John","age":30}'

# Custom indentation
format --indent 4 '{"name":"John","age":30}'
```

### export
Export JSON data to various formats.

#### Syntax
```bash
dotnet run --project src/JsonExtractor export <json-string> <format> [jsonpath]
dotnet run --project src/JsonExtractor export --file <file-path> <format> [jsonpath]
```

#### Parameters
- `<json-string>` - JSON string to export (required if not using --file)
- `<format>` - Export format: `csv`, `xml`, `json`
- `[jsonpath]` - Optional JSONPath query to filter data before export
- `--file, -f <file-path>` - Path to JSON file to export

#### Options
- `--output, -o <file>` - Write to file instead of console
- `--delimiter <char>` - CSV delimiter (default: ',')
- `--root-element <name>` - XML root element name (default: 'Results')
- `--item-element <name>` - XML item element name (default: 'Item')
- `--pretty, -p` - Pretty format output

#### Supported Formats

##### CSV Export
- Automatic header generation from object keys
- Proper value escaping and quoting
- Customisable delimiters
- Handles nested objects by flattening

##### XML Export
- Structured XML output with proper nesting
- Customisable root and item element names
- Attribute and element handling
- Proper XML encoding

##### JSON Export
- Re-export with JSONPath filtering
- Pretty printing options
- Array and object handling

#### Returns
- **Success**: Exported data in specified format
- **Error**: Export failure or format error

#### Examples
```bash
# Export to CSV
export '{"users":[{"name":"John","age":30}]}' csv '$.users[*]'

# Export to XML with custom elements
export --file data.json xml --root-element 'Data' --item-element 'Record'

# Export to file
export --output results.csv '{"data":[1,2,3]}' csv '$.data[*]'
```

### help
Display help information and usage examples.

#### Syntax
```bash
dotnet run --project src/JsonExtractor help [command]
```

#### Parameters
- `[command]` - Optional specific command to get help for

#### Examples
```bash
# General help
help

# Command-specific help
help query
```

## Command-Line Options

### Global Options
- `--help, -h` - Show help information
- `--version` - Show version information
- `--verbose, -v` - Enable verbose logging

### File Input Options
- `--file, -f <path>` - Read from file instead of command line argument
- Supports absolute and relative paths
- Automatic file existence validation

### Output Formatting Options
- `--pretty, -p` - Enable pretty printing with indentation
- `--compact, -c` - Disable indentation for compact output
- `--indent <size>` - Specify indentation size (spaces)

### Export Options
- `--output, -o <file>` - Write output to specified file
- `--delimiter <char>` - Set CSV delimiter character
- `--root-element <name>` - Set XML root element name
- `--item-element <name>` - Set XML item element name

## Core Interfaces

### ICommand
Base interface for all command implementations.

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task<CommandResult> ExecuteAsync(string[] args);
}
```

**Properties:**
- `Name` - Command identifier used in CLI
- `Description` - Human-readable command description

**Methods:**
- `ExecuteAsync(string[] args)` - Execute command with provided arguments

### IJsonExtractorService
Core JSON processing and manipulation service.

```csharp
public interface IJsonExtractorService
{
    Task<CommandResult> ParseJsonAsync(string json, ExtractorOptions? options = null);
    Task<CommandResult> ParseJsonFromFileAsync(string filePath, ExtractorOptions? options = null);
    Task<CommandResult> FormatJsonAsync(string json, ExtractorOptions? options = null);
    Task<CommandResult> ValidateJsonAsync(string json);
    CommandResult ConvertToObject<T>(string json, ExtractorOptions? options = null) where T : class;
    CommandResult SerializeObject<T>(T obj, ExtractorOptions? options = null) where T : class;
    Task<CommandResult> ParseJsonDocumentAsync(string json, ExtractorOptions? options = null);
}
```

**Key Methods:**
- `ParseJsonAsync` - Parse JSON string with options
- `ParseJsonFromFileAsync` - Parse JSON from file
- `FormatJsonAsync` - Format JSON with pretty printing
- `ValidateJsonAsync` - Validate JSON syntax
- `ParseJsonDocumentAsync` - Parse to JsonDocument for querying

### IJsonQueryService
Advanced JSONPath querying and search operations.

```csharp
public interface IJsonQueryService
{
    QueryResult ExecuteJsonPath(JsonElement rootElement, string jsonPath);
    QueryResult ExecuteMultipleQueries(JsonElement rootElement, IEnumerable<string> queries);
    QueryResult FindByKey(JsonElement rootElement, string key, bool caseSensitive = true);
    QueryResult FindByValue(JsonElement rootElement, object value, bool caseSensitive = true);
    QueryResult GetArrayElements(JsonElement rootElement, string arrayPath, int? skip = null, int? take = null);
    QueryResult FilterArray(JsonElement rootElement, string arrayPath, Func<JsonElement, bool> predicate);
}
```

**Key Methods:**
- `ExecuteJsonPath` - Execute JSONPath expression
- `ExecuteMultipleQueries` - Execute multiple queries and aggregate results
- `FindByKey` - Recursive key search
- `FindByValue` - Recursive value search
- `GetArrayElements` - Array pagination and slicing
- `FilterArray` - Custom predicate filtering

### IConfigurationService
Application configuration management.

```csharp
public interface IConfigurationService
{
    JsonExtractorConfiguration GetConfiguration();
    T GetSection<T>(string sectionName) where T : new();
    void UpdateConfiguration(JsonExtractorConfiguration configuration);
    void SaveConfiguration();
    void LoadConfiguration(string? filePath = null);
}
```

## Models and Data Types

### CommandResult
Standardised result wrapper for all operations.

```csharp
public record CommandResult
{
    public bool Success { get; private init; }
    public string? Message { get; private init; }
    public object? Data { get; private init; }
    public Exception? Exception { get; private init; }
}
```

**Static Methods:**
- `CreateSuccess(string? message, object? data)` - Create success result
- `CreateError(string message, Exception? exception)` - Create error result

### ExtractorOptions
Configuration options for JSON processing.

```csharp
public record ExtractorOptions
{
    public bool PrettyPrint { get; init; } = true;
    public bool CaseSensitive { get; init; } = true;
    public bool AllowComments { get; init; } = true;
    public bool AllowTrailingCommas { get; init; } = true;
    public int MaxDepth { get; init; } = 1000;
    public JsonNamingPolicy? PropertyNamingPolicy { get; init; }
}
```

**Properties:**
- `PrettyPrint` - Enable indented output formatting
- `CaseSensitive` - Property name case sensitivity
- `AllowComments` - Support for JSON comments
- `AllowTrailingCommas` - Allow trailing commas in JSON
- `MaxDepth` - Maximum parsing depth (prevents stack overflow)
- `PropertyNamingPolicy` - Custom property naming conventions

### QueryResult
Result container for JSONPath query operations.

```csharp
public class QueryResult
{
    public bool Success { get; init; }
    public List<JsonElement> Results { get; init; }
    public string? ErrorMessage { get; init; }
    public string Query { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    public int Count => Results?.Count ?? 0;
}
```

## Configuration

### JsonExtractorConfiguration
Root configuration object with nested configuration sections.

```csharp
public class JsonExtractorConfiguration
{
    public LoggingConfiguration Logging { get; init; } = new();
    public JsonProcessingConfiguration JsonProcessing { get; init; } = new();
    public QueryConfiguration Query { get; init; } = new();
    public ExportConfiguration Export { get; init; } = new();
    public PerformanceConfiguration Performance { get; init; } = new();
}
```

### Configuration Sections

#### LoggingConfiguration
```csharp
public class LoggingConfiguration
{
    public string LogLevel { get; init; } = "Information";
    public bool EnableConsoleLogging { get; init; } = true;
    public bool EnableFileLogging { get; init; } = false;
    public string LogFilePath { get; init; } = "logs/jsonextractor.log";
}
```

#### JsonProcessingConfiguration
```csharp
public class JsonProcessingConfiguration
{
    public int DefaultIndentSize { get; init; } = 2;
    public bool EnablePrettyPrinting { get; init; } = true;
    public int MaxJsonSize { get; init; } = 104857600; // 100MB
    public bool EnableValidation { get; init; } = true;
    public int MaxDepth { get; init; } = 64;
}
```

#### QueryConfiguration
```csharp
public class QueryConfiguration
{
    public int DefaultTimeout { get; init; } = 30;
    public int MaxResults { get; init; } = 10000;
    public bool EnableCaching { get; init; } = false;
    public int CacheDuration { get; init; } = 300;
}
```

### Environment Variables
- `JSONEXTRACTOR_LOGLEVEL` - Override logging level
- `JSONEXTRACTOR_OUTPUTFORMAT` - Set default output format

## Error Handling

### Common Error Types
1. **JSON Parsing Errors**
   - Invalid JSON syntax
   - Exceeding max depth
   - Unsupported features

2. **JSONPath Query Errors**
   - Invalid expression syntax
   - Property not found
   - Type mismatches

3. **File Operation Errors**
   - File not found
   - Access permissions
   - I/O errors

4. **Export Errors**
   - Unsupported format
   - Data conversion issues
   - Output file errors

### Error Response Format
All errors return a `CommandResult` with:
- `Success: false`
- `Message: string` - Human-readable error description
- `Exception: Exception?` - Original exception if available

## Examples

### Basic Operations
```bash
# Parse and validate JSON
parse '{"name":"John","age":30,"city":"New York"}'

# Query specific values
query '{"users":[{"name":"John","age":30},{"name":"Jane","age":25}]}' '$.users[*].name'

# Format JSON with custom indentation
format --indent 4 '{"compact":"json","needs":"formatting"}'
```

### Advanced JSONPath Queries
```bash
# Recursive property search
query --file large-data.json '$..price'

# Complex filtering
query '{"products":[{"name":"A","price":10},{"name":"B","price":20}]}' '$.products[?(@.price > 15)]'

# Array operations
query '{"items":[1,2,3,4,5,6,7,8,9,10]}' '$.items[2:8:2]'  # slice with step
```

### Export Operations
```bash
# Export to CSV with custom delimiter
export --delimiter=';' '{"data":[{"a":1,"b":2},{"a":3,"b":4}]}' csv '$.data[*]'

# Export to XML file
export --output results.xml --root-element 'Results' '{"items":[1,2,3]}' xml '$.items[*]'

# Chain operations: query then export
query '{"large_dataset":[...]}' '$.filtered_data[?(@.active == true)]' | \
export --output active-users.csv csv
```

### File Operations
```bash
# Process large JSON file
parse --file /path/to/large-file.json

# Query with file input and output
query --file input.json '$.specific.path[*]' > filtered-results.json

# Batch validation
for file in *.json; do
    parse --validate-only --file "$file" || echo "Invalid: $file"
done
```

---

This API reference provides comprehensive documentation for developers and users working with the JSON Extractor tool. For architectural information and extension patterns, see the [Architecture Guide](ARCHITECTURE.md).
