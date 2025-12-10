using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.IO;
using Solar.Asm.Engine.Model.Symbols;

namespace Demo.Mos6502
{
    public sealed class Easy6502Formatter : IOutputFormatter
    {
        private const int MAX_ITERATIONS = 256;
        private const int MAX_PADDING_ARRAY_SIZE = 1024;
        private const ulong CODE_START = 0x600;
        private const ulong CODE_END = 0xFFFF;

        private IAssemblyParser _assemblyParser;

        private Easy6502Formatter(IAssemblyParser parser)
        {
            _assemblyParser = parser;
        }

        public static SharedMetaContext Create(IAssemblyParser assemblyParser)
        {
            if (!IsCompatibleWithArchitecture(assemblyParser.ArchSpecs))
                throw new IncompatibleArchitectureException($"OutputFormatter '{nameof(Easy6502Formatter)}' does not support architecture with ID'{assemblyParser.ArchSpecs.ArchitectureIdCode}'");

            return new(
                new Easy6502Formatter(assemblyParser),
                assemblyParser
            );
        }

        public static bool IsCompatibleWithArchitecture(ArchitectureSpecs architectureSpecs) => architectureSpecs == Mos6502Parser.StaticSpecs;

        public Program CreateProgram() => new Mos6502Program(new(this, _assemblyParser));

        /// <summary>
        /// Adjusts the desired addresses of sections by emitting them in order.
        /// This method does not change the order, so they must already be in their final order
        /// </summary>
        /// <remarks>
        /// Due to dynamic emission and address changes, this variable probably won't return a well-assembled byte stream.<br/>
        /// It represents a single iteration towards the "correct" representation
        /// </remarks>
        /// <param name="sections">
        /// The sections to adjust and recalculate. The sections must be in the final order they'll appear in the output
        /// </param>
        /// <returns>
        /// The concatenated byte emissions from all sections, in order.
        /// </returns>
        private IList<byte> ComposeSections(Section[] sections)
        {
            List<byte> iterationResult = [];

            for (int i = 0; i < sections.Length; i++)
            {
                Section section = sections[i];

                // Dynamically recalculate
                IEnumerable<byte> bytes = section.EmitBytes();
                iterationResult.AddRange(bytes);

                // Check if the next section needs adjustment
                if (i + 1 < sections.Length && !sections[i + 1].IsDesiredAddressFixed)
                    sections[i + 1].DesiredAddress = section.DesiredAddress + (ulong)bytes.LongCount();
            }

            return iterationResult;
        }

        public void EmitProgram(Program program, in Stream output)
        {
            using BinaryWriter bw = new(output);

            // First, finalize the program and its symbols
            program.FinalizeSymbols();

            // Order the sections by how they appear in the source code
            var sections = program.CodeEntities.SearchEntities<Section>().OrderBy(s => s.DeclarationOrder).ToArray();

            // Then, split the program into "segments", delimited by fixed sections
            ulong origin = CODE_START;
            foreach (var section in sections)
            {
                if (section.IsDesiredAddressFixed)
                    origin = section.DesiredAddress;
                else
                    section.DesiredAddress = origin;
            }

            // These segments may be out of order, so we group on the address, sort, and ungroup
            sections = sections.GroupBy(s => s.DesiredAddress).OrderBy(sg => sg.Key).SelectMany(sg => sg).ToArray();

            // The order is finalized, so we now iterate over the sections and re-emit until they stabilise (expensive)
            uint iterations_allowed = MAX_ITERATIONS;
            IList<byte> prevIteration = ComposeSections(sections);
            while (iterations_allowed > 0)
            {
                var nextIteration = ComposeSections(sections);

                if (nextIteration.SequenceEqual(prevIteration))
                    break;

                prevIteration.Clear();
                prevIteration = nextIteration;

                iterations_allowed--;
            }

            if (iterations_allowed <= 0)
                throw new CannotOutputException($"Program exceeded {MAX_ITERATIONS} iterations without stabilizing");

            // Emit the bytes to the output stream
            byte[] padding = Enumerable.Repeat<byte>(0x00, MAX_PADDING_ARRAY_SIZE).ToArray(); // useful byte array to make padding more efficient
            ulong currentAddress = CODE_START;
            string prevSection = $"START 0000-{CODE_START:X4}";
            foreach (var section in sections)
            {
                if (currentAddress > section.DesiredAddress)
                    throw new CannotOutputException($"Section '{prevSection}' overlaps with Section '{section.Name}', conflict could not be resolved");

                // Pad between sections
                while (currentAddress < section.DesiredAddress)
                {
                    int gap = Math.Min(padding.Length, (int)(section.DesiredAddress - currentAddress));
                    bw.Write(padding, 0, gap);
                    currentAddress += (ulong)gap;
                }

                // Output the section
                byte[] sectionBytes = section.EmitBytes().ToArray();
                BinaryPatch[] patches = section.EmitPatches(); // Verify that all conditions are met, even if we don't care about the patches
                sectionBytes = _assemblyParser.PatchBytes(currentAddress, sectionBytes, patches);

                ulong sectionLength = (ulong)sectionBytes.Length;

                if (currentAddress + sectionLength > CODE_END)
                    throw new CannotOutputException($"Program exceeded the maximum size allowed ('{currentAddress+sectionLength:X}' > '{CODE_END:X}')");

                bw.Write(sectionBytes);
                currentAddress += sectionLength;

                // Replace the padding with this section's values
                padding = Enumerable.Repeat(section.Flags.PadValue, MAX_PADDING_ARRAY_SIZE / section.Flags.PadValue.Length).SelectMany(x => x).ToArray();

                prevSection = section.Name;
            }
        }

        bool IOutputFormatter.AreSymbolsEquivalent(Symbol symbol1, Symbol symbol2)
        {
            throw new InvalidSymbolException($"OutputerFormatter '{nameof(Easy6502Formatter)}' does not support plugin symbol binding");
        }

        void IOutputFormatter.MergeSymbols(Symbol incoming, Symbol destination)
        {
            throw new InvalidSymbolException($"OutputerFormatter '{nameof(Easy6502Formatter)}' does not support plugin symbol binding");
        }
    }
}
