# JSON Extractor Console App

A comprehensive console application for parsing, querying, and manipulating JSON data with custom JSONPath support.

## Documentation

- **[User Guide](docs/USER_GUIDE.md)** - Comprehensive guide with examples and troubleshooting
- **[API Reference](docs/API_REFERENCE.md)** - Technical reference for all commands and options
- **[Architecture Guide](docs/ARCHITECTURE.md)** - Design patterns and extension points

## Features

- **JSON Parsing & Validation**: Parse JSON from strings or files with comprehensive error handling
- **Advanced JSONPath Querying**: Full-featured JSONPath implementation with recursive descent, filters, array
  operations, and functions
- **Multiple Export Formats**: Export to CSV, XML, and JSON with customizable options
- **Advanced Search**: Find by key/value with case sensitivity options
- **Array Operations**: Filter, paginate, slice, and manipulate JSON arrays
- **Console Interface**: Command-based architecture with comprehensive help system
- **Configuration Management**: JSON-based configuration with environment variable support
- **Extensible Design**: Clean architecture with dependency injection, logging, and service container

## Quick Start

### Setup

```bash
# Clone and build
git clone <repository>
cd JsonExtractor
dotnet build

# Run tests
dotnet test

# Show help
dotnet run --project src/JsonExtractor help

## Basic Usage

```bash
# Parse and format JSON
dotnet run --project src/JsonExtractor parse '{"name":"John","age":30}'

# Parse from file
dotnet run --project src/JsonExtractor parse --file data.json

# Query with JSONPath
dotnet run --project src/JsonExtractor query '{"users":[{"name":"John"},{"name":"Jane"}]}' '$.users[*].name'

# Format JSON
dotnet run --project src/JsonExtractor format '{"name":"John","age":30}'
```

## Advanced Query Examples

```bash
# Query nested objects
dotnet run --project src/JsonExtractor query '{"store":{"book":[{"title":"Book1","price":10.99},{"title":"Book2","price":8.99}]}}' '$.store.book[*].title'

# Query with array indexing
dotnet run --project src/JsonExtractor query '{"users":[{"name":"John","age":30},{"name":"Jane","age":25}]}' '$.users[0].name'

# Query all elements in array
dotnet run --project src/JsonExtractor query '{"items":[1,2,3,4,5]}' '$.items[*]'
```

## Advanced JSONPath Support

Our enhanced JSONPath implementation now supports:

- `$` - Root element
- `$.property` - Property access
- `$.store.book` - Nested property access
- `$.store.book[0]` - Array indexing
- `$.store.book[*].title` - Array wildcard with property access
- `$.items[*]` - Array wildcard (all elements)
- `$..property` - Recursive descent for `property`
- `$..*` - Recursive descent wildcard
- `$.store.book[?(@.price < 10)]` - Filter expressions
- `$.store.book[1:3]` - Array slicing
- `$.store.book[0,2]` - Multiple indices
- `$.store.book.length()` - Functions

### Advanced Query Examples

```bash
# Recursive descent to find all prices
JsonExtractor query '<JSON_DATA>' '$..price'

# Filter books cheaper than 10
JsonExtractor query '<JSON_DATA>' '$.store.book[?(@.price < 10)]'

# Access multiple indices in an array
JsonExtractor query '<JSON_DATA>' '$.store.book[0,2]'

# Array slicing to get a subset of books
JsonExtractor query '<JSON_DATA>' '$.store.book[1:3]'
```

## Architecture

- **Services**: Core JSON processing and querying logic
    - `JsonExtractorService`: Main JSON parsing, formatting, and validation
    - `JsonQueryService`: Custom JSONPath query engine
- **Commands**: Command pattern for extensible CLI operations
    - `ParseCommand`: Parse and format JSON
    - `QueryCommand`: Execute JSONPath queries
    - `FormatCommand`: Format and prettify JSON
    - `HelpCommand`: Display help and usage information
- **Models**: Data transfer objects and configuration
    - `CommandResult`: Standardized response wrapper
    - `ExtractorOptions`: Configuration options
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Structured logging with Microsoft.Extensions.Logging
- **Testing**: Comprehensive test suite with xUnit, Moq, and FluentAssertions

## Testing

- **Unit Tests**: Comprehensive coverage with xUnit, Moq, and FluentAssertions
- **Integration Tests**: Real-world scenarios with various JSON structures
- **Command Tests**: All CLI commands thoroughly tested
- **Service Tests**: Core functionality and edge cases covered
- **Advanced JSONPath Tests**: Comprehensive test coverage for all JSONPath features
- **Test Results**: 111 tests passing, 0 failures

## Dependencies

### Runtime

- .NET 9.0
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Logging.Console
- Microsoft.Extensions.Configuration
- System.Text.Json (built-in)

### Testing

- xUnit (Testing framework)
- Moq (Test mocking)
- FluentAssertions (Test assertions)

## Project Status

✅ **Completed Features:**

- JSON parsing and validation
- JSON formatting and prettification
- Advanced JSONPath query engine with recursive descent, filters, array operations, and functions
- Multiple export formats (CSV, XML, JSON) with customizable options
- Configuration management with JSON-based settings and environment variables
- Command-line interface with comprehensive help system
- Service container and dependency injection architecture
- Comprehensive error handling and logging
- Full test suite (111 tests passing, 0 failures)
- Clean architecture with SOLID principles
- Structured logging with Microsoft.Extensions.Logging

🔄 **Potential Enhancements:**

- File I/O operations with streaming support
- Performance optimizations for large JSON datasets
- Additional export formats (YAML, TOML)
- Query result caching
- Plugin architecture for extensibility
- Interactive mode with REPL
- Batch processing capabilities

### Setup Instructions

```bash
# Create solution structure
mkdir JsonExtractor
cd JsonExtractor

# Create solution
dotnet new sln

# Create projects
mkdir -p src/JsonExtractor tests/JsonExtractor.Tests
cd src/JsonExtractor && dotnet new console && cd ../..
cd tests/JsonExtractor.Tests && dotnet new xunit && cd ../..

# Add to solution
dotnet sln add src/JsonExtractor/JsonExtractor.csproj
dotnet sln add tests/JsonExtractor.Tests/JsonExtractor.Tests.csproj

# Add project reference
dotnet add tests/JsonExtractor.Tests/JsonExtractor.Tests.csproj reference src/JsonExtractor/JsonExtractor.csproj

# Install packages (main project)
cd src/JsonExtractor
dotnet add package JsonCons.JsonPath --version 1.1.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add package Microsoft.Extensions.Logging --version 8.0.0
dotnet add package Microsoft.Extensions.Logging.Console --version 8.0.0
cd ../..

# Install packages (test project)
cd tests/JsonExtractor.Tests
dotnet add package Moq --version 4.20.69
dotnet add package FluentAssertions --version 6.12.0
cd ../..

# Build and test
dotnet build
dotnet test
```
