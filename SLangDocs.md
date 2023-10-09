# SLang Programming Language Documentation

---

## Introduction

SLang is a simple, yet powerful, programming language designed for ease-of-use and rapid development. This documentation will guide you through his core features and functionalities.

This documentation has been wrote for the runtime v0.5

---

## Table of Contents

- [Data Types](#data-types)
- [Common Language Design](#common-language-design)
- [Variables](#variables)
- [Operators](#operators)
- [Control Flow](#control-flow)
- [Functions](#functions)
- [Built-in Functions](#built-in-functions)
- [Comments](#comments)

---

## Data Types

SLang supports the following data types:

- 32-bit signed integer (System.Int32)
- Single-precision floating point (System.Single) (Values defined in this type must finish by 'F' or 'f')
- Double-precision floating point (System.Double) (Values defined in this type must finish by 'D' or 'd')
- UTF-16 text (System.String)
- Boolean

---

## Common Language Design
SLang is designed to look like C/C#/Python.

In SLang, every line of code must finish with ;.

If you aren't sure about control flows, there must be only 1 ';' at the end.

Example:
\```slang
if (<expression>) {
    <body>
} else {
    <body>
};
\```

---

## Variables

Variables are declared and initialized as follows:

\```slang
<name> = <value>;
\```

---

## Operators

Supported operators are:

- Addition (`+`)
- Subtraction (`-`)
- Multiplication (`*`)
- Division (`/`)

Example:

\```slang
int result = 10 + 20;
\```

---

## Control Flow

SLang supports `if`, `else` and `while` statements.

- **If Statement**

\```slang
if (condition) {
    // Do something
};
\```

- **If-Else Statement**

\```slang
if (condition) {
    // Do something
} else {
    // Do something else
};
\```

- **While Loop**

\```slang
while (condition) {
    // Do something
};
\```

---

## Functions

You can define and call functions as follows:

\```slang
def myFunction(arg1, arg2) {
    // Do something
}
\```

Calling a function:

\```slang
myFunction(10, "Hello");
\```

---

## Built-in Functions

SLang offers built-in functions for common operations.

- `printrt()`: Displays the runtime context.
- `printc(arg1, arg2, ...)`: Concatenates and prints multiple arguments.

Example:

\```slang
printc("Hello", " ", "world!");
\```

Result:
\```
Hello world!
\```

---

#### Comments

SLang supports single-line comments with `//`.

\```slang
// This is a comment
\```

---

## Example Code

Here's an example code snippet that shows off various SLang features:

\```slang
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
\```

That should give you a good starting point for understanding the core functionalities of SLang.
