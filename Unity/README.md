# TeuchiUdon

[![GitHub tag version](https://img.shields.io/github/v/tag/akanevrc/TeuchiUdon)]()
[![GitHub license](https://img.shields.io/github/license/akanevrc/TeuchiUdon)](LICENSE)

TeuchiUdon is a programming language for [VRChat](https://hello.vrchat.com/) Udon.
TeuchiUdon compiler generates Udon-assembly.

This project is currently in progress.

## ANTLR

TeuchiUdon uses [ANTLR](https://www.antlr.org/) parser generator and its runtime for C#.
Thanks a lot!

ANTLR Compilation Usage:

```console
# on project folder
$ antlr4 -package akanevrc.TeuchiUdon.Editor.Compiler ./Assets/akanevrc/TeuchiUdon/Editor/Compiler/TeuchiUdonLexer.g4
$ antlr4 -package akanevrc.TeuchiUdon.Editor.Compiler ./Assets/akanevrc/TeuchiUdon/Editor/Compiler/TeuchiUdonParser.g4
```
