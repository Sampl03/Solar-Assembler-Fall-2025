# Solar Multilingual Assembler

*i.e. Extensible Data-Oriented Framework for Assembly Code Representation and Manipulation*

*This repository was created for the University of Ottawa CSI 4900 class of Fall 2025*\
*It contains a subset of features suitable to the 4 months of the Honours Project, and may be expanded upon at a later date, in a separate repository*

**NOTE:** This README provides a high-level overview. For full technical detail, see the project wiki\
TODO: Add project wiki link

---

The **Solar Multilingual Assembler (SMLA)** project is an experimental assembler framework written in C# with a focus on implementing data structures that facilitate expressivity, flexibility and extensibility.

It takes inspiration from the *Intermediate Representation* (IR) code of contemporary compilers, representing assembly code in a language-agnostic way that can be manipulated by a shared corpus of tools. This allows directives, opcodes, input/output formats and more utilities to be swapped in and out and combined without having to modify the front-end of the compiler.

By doing so, the time cost of creating new tooling while working on custom architectures is severely reduced, and the expressivity of a more complex system is kept between multiple different projects.

The *SMLA* fundamentally parses the **Control and Expression Evaluation Language (CEEL)**, a custom *Domain-Specific Language* designed to control and enhance the assembly process.

It enables:
- Complex mathematical expression evaluation
- Conditional assembly
- Metadata access and mutation
- Reflection and inspection of program state
- Plugin integration
- Embedding of external data in several formats (e.g., binaries, source files, object files)
- Support for multiple output formats (e.g., flat binary, simple disassembly, executable)

Regular assembly, which can itself contain embedded mathematical expressions and directives, is embedded within top-level *CEEL* blocks.

The name **Multilingual** reflects its ability to work with multiple different assembly languages (as defined by an opcode database), with C# for plugins, and with *CEEL* for assembly orchestration.

### The Goals of This Project

Due to time constraints and the inherent extensibility of the assembler, this repository will focus on implementing the core data structure, the *CEEL*-capable front end parser, the plugin system, a command-line tool, and a small demo program.

Note that due to its complexity, the ability to define functions/macros directly in *CEEL* will not be implemented for the Honours Project.

Please see the [Roadmap](#roadmap) for a detailed breakdown.

#### Demo

In order to demo the assembler, a small emulator (see [Emulator](/Emulator/README.md)) capable of running 6502 assembly will be implemented.\
It will utilise a toy executable format, supported by a custom C# plugin.

A snake program (see [Snake](/Snake/README.md)) will be written, assembled, and then run on this emulator.

## Project Structure
TODO: Go over all the folders:\
- SolarMLAssembler (The Core and Console)
- Emulator (The Emulator and its SMLA plugins for 6502 and its custom output format)
- Snake (The Snake game source code)

## Roadmap
TODO: Write down step-by-step goals for 

## Installation
TODO: Show what version of C# to have\
TODO: Show how to download and setup the package\
TODO: Show how to add plugins

## Usage
TBD\
TODO: Show how to use the command-line for basic assembly\
TODO: Show how to hook into the assembler directly (maybe)

## Contacts

**Author**\
Sam Pierre-Louis ([spier033@uottawa.ca](mailto:spier033@uottawa.ca))

**Supervisor**\
Prof. Stéphane Some ([Stephane.Some@uottawa.ca](mailto:Stephane.Some@uottawa.ca))
