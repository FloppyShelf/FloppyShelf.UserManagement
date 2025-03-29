# FloppyShelf.UserManagement

C# library for generating unique usernames based on first and last names while ensuring consistency and availability. It is designed to be integrated into user management systems.

## Features

- Generates unique usernames based on first and last names.
- Handles replacement rules for special characters (e.g., "Ä" becomes "Ae").
- Removes invalid characters (non-alphanumeric) from the username.
- Supports length constraints for the generated username.
- Handles username collisions with numeric suffixes.
- Reverses the order of names for additional variety in username generation.
- Easily configurable replacement rules.

## Installation

Install via **NuGet**:

```bash
dotnet add package FloppyShelf.UserManagement
```

Or via the Package Manager:

```bash
Install-Package FloppyShelf.UserManagement
```

## Usage

### Basic Usage Example

The UniqueUsernameGenerator class allows you to generate unique usernames based on a user's first and last name. Below is an example of how to use the service in your project.

```csharp
using FloppyShelf.UserManagement.Services;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Example replacement rules (optional)
        var replacementRules = new Dictionary<string, string>
        {
            { "Sch", "S" }, // Replace "Sch" with "S"
            { "Ä", "Ae" }   // Replace "Ä" with "Ae"
        };

        // Initialize the username generator with replacement rules (optional)
        var usernameGenerator = new UniqueUsernameGenerator(replacementRules);

        // Set of existing usernames to avoid collision
        var existingUsernames = new HashSet<string>
        {
            "johnsmith",
            "janedoe"
        };

        try
        {
            // Generate a unique username
            string firstName = "John";
            string lastName = "Schmidt";
            string uniqueUsername = usernameGenerator.GenerateUniqueUsername(firstName, lastName, 6, 12, existingUsernames);

            Console.WriteLine($"Generated unique username: {uniqueUsername}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

### Constructor

```csharp
public UniqueUsernameGenerator(Dictionary<string, string> replacementRules = null);
```

- replacementRules (optional): A dictionary of string replacement rules to apply to the names before generating the username. If not provided, default rules are used.

### Methods

#### GenerateUniqueUsername

```csharp
public string GenerateUniqueUsername(string firstName, string lastName, int minLength, int maxLength, HashSet<string> existingUsernames);
```

- **firstName**: The first name of the user.
- **lastName**: The last name of the user.
- **minLength**: The minimum length of the generated username.
- **maxLength**: The maximum length of the generated username.
- **existingUsernames**: A set of existing usernames to avoid username collisions.

**Returns**: A unique username generated from the provided first and last names.

**Throws:**

- **ArgumentException** if the first or last name is empty or whitespace.
- **ArgumentOutOfRangeException** if the provided length constraints are invalid.
- **InvalidOperationException** if no unique username can be generated.

## Replacement Rules

By default, the service includes the following replacement rules:

- "Sch" → "S"
- "sch" → "s"
- "Ä" → "Ae"
- "Ö" → "Oe"
- "Ü" → "Ue"
- "ä" → "ae"
- "ö" → "oe"
- "ü" → "ue"
- "ß" → "ss"

You can customize these rules by providing a dictionary of replacement rules when initializing the UniqueUsernameGenerator class.