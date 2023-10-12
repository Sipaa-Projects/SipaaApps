# SLang Programming Language Documentation

---

## Introduction

SLang is a simple, yet powerful, programming language designed for ease-of-use and rapid development. This documentation will guide you through his core features and functionalities.

This documentation has been wrote for the runtime v0.6.0

---

## Table of Contents

- [Data Types](#data-types)
- [SLang Files](#slang-files)
- [Common Language Design](#common-language-design)
- [Variables](#variables)
- [Operators](#operators)
- [Control Flow](#control-flow)
- [Functions](#functions)
- [Built-in Functions](#built-in-functions)
- [Comments](#comments)
- [External Libraries](#external-libraries)

---

## Data Types

SLang supports the following data types:

- 32-bit signed integer (System.Int32)
- Single-precision floating point (System.Single) (Values defined in this type must finish by 'F' or 'f')
- Double-precision floating point (System.Double) (Values defined in this type must finish by 'D' or 'd')
- Array of anything said here (System.Object[])
- UTF-16 text (System.String)
- Boolean
- Null

NOTE : The SLang runtime & external libraries can define variables of types who isn't said here.

---

## SLang Files
SLang support running files containing SLang code.

NOTE : SLang doesn't care about the file extension

To do that, drag & drop a file containing SLang code into the SLang REPL executable

---

## Common Language Design
SLang is designed to look like C/C#/Python.

In SLang, every line of code must finish with ;.

If you aren't sure about control flows, there must be only 1 ';' at the end.

Example:
```
if (<expression>) {
    <body>
} else {
    <body>
};
```

---

## Variables

Variables are declared and initialized as follows:

```
<name = <value>;
```

---

## Operators

Supported operators are:

- Addition (`+`)
- Subtraction (`-`)
- Multiplication (`*`)
- Division (`/`)

Example:

```c
int result = 10 + 20;
```

---

## Control Flow

SLang supports `if`, `else`, `while`, `for` & `foreach` statements.

- **If Statement**

```c
if (condition) {
    // Do something
};
```

- **If-Else Statement**

```c
if (condition) {
    // Do something
} else {
    // Do something else
};
```

- **While Loop**

```c
while (condition) {
    // Do something
};
```

- **For Loop**

```c
for (num = 0; num < 8; num = num + 1) {
    // Do something
}
```

- **Foreach Loop**

```c
foreach (num in numarray) {
    // Do something
}
```

---

## Functions

You can define and call functions as follows:

```c
def myFunction(arg1, arg2) {
    // Do something
};
```

Functions can also return values:

```c
def myFunction(arg1, arg2) {
    // Do something
    return arg1 + arg2;
};
printc(myFunction(1, 4));
```
Result : `5`

Calling a function:

```c
myFunction(10, "Hello");
```

---

## Built-in Functions

SLang offers built-in functions for common operations.

- `printrt()`: Displays the runtime context.
- `printc(arg1, arg2, ...)`: Concatenates and prints multiple arguments.
- `load(path)`: Load a CIL library (see [this](#external-libraries)).

Example:

```c
printc("Hello", " ", "world!");
```

Result:
```
Hello world!
```

---

## Comments

SLang supports single-line comments with `//`.

```c
// This is a comment
```

---

## External Libraries
SLang supports loading CIL libraries since runtime version 0.6.0.

### To make a library for S#, make a new C# class library with .NET 6 as framework, then add a class called "SLMetadata", and a dependency for "SLang.Runtime.dll" and put this code inside :

```cs
using SLang.Runtime;

namespace SLang.TestLibrary
{
    public class SLMetadata
    {
        public bool LibLoad(SLRuntime rt)
        {
            rt.Variables["newvar"] = "Library sucessfully loaded!";

            return true;
        }
    }
}
```

Then, you can change the code inside LibLoad, and interact with the runtime instance. (defined as 'rt' in this example)

NOTE: Libraries can also be built with any CIL-compilant languages, but we won't show the example for those languages.

### To load a library in SLang, use the 'load()' function, and it will do everything needed.
Example in the SLang REPL: `>>> load("SLang.TestLibrary.dll");`

This example will load the "SLang.TestLibrary.dll" file contained in the same directory as `SLang.exe`.

---

## Example Code

Here's an example code snippet that shows off various SLang features:

```c
// Declare variables
int a = 10;
float b = 20.5;
string name = "SLang";

// Arithmetic
int sum = a + 5;

// Control Flow
if (sum > 10) {
    printc("Sum is greater than 10. Actual sum: ", sum);
} else {
    printc("Sum is less than or equal to 10. Actual sum: ", sum);
};

// Function call
printrt();
```

That should give you a good starting point for understanding the core functionalities of SLang.
