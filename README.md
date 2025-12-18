# Solar Multilingual Assembler

*i.e. Extensible Data-Oriented Framework for Assembly Code Representation and Manipulation*

*This repository was created for the University of Ottawa CSI 4900 class of Fall 2025*\
*It contains a subset of features suitable to the 4 months of the Honours Project, and may be expanded upon at a later date, in a separate repository*

**NOTE:** This README provides a high-level overview. For full technical detail, see the [project wiki](../../wiki)

---

The **Solar Multilingual Assembler (SMLA)** project is an experimental assembler framework written in C# with a focus on implementing a data model that facilitates expressivity, flexibility and extensibility.

It takes inspiration from the *Intermediate Representation* (IR) code of contemporary compilers, representing assembly code in a language-agnostic way that can be manipulated by a shared corpus of tools. This allows directives, opcodes, input/output formats and more utilities to be swapped in and out and combined without having to modify the front-end of the compiler.

By doing so, the time cost of creating new tooling while working on custom architectures is severely reduced, and the expressivity of a more complex system is kept between multiple different projects.

Fundamentally, an *SMLA* session reads input from one or more source files into a data model, manipulates the data model, and returns the model to be encoded into an output format.
Sessions can contain multiple program contexts, allowing for the merging of several source files, provided that the formats are all compatible.

There is built-in support for **Expressions** which can be reevaluated as needed, and many components of the model can be extended to add new behaviour.

The name **Multilingual** reflects its ability to work with multiple different assembly languages as well as multiple input and output formats.


### The Goals of This Project

Due to time constraints and the inherent extensibility of the assembler, this repository will focus on implementing the core data structure, the plugin system, a command-line tool, and a small demo plugin and program pair.

#### Demo

As a proof of viability, a small 6502 program will be written and assembled by this framework, and then run on the [Easy 6502 Simulator](https://skilldrick.github.io/easy6502/simulator.html)

See the [6502 demo](../../wiki/QOI-Demo) wiki page

## Project Structure

This repository contains a Visual Studio Solution with 4 projects:

This repository contains a Visual Studio Solution with 5 projects:

| Project | Description |
|---|---|
| [Solar.EntitySystem](/SolarMLAssembler/Solar.EntitySystem)             | The overarching entity system |
| [Engine.Model](/SolarMLAssembler/Solar.Asm/Engine/Model)               | The library which defines the base data model classes |
| [Demo6502](/SolarMLAssembler/Demo)                                     | The plugin implementation for 6502 assembly |
| [DemoCLI](/SolarMLAssembler/Demo)                                      | The plugin implementation for 6502 assembly |

As well as two utility programs written in Python:
| Project | Description |
|---|---|

TODO: Continue

## Installation

This project is built on C# 12.

From source, simply build the `CLI` project. It will generate the following files:\
TODO: Explain what each DLL is

As well as the following folders:\
TODO: Explain Plugins and SystemPlugins

In order to run the Demo, build the `Demo6502` project, and add the generated `Demo6502.plugin.dll` file to the "Plugins" folders.

Other plugins can be added by adding their DLLs to the `Plugins` or `SystemPlugins` folders

## Usage
TBD\
TODO: Show how to use the command-line for basic assembly\
TODO: Show how to hook into the assembler directly (maybe)

## Credits

- mass:werks ([www.masswerk.at](https://www.masswerk.at/)) for their detailed [6502 Instruction Set reference](https://www.masswerk.at/6502/6502_instruction_set.html)
- Nick Morgan ([@skilldrick](https://github.com/skilldrick)) and other contributors of the [easy6502 repository](https://github.com/skilldrick/easy6502)

## Contacts

**Author**\
Sam Pierre-Louis ([spier033@uottawa.ca](mailto:spier033@uottawa.ca))

**Supervisor**\
Prof. Stéphane Some ([Stephane.Some@uottawa.ca](mailto:Stephane.Some@uottawa.ca))
