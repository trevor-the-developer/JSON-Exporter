namespace JsonExtractor.Tests.TestData;

public static class SampleData
{
    public const string SimpleJson = """
                                     {
                                         "name": "John Doe",
                                         "age": 30,
                                         "isActive": true,
                                         "address": {
                                             "street": "123 Main St",
                                             "city": "New York",
                                             "zipCode": "10001"
                                         },
                                         "hobbies": ["reading", "swimming", "coding"],
                                         "metadata": null
                                     }
                                     """;

    public const string ComplexJson = """
                                      {
                                          "store": {
                                              "book": [
                                                  {
                                                      "category": "reference",
                                                      "author": "Nigel Rees",
                                                      "title": "Sayings of the Century",
                                                      "price": 8.95
                                                  },
                                                  {
                                                      "category": "fiction",
                                                      "author": "Evelyn Waugh",
                                                      "title": "Sword of Honour",
                                                      "price": 12.99
                                                  },
                                                  {
                                                      "category": "fiction",
                                                      "author": "Herman Melville",
                                                      "title": "Moby Dick",
                                                      "isbn": "0-553-21311-3",
                                                      "price": 8.99
                                                  }
                                              ],
                                              "bicycle": {
                                                  "color": "red",
                                                  "price": 19.95
                                              }
                                          },
                                          "expensive": 10
                                      }
                                      """;

    public const string InvalidJson = """
                                      {
                                          "name": "John",
                                          "age": 30,
                                          "invalid":
                                      }
                                      """;

    public const string ArrayJson = """
                                    [
                                        {"id": 1, "name": "Alice", "email": "alice@example.com"},
                                        {"id": 2, "name": "Bob", "email": "bob@example.com"},
                                        {"id": 3, "name": "Charlie", "email": "charlie@example.com"}
                                    ]
                                    """;

    public const string NestedArrayJson = """
                                          {
                                              "users": [
                                                  {
                                                      "id": 1,
                                                      "name": "Alice",
                                                      "orders": [
                                                          {"orderId": "A001", "amount": 50.00},
                                                          {"orderId": "A002", "amount": 75.50}
                                                      ]
                                                  },
                                                  {
                                                      "id": 2,
                                                      "name": "Bob",
                                                      "orders": [
                                                          {"orderId": "B001", "amount": 100.00}
                                                      ]
                                                  }
                                              ]
                                          }
                                          """;

    public const string FacebookMessagesJson = """
                                               {
                                                   "messages": [
                                                       {
                                                           "sender_name": "John Smith",
                                                           "timestamp_ms": 1609459200000,
                                                           "content": "Hi, I need carpet cleaning for my office. Can you provide a quote?",
                                                           "type": "Generic"
                                                       },
                                                       {
                                                           "sender_name": "CleanPro Services",
                                                           "timestamp_ms": 1609462800000,
                                                           "content": "Sure! For office carpet cleaning, we charge $2 per square foot. What's the size of your office?",
                                                           "type": "Generic"
                                                       },
                                                       {
                                                           "sender_name": "John Smith",
                                                           "timestamp_ms": 1609466400000,
                                                           "content": "The office is about 500 square feet. When can you do it?",
                                                           "type": "Generic"
                                                       },
                                                       {
                                                           "sender_name": "CleanPro Services",
                                                           "timestamp_ms": 1609470000000,
                                                           "content": "That would be $1000 total. We can schedule for next Monday at 9 AM. Does that work?",
                                                           "type": "Generic"
                                                       }
                                                   ]
                                               }
                                               """;
}