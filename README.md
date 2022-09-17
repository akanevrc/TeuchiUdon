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
$ cd ./Parser/Grammars
$ antlr4 -package akanevrc.TeuchiUdon TeuchiUdonLexer.g4
$ antlr4 -package akanevrc.TeuchiUdon TeuchiUdonParser.g4
```
