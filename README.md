# doxygeneditor
A visual tool for editing & validating doxygen documentations based on C# and Scintilla.NET.

Its main purpose is helping you in writing & validating doxygen-based documentations for C/C++ projects. 

## Features:
- Tabbed file editor with standard mechanics (New, Open, Save, Reload, Undo/Redo, etc.) 
- Treeview for fast-navigating inside doxygen pages & sections 
- Symbol search inside the code-editor for C/C++ and doxygen symbols 

- Basic lexical analysis of C/C++
- Full lexical analysis of Doxygen 

- Syntax Highlighting of C/C++ 
- Syntax Highlighting of Doxygen (Including @code syntax highlighting for .c code) 
- Syntax Highlighting of HTML (Tags and attributes) 

- Basic parsing of several C/C++ (typedefs, enums, structs, functions, defines, etc.) 
- Full parsing of all Doxygen elements (commands, paragraphs, sections, pages, references, etc.) 

- Symbol reference validation for C/C++ (Partially) 
- Symbol reference validation for doxygen elements (ref, subpage, etc.) 

- Special validations for my needs

## Limitations:
- The c++ lexer & parser does not implement the full grammar of C99 or C++, but it gets close
- Its slow like hell (Will transform the lexer into a FSM in the future)

## Screenshots:

![Doxygen Editor v0.8.1 Screenshot](https://www.libfpl.org/doxygeneditor/doxygeneditor_v0_8_1_shot2.jpg "Doxygen Editor v0.8.1 Screenshot")