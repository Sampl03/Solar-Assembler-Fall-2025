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

Due to time constraints and the inherent extensibility of the assembler, this repository focuses on implementing the core data structure and framework entities, the entity system, and a small demo written in 6502 assembly.

#### Demo

As a proof of viability, a small 6502 program was be written and assembled by this framework, which runs on the [Easy 6502 Simulator](https://skilldrick.github.io/easy6502/simulator.html)

See the [6502 demo](../../wiki/demo) wiki page


## Project Structure

This repository contains a Visual Studio Solution with 4 projects:

| Project | Description |
|---|---|
| [Solar.EntitySystem](/SolarMLAssembler/Solar.EntitySystem) | The overarching entity system |
| [Engine.Model](/SolarMLAssembler/Solar.Asm/Engine/Model)   | The library which defines the base data model classes |
| [Demo6502](/SolarMLAssembler/Demo)                         | The plugin implementation for 6502 assembly |
| [DemoCLI](/SolarMLAssembler/DemoCLI)                          | The executable command-line tool which creates and assembles the demo program via the other libraries |

As well as two utility programs written in Python:
| Project | Description |
|---|---|
| [BinToDCB](/BinToDCB/bintodcb.py) | Converts binary files to DCB (declare bytes) statements for the `Easy6502` simulator |
| [ImagePrep](/ImagePrep/) | Creates 32x32px binary image dumps out of compatible images |


## Installation

#### Framework
The core framework is written with C# 12 and is a Visual Studio solution

There are no additional packages to install.

#### Python Utilities
The utilities `ImagePrep` and `BinToDCB` were written using Python 3.12.1

`BinToDCB` requires no additional module.

`ImagePrep` requires [`numpy`](https://pypi.org/project/numpy/) and [`PIL`](https://pypi.org/project/pillow/)


## Usage
This repository contains the core model, the 6502 plugin, a demo, and two Python utilities.

The core model and 6502 plugin are library projects and cannot be run standalone.

#### Demo
*Main page:* [6502 Demo Program](../../wiki/demo)

To run the demo builder, please compile the `DemoCLI` project and run `DemoCLI.exe`. The assembled program will be written to `out/demo6502_image.o`.\
New images can be added to the `Images/` folder, although they must be 32x32 pixel binary dumps compatible with the `Easy6502` screen format.

The assembled demo can be run on the [Easy6502 Simulator](https://skilldrick.github.io/easy6502/simulator.html), but must be converted to a chain of byte
declarations using `BinToDCB`

#### BinToDCB
*Main page:* [6502 Demo - Utilities](../../wiki/demo‐utils#bintodcb)

In order to be executed by the `Easy6502` emulator, the binary object file must be converted to `Declare Byte (DCB)` syntax.\
Please drag `demo6502_image.o` to a folder containing the [bintodcb.py](/BinToDCB/bintodcb.py) script and execute.\
The new file, `demo6502_image.s`, will be loadable in the `Easy6502` and can simply be copy pasted.

#### ImagePrep
For `ImagePrep`, please read [6502 Demo - Utilities](../../wiki/demo‐utils#imageprep)

## Credits

- mass:werks ([www.masswerk.at](https://www.masswerk.at/)) for their detailed [6502 Instruction Set reference](https://www.masswerk.at/6502/6502_instruction_set.html)
- Nick Morgan ([@skilldrick](https://github.com/skilldrick)) and other contributors of the [easy6502 repository](https://github.com/skilldrick/easy6502)
- [Demo Image Credits](/ImagePrep/image_credits.md)

## Contacts

**Author**\
Sam Pierre-Louis ([spier033@uottawa.ca](mailto:spier033@uottawa.ca))

**Supervisor**\
Prof. Stéphane Some ([Stephane.Some@uottawa.ca](mailto:Stephane.Some@uottawa.ca))
